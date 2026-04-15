using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;
using MikroClean.Domain.MikroTik.Operations.Bridges;
using MikroClean.Domain.MikroTik.Operations.Firewall;
using MikroClean.Domain.MikroTik.Operations.Interfaces;
using MikroClean.Domain.MikroTik.Operations.IpAddresses;
using MikroClean.Domain.MikroTik.Operations.IpPools;
using MikroClean.Domain.MikroTik.Operations.PPPoE;
using MikroClean.Domain.MikroTik.Operations.PPPoE.Profiles;
using MikroClean.Domain.MikroTik.Operations.PPPoE.Secrets;
using MikroClean.Domain.MikroTik.Operations.PPPoE.Servers;
using MikroClean.Domain.MikroTik.Operations.System;
using MikroClean.Domain.MikroTik.Operations.Vlans;
using System.Text.Json;

namespace MikroClean.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para operaciones MikroTik
    /// Orquesta las operaciones entre el connection manager y los repositorios
    /// </summary>
    public class MikroTikService : IMikroTikService
    {
        private readonly TimeSpan _routerQueryTimeout;
        private readonly TimeSpan _routerMutationTimeout;

        private readonly IMikroTikConnectionManager _connectionManager;
        private readonly IRouterRepository _routerRepository;
        
        // Repositorios locales
        private readonly IIpPoolRepository _ipPoolRepository;
        private readonly IPppProfileRepository _pppProfileRepository;
        private readonly IPppSecretRepository _pppSecretRepository;
        private readonly IPppServerRepository _pppServerRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPendingChangeRepository _pendingChangeRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<MikroTikService> _logger;

        public MikroTikService(
            IMikroTikConnectionManager connectionManager,
            IRouterRepository routerRepository,
            IIpPoolRepository ipPoolRepository,
            IPppProfileRepository pppProfileRepository,
            IPppSecretRepository pppSecretRepository,
            IPppServerRepository pppServerRepository,
            ISubscriptionRepository subscriptionRepository,
            IPendingChangeRepository pendingChangeRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<MikroTikService> logger)
        {
            _connectionManager = connectionManager;
            _routerRepository = routerRepository;
            
            _ipPoolRepository = ipPoolRepository;
            _pppProfileRepository = pppProfileRepository;
            _pppSecretRepository = pppSecretRepository;
            _pppServerRepository = pppServerRepository;
            _subscriptionRepository = subscriptionRepository;
            _pendingChangeRepository = pendingChangeRepository;
            _unitOfWork = unitOfWork;

            var queryTimeoutRaw = configuration["MikroTik:QueryTimeoutSeconds"];
            var mutationTimeoutRaw = configuration["MikroTik:MutationTimeoutSeconds"];
            var queryTimeoutSeconds = int.TryParse(queryTimeoutRaw, out var parsedQueryTimeout) ? parsedQueryTimeout : 8;
            var mutationTimeoutSeconds = int.TryParse(mutationTimeoutRaw, out var parsedMutationTimeout) ? parsedMutationTimeout : 6;
            _routerQueryTimeout = TimeSpan.FromSeconds(Math.Max(2, queryTimeoutSeconds));
            _routerMutationTimeout = TimeSpan.FromSeconds(Math.Max(2, mutationTimeoutSeconds));
            
            _logger = logger;
        }

        // ============= GESTIÓN DE CONEXIONES =============

        public async Task<ApiResponse<RouterConnectionStatus>> TestRouterConnectionAsync(int routerId)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                if (router == null)
                {
                    return ApiResponse<RouterConnectionStatus>.NotFound("Router no encontrado");
                }

                if (!router.IsActive)
                {
                    return ApiResponse<RouterConnectionStatus>.Error("El router está inactivo");
                }

                var isConnected = await _connectionManager.TestConnectionAsync(routerId);
                var status = await _connectionManager.GetConnectionStatusAsync(routerId);

                return ApiResponse<RouterConnectionStatus>.Success(
                    status,
                    isConnected ? "Conexión exitosa" : "No se pudo conectar al router"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error probando conexión con router {RouterId}", routerId);
                return ApiResponse<RouterConnectionStatus>.Error($"Error al probar conexión: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RouterConnectionStatus>> GetRouterStatusAsync(int routerId)
        {
            try
            {
                var status = await _connectionManager.GetConnectionStatusAsync(routerId);
                return ApiResponse<RouterConnectionStatus>.Success(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estado de router {RouterId}", routerId);
                return ApiResponse<RouterConnectionStatus>.Error($"Error al obtener estado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<Dictionary<int, bool>>> WarmUpOrganizationConnectionsAsync(int organizationId)
        {
            try
            {
                await _connectionManager.WarmUpConnectionsAsync(organizationId);

                var routers = await _routerRepository.GetAvailableRoutersAsync(organizationId);
                var results = new Dictionary<int, bool>();

                foreach (var router in routers)
                {
                    var isConnected = await _connectionManager.TestConnectionAsync(router.Id);
                    results[router.Id] = isConnected;
                }

                var successCount = results.Values.Count(v => v);
                return ApiResponse<Dictionary<int, bool>>.Success(
                    results,
                    $"Conexiones pre-calentadas: {successCount}/{results.Count} exitosas"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pre-calentando conexiones de organización {OrganizationId}", organizationId);
                return ApiResponse<Dictionary<int, bool>>.Error($"Error al pre-calentar conexiones: {ex.Message}");
            }
        }

        public async Task ProcessPendingChangesAsync(CancellationToken cancellationToken = default)
        {
            var routerIds = await _pendingChangeRepository.GetRouterIdsWithReadyPendingAsync(200);

            if (routerIds.Count > 0)
            {
                _logger.LogInformation("Procesando pending changes para {RouterCount} routers", routerIds.Count);
            }

            foreach (var routerId in routerIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessPendingChangesIfOnlineAsync(routerId, cancellationToken);
            }
        }

        // ============= INTERFACES =============

        public async Task<ApiResponse<BridgeResponse>> CreateBridgeAsync(int routerId, CreateBridgeRequest request)
        {
            try
            {
                var operation = new CreateBridgeOperation();
                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

                if (!result.IsSuccess)
                {
                    return ApiResponse<BridgeResponse>.Error(
                        $"Error creando bridge: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<BridgeResponse>.Success(result.Data!, "Bridge creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando bridge en router {RouterId}", routerId);
                return ApiResponse<BridgeResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<VlanResponse>> CreateVlanAsync(int routerId, CreateVlanRequest request)
        {
            try
            {
                var operation = new CreateVlanOperation();
                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

                if (!result.IsSuccess)
                {
                    return ApiResponse<VlanResponse>.Error(
                        $"Error creando VLAN: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<VlanResponse>.Success(result.Data!, "VLAN creada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando VLAN en router {RouterId}", routerId);
                return ApiResponse<VlanResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<InterfaceResponse>>> GetAllInterfacesAsync(int routerId)
        {
            try
            {
                var query = new GetAllInterfacesQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

                if (!result.IsSuccess)
                {
                    return ApiResponse<List<InterfaceResponse>>.Error(
                        $"Error obteniendo interfaces: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<List<InterfaceResponse>>.Success(
                    result.Data!,
                    $"Se encontraron {result.Data!.Count} interfaces"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo interfaces de router {RouterId}", routerId);
                return ApiResponse<List<InterfaceResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        // ============= IP ADDRESS =============

        public async Task<ApiResponse<IpAddressResponse>> CreateIpAddressAsync(int routerId, CreateIpAddressRequest request)
        {
            try
            {
                var operation = new CreateIpAddressOperation();
                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

                if (!result.IsSuccess)
                {
                    return ApiResponse<IpAddressResponse>.Error(
                        $"Error agregando dirección IP: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<IpAddressResponse>.Success(result.Data!, "Dirección IP agregada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando IP en router {RouterId}", routerId);
                return ApiResponse<IpAddressResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        // ============= FIREWALL =============

        public async Task<ApiResponse<FirewallRuleResponse>> CreateFirewallRuleAsync(int routerId, CreateFirewallRuleRequest request)
        {
            try
            {
                var operation = new CreateFirewallRuleOperation();
                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

                if (!result.IsSuccess)
                {
                    return ApiResponse<FirewallRuleResponse>.Error(
                        $"Error creando regla de firewall: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<FirewallRuleResponse>.Success(result.Data!, "Regla de firewall creada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando regla de firewall en router {RouterId}", routerId);
                return ApiResponse<FirewallRuleResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        // ============= SYSTEM INFO =============

        public async Task<ApiResponse<SystemResourceResponse>> GetSystemResourcesAsync(int routerId)
        {
            try
            {
                var query = new GetSystemResourceQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

                if (!result.IsSuccess)
                {
                    return ApiResponse<SystemResourceResponse>.Error(
                        $"Error obteniendo recursos del sistema: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<SystemResourceResponse>.Success(
                    result.Data!,
                    "Información del sistema obtenida exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recursos del router {RouterId}", routerId);
                return ApiResponse<SystemResourceResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ResourcesRouterResponse>> GetResourcesRouterAsync(int routerId)
        {
            try
            {
                var query = new GetResourcesRouterQuery();
                var result = await ExecuteQueryWithTimeoutAsync(routerId, query);

                if (!result.IsSuccess)
                {
                    return ApiResponse<ResourcesRouterResponse>.Error(
                        $"Error obteniendo recursos del router: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<ResourcesRouterResponse>.Success(result.Data!, "Recursos del router obtenidos exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recursos del router {RouterId}", routerId);
                return ApiResponse<ResourcesRouterResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        // ============= OPERACIONES EN BATCH =============

        public async Task<ApiResponse<Dictionary<int, MikroTikResult<TResponse>>>> ExecuteOnMultipleRoutersAsync<TRequest, TResponse>(
            int organizationId,
            IMikroTikOperation<TRequest, TResponse> operation,
            TRequest request)
        {
            try
            {
                var routers = await _routerRepository.GetAvailableRoutersAsync(organizationId);
                if (!routers.Any())
                {
                    return ApiResponse<Dictionary<int, MikroTikResult<TResponse>>>.NotFound(
                        "No se encontraron routers activos para esta organización"
                    );
                }

                var results = new Dictionary<int, MikroTikResult<TResponse>>();
                var tasks = routers.Select(async router =>
                {
                    var result = await _connectionManager.ExecuteOperationAsync(router.Id, operation, request);
                    lock (results)
                    {
                        results[router.Id] = result;
                    }
                });

                await Task.WhenAll(tasks);

                var successCount = results.Values.Count(r => r.IsSuccess);
                var totalCount = results.Count;

                return ApiResponse<Dictionary<int, MikroTikResult<TResponse>>>.Success(
                    results,
                    $"Operación ejecutada en {successCount}/{totalCount} routers exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error ejecutando operación en múltiples routers de organización {OrganizationId}",
                    organizationId
                );
                return ApiResponse<Dictionary<int, MikroTikResult<TResponse>>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<IpPoolResponse>>> GetIpPoolsPagedAsync(int routerId, PaginationParams paginationParams)
        {
            try
            {
                var (syncedFromRouter, offlineRouter) = await TrySyncIpPoolsFromRouterAsync(routerId);
                var dbPools = (await _ipPoolRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var allPools = dbPools.Select(MapIpPoolEntityToResponse).ToList();

                // Filtrado (SearchTerm)
                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var term = paginationParams.SearchTerm.ToLower();
                    allPools = allPools.Where(p => 
                        p.Name.ToLower().Contains(term) || 
                        p.Ranges.ToLower().Contains(term) || 
                        (p.NextPool != null && p.NextPool.ToLower().Contains(term)) ||
                        (p.Comment != null && p.Comment.ToLower().Contains(term))
                    ).ToList();
                }

                // Ordenamiento
                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    var propertyInfo = typeof(IpPoolResponse).GetProperty(paginationParams.SortBy, 
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (propertyInfo != null)
                    {
                        allPools = paginationParams.SortDescending 
                            ? allPools.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList()
                            : allPools.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                    }
                }
                else
                {
                    allPools = allPools.OrderBy(p => p.Name).ToList();
                }

                var totalCount = allPools.Count;
                var pagedItems = allPools
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var pagedResult = new PagedResult<IpPoolResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<IpPoolResponse>>.Success(
                    pagedResult,
                    offlineRouter
                        ? $"Router offline: mostrando {totalCount} pools desde base de datos local (Pgina {paginationParams.PageNumber})"
                        : syncedFromRouter
                            ? $"Se encontraron {totalCount} pools de IP (sincronizados, Pgina {paginationParams.PageNumber})"
                            : $"Se encontraron {totalCount} pools de IP desde base de datos local (Pgina {paginationParams.PageNumber})"
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error obteniendo pools de IP paginados del router {RouterId}", routerId);
                return ApiResponse<PagedResult<IpPoolResponse>>.Error($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResponse<IpPoolResponse>> CreateIpPoolAsync(int routerId, CreateIpPoolRequest createPoolRequest)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllIpPoolsQuery();
                var existingPoolsResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingPoolsResult.IsSuccess && existingPoolsResult.Data != null)
                {
                    if (existingPoolsResult.Data.Any(p => p.Name.Equals(createPoolRequest.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ApiResponse<IpPoolResponse>.Error($"Ya existe un IP Pool con el nombre '{createPoolRequest.Name}'");
                    }
                }

                var result = await ExecuteMutationWithTimeoutAsync(routerId, new CreateIpPoolOperation(), createPoolRequest);

                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localPool = await ApplyLocalIpPoolCreateAsync(routerId, createPoolRequest);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.IpPool, PendingChangeOperation.Create, createPoolRequest, localPool.Id, result.ErrorMessage);
                        return ApiResponse<IpPoolResponse>.Warning("Router offline: pool creado localmente y guardado como pendiente para sincronizar", localPool);
                    }

                    return ApiResponse<IpPoolResponse>.Error($"Error creando IP pool: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                }

                // Confirmar creacin y obtener datos completos
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de pools despus de crear uno nuevo en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<IpPoolResponse>.Warning("Pool creado pero no se pudo recuperar la lista de pools para confirmar", null);
                }

                var createdPool = getResult.Data!.FirstOrDefault(p => p.Name == createPoolRequest.Name);
                if (createdPool == null)
                {
                    _logger.LogWarning("No se pudo encontrar el pool recin creado con nombre {PoolName} en router {RouterId} despus de la creacin", createPoolRequest.Name, routerId);
                    return ApiResponse<IpPoolResponse>.Warning("Pool creado pero no se pudo confirmar su existencia en la lista de pools", null);
                }

                if (!await SyncIpPoolsToDatabaseAsync(routerId, getResult.Data!))
                {
                    return ApiResponse<IpPoolResponse>.Warning("IP Pool creado en MikroTik, pero fall la sincronizacin en base de datos", createdPool);
                }

                return ApiResponse<IpPoolResponse>.Success(createdPool, "IP Pool creado exitosamente");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creando IP pool en router {RouterId}", routerId);
                return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {e.Message}");
            }
        }

        public async Task<ApiResponse<IpPoolResponse>> UpdateIpPoolAsync(int routerId, UpdateIpPoolRequest updateIpPoolRequest)
        {
            try
            {
                // Validacin: No permitir nombres duplicados (si se est cambiando el nombre)
                var query = new GetAllIpPoolsQuery();
                var existingPoolsResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingPoolsResult.IsSuccess && existingPoolsResult.Data != null && !string.IsNullOrEmpty(updateIpPoolRequest.Name))
                {
                    if (existingPoolsResult.Data.Any(p => p.Name.Equals(updateIpPoolRequest.Name, StringComparison.OrdinalIgnoreCase) && p.Id != updateIpPoolRequest.Id))
                    {
                        return ApiResponse<IpPoolResponse>.Error($"Ya existe otro IP Pool con el nombre '{updateIpPoolRequest.Name}'");
                    }
                }

                var result = await ExecuteMutationWithTimeoutAsync(routerId, new UpdateIpPoolOperation(), updateIpPoolRequest);

                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localPool = await ApplyLocalIpPoolUpdateAsync(routerId, updateIpPoolRequest);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.IpPool, PendingChangeOperation.Update, updateIpPoolRequest, updateIpPoolRequest.Id, result.ErrorMessage);
                        return ApiResponse<IpPoolResponse>.Warning("Router offline: pool actualizado localmente y guardado como pendiente para sincronizar", localPool);
                    }

                    return ApiResponse<IpPoolResponse>.Error($"Error actualizando IP pool: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                }

                // Confirmar actualizacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);

                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de pools despus de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<IpPoolResponse>.Warning("Pool actualizado pero no se pudo recuperar la lista de pools para confirmar", null);
                }

                var updatedPool = getResult.Data!.FirstOrDefault(p => p.Id == updateIpPoolRequest.Id);
                if (updatedPool != null)
                {
                    if (!await SyncIpPoolsToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<IpPoolResponse>.Warning("IP Pool actualizado en MikroTik, pero fall la sincronizacin en base de datos", updatedPool);
                    }

                    return ApiResponse<IpPoolResponse>.Success(updatedPool, "IP Pool actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el pool actualizado con ID {PoolId} en router {RouterId} despus de la actualizacin", updateIpPoolRequest.Id, routerId);
                    return ApiResponse<IpPoolResponse>.Warning("Pool actualizado pero no se pudo confirmar su existencia en la lista de pools", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando IP pool en router {RouterId}", routerId);
                return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IpPoolResponse>> DeleteIpPoolAsync(int routerId, DeleteIpPoolRequest request)
        {
            try
            {
                var result = await ExecuteMutationWithTimeoutAsync(routerId, new DeleteIpPoolOperation(), request);

                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        await ApplyLocalIpPoolDeleteAsync(routerId, request.Id);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.IpPool, PendingChangeOperation.Delete, request, request.Id, result.ErrorMessage);
                        return ApiResponse<IpPoolResponse>.Warning("Router offline: pool eliminado localmente y guardado como pendiente para sincronizar", new IpPoolResponse { Id = request.Id });
                    }

                    return ApiResponse<IpPoolResponse>.Error($"Error eliminando IP pool: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                }

                var query = new GetAllIpPoolsQuery();
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de pools despus de eliminar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<IpPoolResponse>.Warning("IP Pool eliminado en MikroTik, pero no se pudo sincronizar la base de datos", new IpPoolResponse { Id = request.Id });
                }

                if (!await SyncIpPoolsToDatabaseAsync(routerId, getResult.Data!))
                {
                    return ApiResponse<IpPoolResponse>.Warning("IP Pool eliminado en MikroTik, pero fall la sincronizacin en base de datos", new IpPoolResponse { Id = request.Id });
                }

                // /remove no retorna datos - devolvemos el Id que fue eliminado
                return ApiResponse<IpPoolResponse>.Success(new IpPoolResponse { Id = request.Id }, "IP Pool eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando IP pool en router {RouterId}", routerId);
                return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<PPPoEProfileResponse>>> GetPPPoEProfilesPagedAsync(int routerId, PaginationParams paginationParams)
        {
            try
            {
                var (syncedFromRouter, offlineRouter) = await TrySyncPppProfilesFromRouterAsync(routerId);
                var dbProfiles = (await _pppProfileRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var allProfiles = dbProfiles.Select(MapPppProfileEntityToResponse).ToList();

                // Filtrado (SearchTerm)
                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var term = paginationParams.SearchTerm.ToLower();
                    allProfiles = allProfiles.Where(p => 
                        p.Name.ToLower().Contains(term) || 
                        p.LocalAddress.ToLower().Contains(term) || 
                        p.RemoteAddress.ToLower().Contains(term) ||
                        (p.Comment != null && p.Comment.ToLower().Contains(term))
                    ).ToList();
                }

                // Ordenamiento
                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    var propertyInfo = typeof(PPPoEProfileResponse).GetProperty(paginationParams.SortBy, 
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (propertyInfo != null)
                    {
                        allProfiles = paginationParams.SortDescending 
                            ? allProfiles.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList()
                            : allProfiles.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                    }
                }
                else
                {
                    allProfiles = allProfiles.OrderBy(p => p.Name).ToList();
                }

                var totalCount = allProfiles.Count;
                var pagedItems = allProfiles
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var pagedResult = new PagedResult<PPPoEProfileResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<PPPoEProfileResponse>>.Success(
                    pagedResult,
                    offlineRouter
                        ? $"Router offline: mostrando {totalCount} perfiles PPPoE desde base de datos local (Pgina {paginationParams.PageNumber})"
                        : syncedFromRouter
                            ? $"Se encontraron {totalCount} perfiles PPPoE (sincronizados, Pgina {paginationParams.PageNumber})"
                            : $"Se encontraron {totalCount} perfiles PPPoE desde base de datos local (Pgina {paginationParams.PageNumber})"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo perfiles PPPoE paginados del router {RouterId}", routerId);
                return ApiResponse<PagedResult<PPPoEProfileResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEProfileResponse>> CreatePPPoEProfileAsync(int routerId, CreatePPPoEProfile createPPPoEProfile)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllPppProfilesQuery();
                var existingProfilesResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingProfilesResult.IsSuccess && existingProfilesResult.Data != null)
                {
                    if (existingProfilesResult.Data.Any(p => p.Name.Equals(createPPPoEProfile.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ApiResponse<PPPoEProfileResponse>.Error($"Ya existe un perfil PPPoE con el nombre '{createPPPoEProfile.Name}'");
                    }
                }

                var operation = new CreatePppProfileOperation();
                var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, createPPPoEProfile);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localProfile = await ApplyLocalPppProfileCreateAsync(routerId, createPPPoEProfile);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppProfile, PendingChangeOperation.Create, createPPPoEProfile, localProfile.Id, result.ErrorMessage);
                        return ApiResponse<PPPoEProfileResponse>.Warning("Router offline: perfil creado localmente y guardado como pendiente para sincronizar", localProfile);
                    }

                    return ApiResponse<PPPoEProfileResponse>.Error(
                        $"Error creando perfil PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                // Confirmar creacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de perfiles PPPoE despus de crear uno nuevo en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE creado pero no se pudo recuperar la lista para confirmar", null);
                }

                var createdProfile = getResult.Data!.FirstOrDefault(p => p.Name == createPPPoEProfile.Name);
                if (createdProfile != null)
                {
                    if (!await SyncPppProfilesToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE creado en MikroTik, pero fall la sincronizacin en base de datos", createdProfile);
                    }

                    return ApiResponse<PPPoEProfileResponse>.Success(createdProfile, "Perfil PPPoE creado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el perfil PPPoE recin creado con nombre {ProfileName} en router {RouterId} despus de la creacin", createPPPoEProfile.Name, routerId);
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE creado pero no se pudo confirmar su existencia en la lista", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando perfil PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEProfileResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEProfileResponse>> UpdatePPPoEProfileAsync(int routerId, UpdatePPPoEProfile updatePPPoEProfile)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllPppProfilesQuery();
                var existingProfilesResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingProfilesResult.IsSuccess && existingProfilesResult.Data != null && !string.IsNullOrEmpty(updatePPPoEProfile.Name))
                {
                    if (existingProfilesResult.Data.Any(p => p.Name.Equals(updatePPPoEProfile.Name, StringComparison.OrdinalIgnoreCase) && p.Id != updatePPPoEProfile.Id))
                    {
                        return ApiResponse<PPPoEProfileResponse>.Error($"Ya existe otro perfil PPPoE con el nombre '{updatePPPoEProfile.Name}'");
                    }
                }

                var operation = new UpdatePppProfileOperation();
                var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, updatePPPoEProfile);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localProfile = await ApplyLocalPppProfileUpdateAsync(routerId, updatePPPoEProfile);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppProfile, PendingChangeOperation.Update, updatePPPoEProfile, updatePPPoEProfile.Id, result.ErrorMessage);
                        return ApiResponse<PPPoEProfileResponse>.Warning("Router offline: perfil actualizado localmente y guardado como pendiente para sincronizar", localProfile);
                    }

                    return ApiResponse<PPPoEProfileResponse>.Error(
                        $"Error actualizando perfil PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                
                // Confirmar actualizacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de perfiles PPPoE despus de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE actualizado pero no se pudo recuperar la lista para confirmar", null);
                }

                var updatedProfile = getResult.Data!.FirstOrDefault(p => p.Id == updatePPPoEProfile.Id);
                if (updatedProfile != null)
                {
                    if (!await SyncPppProfilesToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE actualizado en MikroTik, pero fall la sincronizacin en base de datos", updatedProfile);
                    }

                    return ApiResponse<PPPoEProfileResponse>.Success(updatedProfile, "Perfil PPPoE actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el perfil PPPoE actualizado con ID {ProfileId} en router {RouterId} despus de la actualizacin", updatePPPoEProfile.Id, routerId);
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE actualizado pero no se pudo confirmar su existencia en la lista", null);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error actualizando perfil PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEProfileResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEProfileResponse>> DeletePPPoEProfileAsync(int routerId, DeletePPPoEProfile request)
        {
            try
            {
                var result = await ExecuteMutationWithTimeoutAsync(routerId, new DeletePppProfileOperation(), request);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        await ApplyLocalPppProfileDeleteAsync(routerId, request.Id);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppProfile, PendingChangeOperation.Delete, request, request.Id, result.ErrorMessage);
                        return ApiResponse<PPPoEProfileResponse>.Warning("Router offline: perfil eliminado localmente y guardado como pendiente para sincronizar", new PPPoEProfileResponse { Id = request.Id });
                    }

                    return ApiResponse<PPPoEProfileResponse>.Error($"Error eliminando perfil PPPoE: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                }

                var query = new GetAllPppProfilesQuery();
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de perfiles PPPoE despus de eliminar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE eliminado en MikroTik, pero no se pudo sincronizar la base de datos", new PPPoEProfileResponse { Id = request.Id });
                }

                if (!await SyncPppProfilesToDatabaseAsync(routerId, getResult.Data!))
                {
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE eliminado en MikroTik, pero fall la sincronizacin en base de datos", new PPPoEProfileResponse { Id = request.Id });
                }

                return ApiResponse<PPPoEProfileResponse>.Success(new PPPoEProfileResponse { Id = request.Id }, "Perfil PPPoE eliminado exitosamente");

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error eliminando perfil PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEProfileResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResult<PPPoESecretResponse>>> GetPPPoESecretsPagedAsync(int routerId, PaginationParams paginationParams)
        {
            try
            {
                var (syncedFromRouter, offlineRouter) = await TrySyncPppSecretsFromRouterAsync(routerId);
                var dbSecrets = (await _pppSecretRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var allSecrets = dbSecrets.Select(MapPppSecretEntityToResponse).ToList();

                // Filtrado (SearchTerm)
                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var term = paginationParams.SearchTerm.ToLower();
                    allSecrets = allSecrets.Where(s => 
                        s.Name.ToLower().Contains(term) || 
                        s.Service.ToLower().Contains(term) || 
                        s.Profile.ToLower().Contains(term) ||
                        (s.Comment != null && s.Comment.ToLower().Contains(term))
                    ).ToList();
                }

                // Ordenamiento
                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    var propertyInfo = typeof(PPPoESecretResponse).GetProperty(paginationParams.SortBy, 
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (propertyInfo != null)
                    {
                        allSecrets = paginationParams.SortDescending 
                            ? allSecrets.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList()
                            : allSecrets.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                    }
                }
                else
                {
                    // Orden por defecto
                    allSecrets = allSecrets.OrderBy(s => s.Name).ToList();
                }

                var totalCount = allSecrets.Count;
                var pagedItems = allSecrets
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var pagedResult = new PagedResult<PPPoESecretResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<PPPoESecretResponse>>.Success(
                    pagedResult,
                    offlineRouter
                        ? $"Router offline: mostrando {totalCount} secretos PPPoE desde base de datos local (Pgina {paginationParams.PageNumber})"
                        : syncedFromRouter
                            ? $"Se encontraron {totalCount} secretos PPPoE (sincronizados, Pgina {paginationParams.PageNumber})"
                            : $"Se encontraron {totalCount} secretos PPPoE desde base de datos local (Pgina {paginationParams.PageNumber})"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo secretos PPPoE paginados del router {RouterId}", routerId);
                return ApiResponse<PagedResult<PPPoESecretResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoESecretResponse>> CreatePPPoESecretAsync(int routerId, CreatePPPoESecretRequest createPPPoESecret)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllPppSecretsQuery();
                var existingSecretsResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingSecretsResult.IsSuccess && existingSecretsResult.Data != null)
                {
                    if (existingSecretsResult.Data.Any(s => s.Name.Equals(createPPPoESecret.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ApiResponse<PPPoESecretResponse>.Error($"Ya existe un PPPoE Secret con el nombre '{createPPPoESecret.Name}'");
                    }
                }

                var operation = new CreatePppSecretOperation();
                var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, createPPPoESecret);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localSecret = await ApplyLocalPppSecretCreateAsync(routerId, createPPPoESecret);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppSecret, PendingChangeOperation.Create, createPPPoESecret, localSecret.Id, result.ErrorMessage);
                        return ApiResponse<PPPoESecretResponse>.Warning("Router offline: secreto creado localmente y guardado como pendiente para sincronizar", localSecret);
                    }

                    return ApiResponse<PPPoESecretResponse>.Error(
                        $"Error creando secreto PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                
                // Confirmar creacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de secretos PPPoE despus de crear uno nuevo en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se cre el secreto PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }
                
                var createdSecret = getResult.Data!.FirstOrDefault(s => s.Name == createPPPoESecret.Name);
                if (createdSecret != null)
                {
                    if (!await SyncPppSecretsToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<PPPoESecretResponse>.Warning("Secreto PPPoE creado en MikroTik, pero fall la sincronizacin en base de datos", createdSecret);
                    }

                    return ApiResponse<PPPoESecretResponse>.Success(createdSecret, "Secreto PPPoE creado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el secreto PPPoE recin creado con nombre {SecretName} en router {RouterId} despus de la creacin", createPPPoESecret.Name, routerId);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se cre el secreto PPPoE pero no se pudo confirmar su existencia en la lista", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando secreto PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoESecretResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoESecretResponse>> UpdatePPPoESecretAsync(int routerId, UpdatePPPoESecretRequest updatePPPoESecret)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllPppSecretsQuery();
                var existingSecretsResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingSecretsResult.IsSuccess && existingSecretsResult.Data != null && !string.IsNullOrEmpty(updatePPPoESecret.Name))
                {
                    if (existingSecretsResult.Data.Any(s => s.Name.Equals(updatePPPoESecret.Name, StringComparison.OrdinalIgnoreCase) && s.Id != updatePPPoESecret.Id))
                    {
                        return ApiResponse<PPPoESecretResponse>.Error($"Ya existe otro PPPoE Secret con el nombre '{updatePPPoESecret.Name}'");
                    }
                }

                var operation = new UpdatePppSecretOperation();
                var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, updatePPPoESecret);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localSecret = await ApplyLocalPppSecretUpdateAsync(routerId, updatePPPoESecret);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppSecret, PendingChangeOperation.Update, updatePPPoESecret, updatePPPoESecret.Id, result.ErrorMessage);
                        return ApiResponse<PPPoESecretResponse>.Warning("Router offline: secreto actualizado localmente y guardado como pendiente para sincronizar", localSecret);
                    }

                    return ApiResponse<PPPoESecretResponse>.Error(
                        $"Error actualizando secreto PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                
                // Confirmar actualizacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de secretos PPPoE despus de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se actualiz el secreto PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }
                
                var updatedSecret = getResult.Data!.FirstOrDefault(s => s.Id == updatePPPoESecret.Id);
                if (updatedSecret != null)
                {
                    if (!await SyncPppSecretsToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<PPPoESecretResponse>.Warning("Secreto PPPoE actualizado en MikroTik, pero fall la sincronizacin en base de datos", updatedSecret);
                    }

                    return ApiResponse<PPPoESecretResponse>.Success(updatedSecret, "Secreto PPPoE actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el secreto PPPoE actualizado con ID {SecretId} en router {RouterId} despus de la actualizacin", updatePPPoESecret.Id, routerId);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se actualiz el secreto PPPoE pero no se pudo confirmar su existencia en la lista", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando secreto PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoESecretResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoESecretResponse>> DeletePPPoESecretAsync(int routerId, DeletePPPoESecretRequest request)
        {
            try
            {
                var result = await ExecuteMutationWithTimeoutAsync(routerId, new DeletePppSecretOperation(), request);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        await ApplyLocalPppSecretDeleteAsync(routerId, request.Id);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppSecret, PendingChangeOperation.Delete, request, request.Id, result.ErrorMessage);
                        return ApiResponse<PPPoESecretResponse>.Warning("Router offline: secreto eliminado localmente y guardado como pendiente para sincronizar", new PPPoESecretResponse { Id = request.Id });
                    }

                    return ApiResponse<PPPoESecretResponse>.Error($"Error eliminando secreto PPPoE: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                }

                var query = new GetAllPppSecretsQuery();
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de secretos PPPoE despus de eliminar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoESecretResponse>.Warning("Secreto PPPoE eliminado en MikroTik, pero no se pudo sincronizar la base de datos", new PPPoESecretResponse { Id = request.Id });
                }

                if (!await SyncPppSecretsToDatabaseAsync(routerId, getResult.Data!))
                {
                    return ApiResponse<PPPoESecretResponse>.Warning("Secreto PPPoE eliminado en MikroTik, pero fall la sincronizacin en base de datos", new PPPoESecretResponse { Id = request.Id });
                }

                return ApiResponse<PPPoESecretResponse>.Success(new PPPoESecretResponse { Id = request.Id }, "Secreto PPPoE eliminado exitosamente");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando secreto PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoESecretResponse>.Error($"Error inesperado: {ex.Message}");

            }
        }

        public async Task<ApiResponse<PagedResult<PPPoEServerResponse>>> GetPPPoEServersPagedAsync(int routerId, PaginationParams paginationParams)
        {
            try
            {
                var (syncedFromRouter, offlineRouter) = await TrySyncPppServersFromRouterAsync(routerId);
                var dbServers = (await _pppServerRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var allServers = dbServers.Select(MapPppServerEntityToResponse).ToList();

                // Filtrado (SearchTerm)
                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var term = paginationParams.SearchTerm.ToLower();
                    allServers = allServers.Where(s => 
                        s.Name.ToLower().Contains(term) || 
                        s.Interface.ToLower().Contains(term) || 
                        s.Profile.ToLower().Contains(term)
                    ).ToList();
                }

                // Ordenamiento
                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    var propertyInfo = typeof(PPPoEServerResponse).GetProperty(paginationParams.SortBy, 
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                    if (propertyInfo != null)
                    {
                        allServers = paginationParams.SortDescending 
                            ? allServers.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList()
                            : allServers.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                    }
                }
                else
                {
                    allServers = allServers.OrderBy(s => s.Name).ToList();
                }

                var totalCount = allServers.Count;
                var pagedItems = allServers
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var pagedResult = new PagedResult<PPPoEServerResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<PPPoEServerResponse>>.Success(
                    pagedResult,
                    offlineRouter
                        ? $"Router offline: mostrando {totalCount} servidores PPPoE desde base de datos local (Pgina {paginationParams.PageNumber})"
                        : syncedFromRouter
                            ? $"Se encontraron {totalCount} servidores PPPoE (sincronizados, Pgina {paginationParams.PageNumber})"
                            : $"Se encontraron {totalCount} servidores PPPoE desde base de datos local (Pgina {paginationParams.PageNumber})"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo servidores PPPoE paginados del router {RouterId}", routerId);
                return ApiResponse<PagedResult<PPPoEServerResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEServerResponse>> CreatePPPoEServerAsync(int routerId, CreatePPPoEServerRequest createPPPoEServer)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllPppServersQuery();
                var existingServersResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingServersResult.IsSuccess && existingServersResult.Data != null)
                {
                    if (existingServersResult.Data.Any(s => s.Name.Equals(createPPPoEServer.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ApiResponse<PPPoEServerResponse>.Error($"Ya existe un servidor PPPoE con el nombre '{createPPPoEServer.Name}'");
                    }
                }

                var operation = new CreatePppServerOperation();
                var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, createPPPoEServer);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localServer = await ApplyLocalPppServerCreateAsync(routerId, createPPPoEServer);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppServer, PendingChangeOperation.Create, createPPPoEServer, localServer.Id, result.ErrorMessage);
                        return ApiResponse<PPPoEServerResponse>.Warning("Router offline: servidor creado localmente y guardado como pendiente para sincronizar", localServer);
                    }

                    return ApiResponse<PPPoEServerResponse>.Error(
                        $"Error creando servidor PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                // Confirmar creacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de servidores PPPoE despus de crear uno nuevo en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se cre el servidor PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }

                var createdServer = getResult.Data!.FirstOrDefault(s => s.Name == createPPPoEServer.Name);
                if (createdServer != null)
                {
                    if (!await SyncPppServersToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<PPPoEServerResponse>.Warning("Servidor PPPoE creado en MikroTik, pero fall la sincronizacin en base de datos", createdServer);
                    }

                    return ApiResponse<PPPoEServerResponse>.Success(createdServer, "Servidor PPPoE creado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el servidor PPPoE recin creado con nombre {ServerName} en router {RouterId} despus de la creacin", createPPPoEServer.Name, routerId);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se cre el servidor PPPoE pero no se pudo confirmar su existencia en la lista", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando servidor PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEServerResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEServerResponse>> UpdatePPPoEServerAsync(int routerId, UpdatePPPoEServerRequest updatePPPoEServer)
        {
            try
            {
                // Validacin: No permitir nombres duplicados
                var query = new GetAllPppServersQuery();
                var existingServersResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                
                if (existingServersResult.IsSuccess && existingServersResult.Data != null && !string.IsNullOrEmpty(updatePPPoEServer.Name))
                {
                    if (existingServersResult.Data.Any(s => s.Name.Equals(updatePPPoEServer.Name, StringComparison.OrdinalIgnoreCase) && s.Id != updatePPPoEServer.Id))
                    {
                        return ApiResponse<PPPoEServerResponse>.Error($"Ya existe otro servidor PPPoE con el nombre '{updatePPPoEServer.Name}'");
                    }
                }

                var operation = new UpdatePppServerOperation();
                var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, updatePPPoEServer);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        var localServer = await ApplyLocalPppServerUpdateAsync(routerId, updatePPPoEServer);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppServer, PendingChangeOperation.Update, updatePPPoEServer, updatePPPoEServer.Id, result.ErrorMessage);
                        return ApiResponse<PPPoEServerResponse>.Warning("Router offline: servidor actualizado localmente y guardado como pendiente para sincronizar", localServer);
                    }

                    return ApiResponse<PPPoEServerResponse>.Error(
                        $"Error actualizando servidor PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                
                // Confirmar actualizacin
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de servidores PPPoE despus de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se actualiz el servidor PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }

                var updatedServer = getResult.Data!.FirstOrDefault(s => s.Id == updatePPPoEServer.Id);
                if (updatedServer != null)
                {
                    if (!await SyncPppServersToDatabaseAsync(routerId, getResult.Data!))
                    {
                        return ApiResponse<PPPoEServerResponse>.Warning("Servidor PPPoE actualizado en MikroTik, pero fall la sincronizacin en base de datos", updatedServer);
                    }

                    return ApiResponse<PPPoEServerResponse>.Success(updatedServer, "Servidor PPPoE actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el servidor PPPoE actualizado con ID {ServerId} en router {RouterId} despus de la actualizacin", updatePPPoEServer.Id, routerId);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se actualiz el servidor PPPoE pero no se pudo confirmar su existencia en la lista", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando servidor PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEServerResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEServerResponse>> DeletePPPoEServerAsync(int routerId, DeletePPPoEServerRequest request)
        {
            try
            {
                var result = await ExecuteMutationWithTimeoutAsync(routerId, new DeletePppServerOperation(), request);
                if (!result.IsSuccess)
                {
                    if (IsRouterOfflineError(result.ErrorType))
                    {
                        await ApplyLocalPppServerDeleteAsync(routerId, request.Id);
                        await QueuePendingChangeAsync(routerId, PendingChangeResource.PppServer, PendingChangeOperation.Delete, request, request.Id, result.ErrorMessage);
                        return ApiResponse<PPPoEServerResponse>.Warning("Router offline: servidor eliminado localmente y guardado como pendiente para sincronizar", new PPPoEServerResponse { Id = request.Id });
                    }

                    return ApiResponse<PPPoEServerResponse>.Error($"Error eliminando servidor PPPoE: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                }

                var query = new GetAllPppServersQuery();
                var getResult = await ExecuteQueryWithTimeoutAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de servidores PPPoE despus de eliminar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEServerResponse>.Warning("Servidor PPPoE eliminado en MikroTik, pero no se pudo sincronizar la base de datos", new PPPoEServerResponse { Id = request.Id });
                }

                if (!await SyncPppServersToDatabaseAsync(routerId, getResult.Data!))
                {
                    return ApiResponse<PPPoEServerResponse>.Warning("Servidor PPPoE eliminado en MikroTik, pero fall la sincronizacin en base de datos", new PPPoEServerResponse { Id = request.Id });
                }

                return ApiResponse<PPPoEServerResponse>.Success(new PPPoEServerResponse { Id = request.Id }, "Servidor PPPoE eliminado exitosamente");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando servidor PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEServerResponse>.Error($"Error inesperado: {ex.Message}");

            }
        }

        public async Task<ApiResponse<PagedResult<PPPoEActiveConnectionResponse>>> GetActivePPPoEConnectionsAsync(int routerId, PaginationParams paginationParams)
        {
            try
            {
                var result = await ExecuteQueryWithTimeoutAsync(routerId, new GetAllPPPoEActiveConnectionsQuery());
                if (!result.IsSuccess)
                {
                    return ApiResponse<PagedResult<PPPoEActiveConnectionResponse>>.Error(
                        $"Error obteniendo conexiones PPPoE activas: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                var allConnections = (result.Data ?? new List<PPPoEActiveConnectionResponse>()).ToList();

                var subscriptions = (await _subscriptionRepository.GetActiveByRouterIdAsync(routerId)).ToList();
                var subsBySecret = subscriptions
                    .Where(x => x.PppSecret != null)
                    .ToDictionary(x => x.PppSecret!.Name, x => x, StringComparer.OrdinalIgnoreCase);

                foreach (var connection in allConnections)
                {
                    if (subsBySecret.TryGetValue(connection.Name, out var sub))
                    {
                        connection.TieneClienteAsociado = true;
                        connection.ClienteId = sub.ClienteId;
                        connection.ClienteNombre = sub.Cliente?.Nombre;
                        connection.PlanNombre = sub.Plan?.Nombre;
                        connection.PlanVelocidadMbps = sub.Plan?.VelocidadMbps;
                        connection.EstadoSuscripcion = sub.Estado.ToString();
                    }
                }

                if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
                {
                    var term = paginationParams.SearchTerm.ToLower();
                    allConnections = allConnections.Where(c =>
                        c.Name.ToLower().Contains(term) ||
                        c.Service.ToLower().Contains(term) ||
                        c.CallerId.ToLower().Contains(term) ||
                        c.Address.ToLower().Contains(term) ||
                        c.Uptime.ToLower().Contains(term)
                    ).ToList();
                }

                if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
                {
                    var propertyInfo = typeof(PPPoEActiveConnectionResponse).GetProperty(
                        paginationParams.SortBy,
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (propertyInfo != null)
                    {
                        allConnections = paginationParams.SortDescending
                            ? allConnections.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList()
                            : allConnections.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                    }
                }
                else
                {
                    allConnections = allConnections.OrderBy(c => c.Name).ToList();
                }

                var totalCount = allConnections.Count;
                var pagedItems = allConnections
                    .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                    .Take(paginationParams.PageSize)
                    .ToList();

                var pagedResult = new PagedResult<PPPoEActiveConnectionResponse>
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    PageNumber = paginationParams.PageNumber,
                    PageSize = paginationParams.PageSize
                };

                return ApiResponse<PagedResult<PPPoEActiveConnectionResponse>>.Success(
                    pagedResult,
                    $"Se encontraron {totalCount} conexiones PPPoE activas (Pgina {paginationParams.PageNumber})"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo conexiones PPPoE activas paginadas del router {RouterId}", routerId);
                return ApiResponse<PagedResult<PPPoEActiveConnectionResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEActiveConnectionResponse>> DeleteActivePPPoEConnectionAsync(int routerId, DeletePPPoEActiveConnectionRequest request)
        {
            try
            {
                var result = await ExecuteMutationWithTimeoutAsync(routerId, new DeletePPPoEActiveConnectionOperation(), request);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoEActiveConnectionResponse>.Error(
                        $"Error eliminando conexión PPPoE activa: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<PPPoEActiveConnectionResponse>.Success(
                    new PPPoEActiveConnectionResponse { Id = request.Id },
                    "Conexión PPPoE activa eliminada exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando conexión PPPoE activa en router {RouterId}", routerId);
                return ApiResponse<PPPoEActiveConnectionResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        private async Task<bool> SyncIpPoolsToDatabaseAsync(int routerId, IEnumerable<IpPoolResponse> routerPools)
        {
            try
            {
                var dbPools = (await _ipPoolRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var routerById = routerPools
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .ToDictionary(x => x.Id.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

                var routerByName = routerPools
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .GroupBy(x => NormalizeKey(x.Name), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                foreach (var dbPool in dbPools)
                {
                    var key = (dbPool.MikroTikId ?? string.Empty).Trim();
                    IpPoolResponse? routerPool = null;

                    if (!string.IsNullOrWhiteSpace(key) && routerById.TryGetValue(key, out var byId))
                    {
                        routerPool = byId;
                    }
                    else
                    {
                        var nameKey = NormalizeKey(dbPool.Name);
                        if (!string.IsNullOrWhiteSpace(nameKey) && routerByName.TryGetValue(nameKey, out var byName))
                        {
                            routerPool = byName;
                        }
                    }

                    if (routerPool != null)
                    {
                        dbPool.MikroTikId = routerPool.Id;
                        dbPool.Name = routerPool.Name;
                        dbPool.Ranges = routerPool.Ranges;
                        dbPool.NextPool = string.IsNullOrWhiteSpace(routerPool.NextPool) ? null : routerPool.NextPool;
                        dbPool.Comment = string.IsNullOrWhiteSpace(routerPool.Comment) ? null : routerPool.Comment;
                        dbPool.SyncState = SyncState.Synced;
                        _ipPoolRepository.UpdateAsync(dbPool);

                        if (!string.IsNullOrWhiteSpace(routerPool.Id))
                        {
                            routerById.Remove(routerPool.Id.Trim());
                        }

                        var matchedNameKey = NormalizeKey(routerPool.Name);
                        if (!string.IsNullOrWhiteSpace(matchedNameKey))
                        {
                            routerByName.Remove(matchedNameKey);
                        }
                    }
                    else
                    {
                        _ipPoolRepository.DeleteAsync(dbPool);
                    }
                }

                foreach (var routerPool in routerById.Values
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .GroupBy(x => NormalizeKey(x.Name), StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First()))
                {
                    _ipPoolRepository.Add(new IpPool
                    {
                        RouterId = routerId,
                        MikroTikId = routerPool.Id,
                        Name = routerPool.Name,
                        Ranges = routerPool.Ranges,
                        NextPool = string.IsNullOrWhiteSpace(routerPool.NextPool) ? null : routerPool.NextPool,
                        Comment = string.IsNullOrWhiteSpace(routerPool.Comment) ? null : routerPool.Comment,
                        SyncState = SyncState.Synced
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando IP Pools en base de datos para router {RouterId}", routerId);
                _unitOfWork.ClearChangeTracker();
                return false;
            }
        }

        private async Task<bool> SyncPppProfilesToDatabaseAsync(int routerId, IEnumerable<PPPoEProfileResponse> routerProfiles)
        {
            try
            {
                var dbProfiles = (await _pppProfileRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var routerById = routerProfiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .ToDictionary(x => x.Id.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

                foreach (var dbProfile in dbProfiles)
                {
                    var key = (dbProfile.MikroTikId ?? string.Empty).Trim();
                    if (routerById.TryGetValue(key, out var routerProfile))
                    {
                        dbProfile.Name = routerProfile.Name;
                        dbProfile.LocalAddress = string.IsNullOrWhiteSpace(routerProfile.LocalAddress) ? null : routerProfile.LocalAddress;
                        dbProfile.RemoteAddress = string.IsNullOrWhiteSpace(routerProfile.RemoteAddress) ? null : routerProfile.RemoteAddress;
                        dbProfile.DnsServers = string.IsNullOrWhiteSpace(routerProfile.DnsServers) ? null : routerProfile.DnsServers;
                        dbProfile.RateLimit = string.IsNullOrWhiteSpace(routerProfile.RateLimit) ? null : routerProfile.RateLimit;
                        dbProfile.OnlyOne = string.IsNullOrWhiteSpace(routerProfile.OnlyOne) ? "default" : routerProfile.OnlyOne;
                        dbProfile.IsDefault = string.Equals(routerProfile.Name, "default", StringComparison.OrdinalIgnoreCase);
                        dbProfile.Comment = string.IsNullOrWhiteSpace(routerProfile.Comment) ? null : routerProfile.Comment;
                        dbProfile.SyncState = SyncState.Synced;
                        _pppProfileRepository.UpdateAsync(dbProfile);
                        routerById.Remove(key);
                    }
                    else
                    {
                        _pppProfileRepository.DeleteAsync(dbProfile);
                    }
                }

                foreach (var routerProfile in routerById.Values)
                {
                    _pppProfileRepository.Add(new PppProfile
                    {
                        RouterId = routerId,
                        MikroTikId = routerProfile.Id,
                        Name = routerProfile.Name,
                        LocalAddress = string.IsNullOrWhiteSpace(routerProfile.LocalAddress) ? null : routerProfile.LocalAddress,
                        RemoteAddress = string.IsNullOrWhiteSpace(routerProfile.RemoteAddress) ? null : routerProfile.RemoteAddress,
                        DnsServers = string.IsNullOrWhiteSpace(routerProfile.DnsServers) ? null : routerProfile.DnsServers,
                        RateLimit = string.IsNullOrWhiteSpace(routerProfile.RateLimit) ? null : routerProfile.RateLimit,
                        OnlyOne = string.IsNullOrWhiteSpace(routerProfile.OnlyOne) ? "default" : routerProfile.OnlyOne,
                        IsDefault = string.Equals(routerProfile.Name, "default", StringComparison.OrdinalIgnoreCase),
                        Comment = string.IsNullOrWhiteSpace(routerProfile.Comment) ? null : routerProfile.Comment,
                        SyncState = SyncState.Synced
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando perfiles PPPoE en base de datos para router {RouterId}", routerId);
                _unitOfWork.ClearChangeTracker();
                return false;
            }
        }

        private async Task<bool> SyncPppSecretsToDatabaseAsync(int routerId, IEnumerable<PPPoESecretResponse> routerSecrets)
        {
            try
            {
                var dbSecrets = (await _pppSecretRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var routerById = routerSecrets
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .ToDictionary(x => x.Id.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

                foreach (var dbSecret in dbSecrets)
                {
                    var key = (dbSecret.MikroTikId ?? string.Empty).Trim();
                    if (routerById.TryGetValue(key, out var routerSecret))
                    {
                        dbSecret.Name = routerSecret.Name;
                        dbSecret.Service = string.IsNullOrWhiteSpace(routerSecret.Service) ? "pppoe" : routerSecret.Service;
                        dbSecret.Profile = string.IsNullOrWhiteSpace(routerSecret.Profile) ? string.Empty : routerSecret.Profile;
                        dbSecret.Disabled = routerSecret.Disabled;
                        dbSecret.Comment = string.IsNullOrWhiteSpace(routerSecret.Comment) ? null : routerSecret.Comment;
                        dbSecret.SyncState = SyncState.Synced;
                        _pppSecretRepository.UpdateAsync(dbSecret);
                        routerById.Remove(key);
                    }
                    else
                    {
                        _pppSecretRepository.DeleteAsync(dbSecret);
                    }
                }

                foreach (var routerSecret in routerById.Values)
                {
                    _pppSecretRepository.Add(new PppSecret
                    {
                        RouterId = routerId,
                        MikroTikId = routerSecret.Id,
                        Name = routerSecret.Name,
                        Service = string.IsNullOrWhiteSpace(routerSecret.Service) ? "pppoe" : routerSecret.Service,
                        Profile = string.IsNullOrWhiteSpace(routerSecret.Profile) ? string.Empty : routerSecret.Profile,
                        Disabled = routerSecret.Disabled,
                        Comment = string.IsNullOrWhiteSpace(routerSecret.Comment) ? null : routerSecret.Comment,
                        SyncState = SyncState.Synced
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando secretos PPPoE en base de datos para router {RouterId}", routerId);
                _unitOfWork.ClearChangeTracker();
                return false;
            }
        }

        private async Task<bool> SyncPppServersToDatabaseAsync(int routerId, IEnumerable<PPPoEServerResponse> routerServers)
        {
            try
            {
                var dbServers = (await _pppServerRepository.GetAllAsync())
                    .Where(x => x.RouterId == routerId)
                    .ToList();

                var routerById = routerServers
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .ToDictionary(x => x.Id.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

                foreach (var dbServer in dbServers)
                {
                    var key = (dbServer.MikroTikId ?? string.Empty).Trim();
                    if (routerById.TryGetValue(key, out var routerServer))
                    {
                        dbServer.Name = routerServer.Name;
                        dbServer.Interface = routerServer.Interface;
                        dbServer.DefaultProfile = string.IsNullOrWhiteSpace(routerServer.Profile) ? "default" : routerServer.Profile;
                        dbServer.MaxMtu = ParseNullableInt(routerServer.MaxMTU);
                        dbServer.MaxMru = ParseNullableInt(routerServer.MaxMRU);
                        dbServer.KeepaliveTimeout = ParseNullableInt(routerServer.KeepAliveTimeOut);
                        dbServer.OneSessionPerHost = ParseRouterBool(routerServer.OneSesionPerHost);
                        dbServer.Disabled = routerServer.Disabled;
                        dbServer.Comment = string.IsNullOrWhiteSpace(routerServer.Comment) ? null : routerServer.Comment;
                        dbServer.SyncState = SyncState.Synced;
                        _pppServerRepository.UpdateAsync(dbServer);
                        routerById.Remove(key);
                    }
                    else
                    {
                        _pppServerRepository.DeleteAsync(dbServer);
                    }
                }

                foreach (var routerServer in routerById.Values)
                {
                    _pppServerRepository.Add(new PppServer
                    {
                        RouterId = routerId,
                        MikroTikId = routerServer.Id,
                        Name = routerServer.Name,
                        Interface = routerServer.Interface,
                        DefaultProfile = string.IsNullOrWhiteSpace(routerServer.Profile) ? "default" : routerServer.Profile,
                        MaxMtu = ParseNullableInt(routerServer.MaxMTU),
                        MaxMru = ParseNullableInt(routerServer.MaxMRU),
                        KeepaliveTimeout = ParseNullableInt(routerServer.KeepAliveTimeOut),
                        OneSessionPerHost = ParseRouterBool(routerServer.OneSesionPerHost),
                        Disabled = routerServer.Disabled,
                        Comment = string.IsNullOrWhiteSpace(routerServer.Comment) ? null : routerServer.Comment,
                        SyncState = SyncState.Synced
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sincronizando servidores PPPoE en base de datos para router {RouterId}", routerId);
                _unitOfWork.ClearChangeTracker();
                return false;
            }
        }

        private static string NormalizeKey(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static bool IsRouterOfflineError(MikroTikErrorType errorType)
        {
            return errorType == MikroTikErrorType.ConnectionFailed
                || errorType == MikroTikErrorType.Timeout
                || errorType == MikroTikErrorType.RouterUnavailable;
        }

        private async Task<MikroTikResult<TResponse>> ExecuteQueryWithTimeoutAsync<TResponse>(int routerId, IMikroTikQuery<TResponse> query)
        {
            var queryTask = _connectionManager.ExecuteQueryAsync(routerId, query);
            var completedTask = await Task.WhenAny(queryTask, Task.Delay(_routerQueryTimeout));

            if (completedTask == queryTask)
            {
                return await queryTask;
            }

            _ = queryTask.ContinueWith(
                t =>
                {
                    _logger.LogWarning(t.Exception, "Query tardía/fallida después de timeout para router {RouterId}. Comando: {Command}", routerId, query.Command);
                },
                TaskContinuationOptions.OnlyOnFaulted);

            _logger.LogWarning("Timeout ejecutando query MikroTik para router {RouterId}. Comando: {Command}. Se usará fallback local.", routerId, query.Command);
            return MikroTikResult<TResponse>.Failure("Timeout consultando router MikroTik", MikroTikErrorType.Timeout, routerId);
        }

        private async Task<MikroTikResult<TResponse>> ExecuteMutationWithTimeoutAsync<TRequest, TResponse>(
            int routerId,
            IMikroTikMutation<TRequest, TResponse> operation,
            TRequest request)
        {
            var mutationTask = _connectionManager.ExecuteMutationAsync(routerId, operation, request);
            var completedTask = await Task.WhenAny(mutationTask, Task.Delay(_routerMutationTimeout));

            if (completedTask == mutationTask)
            {
                return await mutationTask;
            }

            _ = mutationTask.ContinueWith(
                t =>
                {
                    _logger.LogWarning(t.Exception, "Mutación tardía/fallida después de timeout para router {RouterId}. Comando: {Command}", routerId, operation.Command);
                },
                TaskContinuationOptions.OnlyOnFaulted);

            _logger.LogWarning("Timeout ejecutando mutación MikroTik para router {RouterId}. Comando: {Command}. Se encolará como pendiente si aplica.", routerId, operation.Command);
            return MikroTikResult<TResponse>.Failure("Timeout ejecutando cambio en router MikroTik", MikroTikErrorType.Timeout, routerId);
        }

        private async Task QueuePendingChangeAsync(
            int routerId,
            PendingChangeResource resource,
            PendingChangeOperation operation,
            object payload,
            string? entityKey,
            string? errorMessage)
        {
            try
            {
                var pending = new PendingChange
                {
                    RouterId = routerId,
                    Resource = resource,
                    Operation = operation,
                    Status = PendingChangeStatus.Pending,
                    EntityKey = entityKey,
                    PayloadJson = JsonSerializer.Serialize(payload),
                    LastError = errorMessage,
                    NextRetryAt = DateTime.UtcNow
                };

                _pendingChangeRepository.Add(pending);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando pending change para router {RouterId}", routerId);
            }
        }

        private async Task ProcessPendingChangesIfOnlineAsync(int routerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionStatus = await _connectionManager.GetConnectionStatusAsync(routerId);
                if (!connectionStatus.IsConnected)
                {
                    return;
                }

                var pendingItems = await _pendingChangeRepository.GetReadyPendingByRouterAsync(routerId, 100);
                if (!pendingItems.Any())
                {
                    return;
                }

                _logger.LogInformation("Router {RouterId}: {PendingCount} pending changes listos para procesar", routerId, pendingItems.Count);

                var appliedAny = false;
                var appliedCount = 0;
                var failedCount = 0;

                foreach (var pending in pendingItems)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    pending.Status = PendingChangeStatus.Processing;
                    _pendingChangeRepository.UpdateAsync(pending);
                    await _unitOfWork.SaveChangesAsync();

                    try
                    {
                        var result = await ApplyPendingChangeAsync(routerId, pending);
                        if (result.IsSuccess)
                        {
                            pending.Status = PendingChangeStatus.Applied;
                            pending.ProcessedAt = DateTime.UtcNow;
                            pending.LastError = null;
                            appliedAny = true;
                            appliedCount += 1;
                        }
                        else
                        {
                            failedCount += 1;
                            pending.Status = PendingChangeStatus.Pending;
                            pending.RetryCount += 1;
                            pending.LastError = result.ErrorMessage;
                            pending.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Min(30, Math.Pow(2, pending.RetryCount)));

                            if (IsRouterOfflineError(result.ErrorType))
                            {
                                _pendingChangeRepository.UpdateAsync(pending);
                                await _unitOfWork.SaveChangesAsync();
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount += 1;
                        pending.Status = PendingChangeStatus.Pending;
                        pending.RetryCount += 1;
                        pending.LastError = ex.Message;
                        pending.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Min(30, Math.Pow(2, pending.RetryCount)));
                    }

                    _pendingChangeRepository.UpdateAsync(pending);
                    await _unitOfWork.SaveChangesAsync();
                }

                if (appliedAny)
                {
                    await RefreshMirrorFromRouterAsync(routerId);
                }

                _logger.LogInformation(
                    "Router {RouterId}: procesamiento de pending finalizado. Aplicados={AppliedCount}, Fallidos={FailedCount}, RefrescoEspejo={RefreshDone}",
                    routerId,
                    appliedCount,
                    failedCount,
                    appliedAny);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando pending changes para router {RouterId}", routerId);
            }
        }

        private async Task<(bool SyncedFromRouter, bool OfflineRouter)> TrySyncIpPoolsFromRouterAsync(int routerId)
        {
            var result = await ExecuteQueryWithTimeoutAsync(routerId, new GetAllIpPoolsQuery());
            if (!result.IsSuccess)
            {
                return (false, IsRouterOfflineError(result.ErrorType));
            }

            var data = result.Data ?? new List<IpPoolResponse>();
            await SyncIpPoolsToDatabaseAsync(routerId, data);
            return (true, false);
        }

        private async Task<(bool SyncedFromRouter, bool OfflineRouter)> TrySyncPppProfilesFromRouterAsync(int routerId)
        {
            var result = await ExecuteQueryWithTimeoutAsync(routerId, new GetAllPppProfilesQuery());
            if (!result.IsSuccess)
            {
                return (false, IsRouterOfflineError(result.ErrorType));
            }

            var data = result.Data ?? new List<PPPoEProfileResponse>();
            await SyncPppProfilesToDatabaseAsync(routerId, data);
            return (true, false);
        }

        private async Task<(bool SyncedFromRouter, bool OfflineRouter)> TrySyncPppSecretsFromRouterAsync(int routerId)
        {
            var result = await ExecuteQueryWithTimeoutAsync(routerId, new GetAllPppSecretsQuery());
            if (!result.IsSuccess)
            {
                return (false, IsRouterOfflineError(result.ErrorType));
            }

            var data = result.Data ?? new List<PPPoESecretResponse>();
            await SyncPppSecretsToDatabaseAsync(routerId, data);
            return (true, false);
        }

        private async Task<(bool SyncedFromRouter, bool OfflineRouter)> TrySyncPppServersFromRouterAsync(int routerId)
        {
            var result = await ExecuteQueryWithTimeoutAsync(routerId, new GetAllPppServersQuery());
            if (!result.IsSuccess)
            {
                return (false, IsRouterOfflineError(result.ErrorType));
            }

            var data = result.Data ?? new List<PPPoEServerResponse>();
            await SyncPppServersToDatabaseAsync(routerId, data);
            return (true, false);
        }

        private static string GeneratePendingMikroTikId()
        {
            return $"pending-{Guid.NewGuid():N}";
        }

        private async Task<IpPoolResponse> ApplyLocalIpPoolCreateAsync(int routerId, CreateIpPoolRequest request)
        {
            var entity = new IpPool
            {
                RouterId = routerId,
                MikroTikId = GeneratePendingMikroTikId(),
                SyncState = SyncState.PendingCreate,
                Name = request.Name,
                Ranges = request.Ranges,
                NextPool = string.IsNullOrWhiteSpace(request.NextPool) ? null : request.NextPool,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment
            };

            _ipPoolRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapIpPoolEntityToResponse(entity);
        }

        private async Task<IpPoolResponse?> ApplyLocalIpPoolUpdateAsync(int routerId, UpdateIpPoolRequest request)
        {
            var entity = (await _ipPoolRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == request.Id);

            if (entity == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Ranges)) entity.Ranges = request.Ranges;
            if (!string.IsNullOrWhiteSpace(request.NextPool)) entity.NextPool = request.NextPool;
            if (!string.IsNullOrWhiteSpace(request.Comment)) entity.Comment = request.Comment;
            entity.SyncState = SyncState.PendingUpdate;

            _ipPoolRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapIpPoolEntityToResponse(entity);
        }

        private async Task ApplyLocalIpPoolDeleteAsync(int routerId, string mikroTikId)
        {
            var entity = (await _ipPoolRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == mikroTikId);

            if (entity == null)
            {
                return;
            }

            _ipPoolRepository.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<PPPoEProfileResponse> ApplyLocalPppProfileCreateAsync(int routerId, CreatePPPoEProfile request)
        {
            var entity = new PppProfile
            {
                RouterId = routerId,
                MikroTikId = GeneratePendingMikroTikId(),
                SyncState = SyncState.PendingCreate,
                Name = request.Name,
                LocalAddress = string.IsNullOrWhiteSpace(request.LocalAddress) ? null : request.LocalAddress,
                RemoteAddress = string.IsNullOrWhiteSpace(request.RemoteAddress) ? null : request.RemoteAddress,
                DnsServers = string.IsNullOrWhiteSpace(request.DnsServers) ? null : request.DnsServers,
                RateLimit = string.IsNullOrWhiteSpace(request.RateLimit) ? null : request.RateLimit,
                OnlyOne = string.IsNullOrWhiteSpace(request.OnlyOne) ? "default" : request.OnlyOne,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment
            };

            _pppProfileRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapPppProfileEntityToResponse(entity);
        }

        private async Task<PPPoEProfileResponse?> ApplyLocalPppProfileUpdateAsync(int routerId, UpdatePPPoEProfile request)
        {
            var entity = (await _pppProfileRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == request.Id);

            if (entity == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.LocalAddress)) entity.LocalAddress = request.LocalAddress;
            if (!string.IsNullOrWhiteSpace(request.RemoteAddress)) entity.RemoteAddress = request.RemoteAddress;
            if (!string.IsNullOrWhiteSpace(request.DnsServers)) entity.DnsServers = request.DnsServers;
            if (!string.IsNullOrWhiteSpace(request.RateLimit)) entity.RateLimit = request.RateLimit;
            if (!string.IsNullOrWhiteSpace(request.OnlyOne)) entity.OnlyOne = request.OnlyOne;
            if (!string.IsNullOrWhiteSpace(request.Comment)) entity.Comment = request.Comment;
            entity.SyncState = SyncState.PendingUpdate;

            _pppProfileRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapPppProfileEntityToResponse(entity);
        }

        private async Task ApplyLocalPppProfileDeleteAsync(int routerId, string mikroTikId)
        {
            var entity = (await _pppProfileRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == mikroTikId);

            if (entity == null)
            {
                return;
            }

            _pppProfileRepository.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<PPPoESecretResponse> ApplyLocalPppSecretCreateAsync(int routerId, CreatePPPoESecretRequest request)
        {
            var entity = new PppSecret
            {
                RouterId = routerId,
                MikroTikId = GeneratePendingMikroTikId(),
                SyncState = SyncState.PendingCreate,
                Name = request.Name,
                Password = request.Password,
                Service = string.IsNullOrWhiteSpace(request.Service) ? "pppoe" : request.Service,
                Profile = string.IsNullOrWhiteSpace(request.Profile) ? string.Empty : request.Profile,
                Disabled = request.Disabled,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment
            };

            _pppSecretRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapPppSecretEntityToResponse(entity);
        }

        private async Task<PPPoESecretResponse?> ApplyLocalPppSecretUpdateAsync(int routerId, UpdatePPPoESecretRequest request)
        {
            var entity = (await _pppSecretRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == request.Id);

            if (entity == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Password)) entity.Password = request.Password;
            if (!string.IsNullOrWhiteSpace(request.Service)) entity.Service = request.Service;
            if (!string.IsNullOrWhiteSpace(request.Profile)) entity.Profile = request.Profile;
            if (request.Disabled.HasValue) entity.Disabled = request.Disabled.Value;
            if (!string.IsNullOrWhiteSpace(request.Comment)) entity.Comment = request.Comment;
            entity.SyncState = SyncState.PendingUpdate;

            _pppSecretRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapPppSecretEntityToResponse(entity);
        }

        private async Task ApplyLocalPppSecretDeleteAsync(int routerId, string mikroTikId)
        {
            var entity = (await _pppSecretRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == mikroTikId);

            if (entity == null)
            {
                return;
            }

            _pppSecretRepository.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<PPPoEServerResponse> ApplyLocalPppServerCreateAsync(int routerId, CreatePPPoEServerRequest request)
        {
            var entity = new PppServer
            {
                RouterId = routerId,
                MikroTikId = GeneratePendingMikroTikId(),
                SyncState = SyncState.PendingCreate,
                Name = request.Name,
                Interface = request.Interface,
                DefaultProfile = string.IsNullOrWhiteSpace(request.Profile) ? "default" : request.Profile,
                MaxMtu = ParseNullableInt(request.MaxMTU),
                MaxMru = ParseNullableInt(request.MaxMRU),
                KeepaliveTimeout = ParseNullableInt(request.KeepAliveTimeOut),
                OneSessionPerHost = ParseRouterBool(request.OneSesionPerHost),
                Disabled = false,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment
            };

            _pppServerRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapPppServerEntityToResponse(entity);
        }

        private async Task<PPPoEServerResponse?> ApplyLocalPppServerUpdateAsync(int routerId, UpdatePPPoEServerRequest request)
        {
            var entity = (await _pppServerRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == request.Id);

            if (entity == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Interface)) entity.Interface = request.Interface;
            if (!string.IsNullOrWhiteSpace(request.Profile)) entity.DefaultProfile = request.Profile;
            if (!string.IsNullOrWhiteSpace(request.MaxMTU)) entity.MaxMtu = ParseNullableInt(request.MaxMTU);
            if (!string.IsNullOrWhiteSpace(request.MaxMRU)) entity.MaxMru = ParseNullableInt(request.MaxMRU);
            if (!string.IsNullOrWhiteSpace(request.KeepAliveTimeOut)) entity.KeepaliveTimeout = ParseNullableInt(request.KeepAliveTimeOut);
            if (!string.IsNullOrWhiteSpace(request.OneSesionPerHost)) entity.OneSessionPerHost = ParseRouterBool(request.OneSesionPerHost);
            if (!string.IsNullOrWhiteSpace(request.Comment)) entity.Comment = request.Comment;
            entity.SyncState = SyncState.PendingUpdate;

            _pppServerRepository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return MapPppServerEntityToResponse(entity);
        }

        private async Task ApplyLocalPppServerDeleteAsync(int routerId, string mikroTikId)
        {
            var entity = (await _pppServerRepository.GetAllAsync())
                .FirstOrDefault(x => x.RouterId == routerId && x.MikroTikId == mikroTikId);

            if (entity == null)
            {
                return;
            }

            _pppServerRepository.DeleteAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<MikroTikResult<bool>> ApplyPendingChangeAsync(int routerId, PendingChange pending)
        {
            try
            {
                switch (pending.Resource)
                {
                    case PendingChangeResource.IpPool:
                        return await ApplyIpPoolPendingAsync(routerId, pending);
                    case PendingChangeResource.PppProfile:
                        return await ApplyPppProfilePendingAsync(routerId, pending);
                    case PendingChangeResource.PppSecret:
                        return await ApplyPppSecretPendingAsync(routerId, pending);
                    case PendingChangeResource.PppServer:
                        return await ApplyPppServerPendingAsync(routerId, pending);
                    default:
                        return MikroTikResult<bool>.Failure("Tipo de recurso pendiente no soportado", MikroTikErrorType.Unknown, routerId);
                }
            }
            catch (Exception ex)
            {
                return MikroTikResult<bool>.Failure(ex.Message, MikroTikErrorType.Unknown, routerId);
            }
        }

        private async Task<MikroTikResult<bool>> ApplyIpPoolPendingAsync(int routerId, PendingChange pending)
        {
            return pending.Operation switch
            {
                PendingChangeOperation.Create => await ExecutePendingMutationAsync(routerId, new CreateIpPoolOperation(), JsonSerializer.Deserialize<CreateIpPoolRequest>(pending.PayloadJson)),
                PendingChangeOperation.Update => await ExecutePendingMutationAsync(routerId, new UpdateIpPoolOperation(), JsonSerializer.Deserialize<UpdateIpPoolRequest>(pending.PayloadJson)),
                PendingChangeOperation.Delete => await ExecutePendingMutationAsync(routerId, new DeleteIpPoolOperation(), JsonSerializer.Deserialize<DeleteIpPoolRequest>(pending.PayloadJson)),
                _ => MikroTikResult<bool>.Failure("Operación pendiente no soportada para IP Pool", MikroTikErrorType.Unknown, routerId)
            };
        }

        private async Task<MikroTikResult<bool>> ApplyPppProfilePendingAsync(int routerId, PendingChange pending)
        {
            return pending.Operation switch
            {
                PendingChangeOperation.Create => await ExecutePendingMutationAsync(routerId, new CreatePppProfileOperation(), JsonSerializer.Deserialize<CreatePPPoEProfile>(pending.PayloadJson)),
                PendingChangeOperation.Update => await ExecutePendingMutationAsync(routerId, new UpdatePppProfileOperation(), JsonSerializer.Deserialize<UpdatePPPoEProfile>(pending.PayloadJson)),
                PendingChangeOperation.Delete => await ExecutePendingMutationAsync(routerId, new DeletePppProfileOperation(), JsonSerializer.Deserialize<DeletePPPoEProfile>(pending.PayloadJson)),
                _ => MikroTikResult<bool>.Failure("Operación pendiente no soportada para PPPoE Profile", MikroTikErrorType.Unknown, routerId)
            };
        }

        private async Task<MikroTikResult<bool>> ApplyPppSecretPendingAsync(int routerId, PendingChange pending)
        {
            return pending.Operation switch
            {
                PendingChangeOperation.Create => await ExecutePendingMutationAsync(routerId, new CreatePppSecretOperation(), JsonSerializer.Deserialize<CreatePPPoESecretRequest>(pending.PayloadJson)),
                PendingChangeOperation.Update => await ExecutePendingMutationAsync(routerId, new UpdatePppSecretOperation(), JsonSerializer.Deserialize<UpdatePPPoESecretRequest>(pending.PayloadJson)),
                PendingChangeOperation.Delete => await ExecutePendingMutationAsync(routerId, new DeletePppSecretOperation(), JsonSerializer.Deserialize<DeletePPPoESecretRequest>(pending.PayloadJson)),
                _ => MikroTikResult<bool>.Failure("Operación pendiente no soportada para PPPoE Secret", MikroTikErrorType.Unknown, routerId)
            };
        }

        private async Task<MikroTikResult<bool>> ApplyPppServerPendingAsync(int routerId, PendingChange pending)
        {
            return pending.Operation switch
            {
                PendingChangeOperation.Create => await ExecutePendingMutationAsync(routerId, new CreatePppServerOperation(), JsonSerializer.Deserialize<CreatePPPoEServerRequest>(pending.PayloadJson)),
                PendingChangeOperation.Update => await ExecutePendingMutationAsync(routerId, new UpdatePppServerOperation(), JsonSerializer.Deserialize<UpdatePPPoEServerRequest>(pending.PayloadJson)),
                PendingChangeOperation.Delete => await ExecutePendingMutationAsync(routerId, new DeletePppServerOperation(), JsonSerializer.Deserialize<DeletePPPoEServerRequest>(pending.PayloadJson)),
                _ => MikroTikResult<bool>.Failure("Operación pendiente no soportada para PPPoE Server", MikroTikErrorType.Unknown, routerId)
            };
        }

        private async Task<MikroTikResult<bool>> ExecutePendingMutationAsync<TRequest, TResponse>(
            int routerId,
            IMikroTikMutation<TRequest, TResponse> operation,
            TRequest? request)
        {
            if (request == null)
            {
                return MikroTikResult<bool>.Failure("Payload de pending change inválido", MikroTikErrorType.InvalidResponse, routerId);
            }

            var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, request);
            if (!result.IsSuccess)
            {
                return MikroTikResult<bool>.Failure(result.ErrorMessage ?? "Error aplicando pending change", result.ErrorType, routerId);
            }

            return MikroTikResult<bool>.Success(true, routerId);
        }

        private async Task RefreshMirrorFromRouterAsync(int routerId)
        {
            var ipPools = await _connectionManager.ExecuteQueryAsync(routerId, new GetAllIpPoolsQuery());
            if (ipPools.IsSuccess && ipPools.Data != null)
            {
                await SyncIpPoolsToDatabaseAsync(routerId, ipPools.Data);
            }

            var profiles = await _connectionManager.ExecuteQueryAsync(routerId, new GetAllPppProfilesQuery());
            if (profiles.IsSuccess && profiles.Data != null)
            {
                await SyncPppProfilesToDatabaseAsync(routerId, profiles.Data);
            }

            var secrets = await _connectionManager.ExecuteQueryAsync(routerId, new GetAllPppSecretsQuery());
            if (secrets.IsSuccess && secrets.Data != null)
            {
                await SyncPppSecretsToDatabaseAsync(routerId, secrets.Data);
            }

            var servers = await _connectionManager.ExecuteQueryAsync(routerId, new GetAllPppServersQuery());
            if (servers.IsSuccess && servers.Data != null)
            {
                await SyncPppServersToDatabaseAsync(routerId, servers.Data);
            }
        }

        private static IpPoolResponse MapIpPoolEntityToResponse(IpPool entity)
        {
            return new IpPoolResponse
            {
                Id = entity.MikroTikId,
                Name = entity.Name,
                Ranges = entity.Ranges,
                NextPool = entity.NextPool ?? string.Empty,
                Comment = entity.Comment ?? string.Empty,
                SyncState = entity.SyncState.ToString().ToLowerInvariant()
            };
        }

        private static PPPoEProfileResponse MapPppProfileEntityToResponse(PppProfile entity)
        {
            return new PPPoEProfileResponse
            {
                Id = entity.MikroTikId,
                Name = entity.Name,
                LocalAddress = entity.LocalAddress ?? string.Empty,
                RemoteAddress = entity.RemoteAddress ?? string.Empty,
                DnsServers = entity.DnsServers ?? string.Empty,
                RateLimit = entity.RateLimit ?? string.Empty,
                OnlyOne = entity.OnlyOne,
                Comment = entity.Comment ?? string.Empty,
                SyncState = entity.SyncState.ToString().ToLowerInvariant()
            };
        }

        private static PPPoESecretResponse MapPppSecretEntityToResponse(PppSecret entity)
        {
            return new PPPoESecretResponse
            {
                Id = entity.MikroTikId,
                Name = entity.Name,
                Profile = entity.Profile,
                Service = entity.Service,
                Disabled = entity.Disabled,
                Comment = entity.Comment ?? string.Empty,
                SyncState = entity.SyncState.ToString().ToLowerInvariant()
            };
        }

        private static PPPoEServerResponse MapPppServerEntityToResponse(PppServer entity)
        {
            return new PPPoEServerResponse
            {
                Id = entity.MikroTikId,
                Name = entity.Name,
                Interface = entity.Interface,
                Profile = entity.DefaultProfile,
                MaxMTU = entity.MaxMtu?.ToString() ?? string.Empty,
                MaxMRU = entity.MaxMru?.ToString() ?? string.Empty,
                KeepAliveTimeOut = entity.KeepaliveTimeout?.ToString() ?? string.Empty,
                OneSesionPerHost = entity.OneSessionPerHost ? "yes" : "no",
                Disabled = entity.Disabled,
                Comment = entity.Comment ?? string.Empty,
                SyncState = entity.SyncState.ToString().ToLowerInvariant()
            };
        }

        private static int? ParseNullableInt(string? value)
        {
            if (int.TryParse(value, out var parsed))
            {
                return parsed;
            }

            return null;
        }

        private static bool ParseRouterBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value == "1";
        }
    }
}
