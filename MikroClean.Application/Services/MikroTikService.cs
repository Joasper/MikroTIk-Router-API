using Microsoft.Extensions.Logging;
using MikroClean.Application.Interfaces;
using MikroClean.Application.MikroTik.Operations;
using MikroClean.Application.Models;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;
using static MikroClean.Application.MikroTik.Operations.CreateFirewallRuleOperation;
using static MikroClean.Application.MikroTik.Operations.CreateFirewallRuleOperation.GetAllIpPoolsQuery;

namespace MikroClean.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para operaciones MikroTik
    /// Orquesta las operaciones entre el connection manager y los repositorios
    /// </summary>
    public class MikroTikService : IMikroTikService
    {
        private readonly IMikroTikConnectionManager _connectionManager;
        private readonly IRouterRepository _routerRepository;
        private readonly ILogger<MikroTikService> _logger;

        public MikroTikService(
            IMikroTikConnectionManager connectionManager,
            IRouterRepository routerRepository,
            ILogger<MikroTikService> logger)
        {
            _connectionManager = connectionManager;
            _routerRepository = routerRepository;
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
                var query = new GetAllIpPoolsQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

                if (!result.IsSuccess)
                {
                    return ApiResponse<PagedResult<IpPoolResponse>>.Error(
                        $"Error obteniendo pools de IP: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                var allPools = result.Data ?? new List<IpPoolResponse>();
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
                    $"Se encontraron {totalCount} pools de IP (Pgina {paginationParams.PageNumber})"
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
                var existingPoolsResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                
                if (existingPoolsResult.IsSuccess && existingPoolsResult.Data != null)
                {
                    if (existingPoolsResult.Data.Any(p => p.Name.Equals(createPoolRequest.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return ApiResponse<IpPoolResponse>.Error($"Ya existe un IP Pool con el nombre '{createPoolRequest.Name}'");
                    }
                }

                var result = await _connectionManager.ExecuteMutationAsync(routerId, new CreateIpPoolOperation(), createPoolRequest);

                if (!result.IsSuccess)
                    return ApiResponse<IpPoolResponse>.Error($"Error creando IP pool: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });

                // Confirmar creacin y obtener datos completos
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
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
                var existingPoolsResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                
                if (existingPoolsResult.IsSuccess && existingPoolsResult.Data != null && !string.IsNullOrEmpty(updateIpPoolRequest.Name))
                {
                    if (existingPoolsResult.Data.Any(p => p.Name.Equals(updateIpPoolRequest.Name, StringComparison.OrdinalIgnoreCase) && p.Id != updateIpPoolRequest.Id))
                    {
                        return ApiResponse<IpPoolResponse>.Error($"Ya existe otro IP Pool con el nombre '{updateIpPoolRequest.Name}'");
                    }
                }

                var result = await _connectionManager.ExecuteMutationAsync(routerId, new UpdateIpPoolOperation(), updateIpPoolRequest);

                if (!result.IsSuccess)
                    return ApiResponse<IpPoolResponse>.Error($"Error actualizando IP pool: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });

                // Confirmar actualizacin
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);

                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de pools despus de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<IpPoolResponse>.Warning("Pool actualizado pero no se pudo recuperar la lista de pools para confirmar", null);
                }

                var updatedPool = getResult.Data!.FirstOrDefault(p => p.Id == updateIpPoolRequest.Id);
                if (updatedPool != null)
                {
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
                var result = await _connectionManager.ExecuteMutationAsync(routerId, new DeleteIpPoolOperation(), request);

                if (!result.IsSuccess)
                    return ApiResponse<IpPoolResponse>.Error($"Error eliminando IP pool: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });

                // /remove no retorna datos - devolvemos el Id que fue eliminado
                return ApiResponse<IpPoolResponse>.Success(new IpPoolResponse { Id = request.Id }, "IP Pool eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando IP pool en router {RouterId}", routerId);
                return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEProfileResponse>> CreatePPPoEProfileAsync(int routerId, CreatePPPoEProfile createPPPoEProfile)
        {
            try
            {
                var operation = new CreatePPPoEProfileOperation();
                var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, createPPPoEProfile);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoEProfileResponse>.Error(
                        $"Error creando perfil PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                return ApiResponse<PPPoEProfileResponse>.Success(result.Data!, "Perfil PPPoE creado exitosamente");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando perfil PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEProfileResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PPPoEProfileResponse>>> GetAllPPPoEProfileAsync(int routerId)
        {
            try
            {
                var query = new GetAllPppProfilesQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!result.IsSuccess)
                {
                    return ApiResponse<List<PPPoEProfileResponse>>.Error(
                        $"Error obteniendo perfiles PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                return ApiResponse<List<PPPoEProfileResponse>>.Success(
                    result.Data!,
                    $"Se encontraron {result.Data!.Count} perfiles PPPoE"
                );

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error obteniendo perfiles PPPoE del router {RouterId}", routerId);
                return ApiResponse<List<PPPoEProfileResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEProfileResponse>> UpdatePPPoEProfileAsync(int routerId, UpdatePPPoEProfile updatePPPoEProfile)
        {
            try
            {
                var operation = new UpdatePppProfileOperation();
                var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, updatePPPoEProfile);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoEProfileResponse>.Error(
                        $"Error actualizando perfil PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                // /set no retorna datos - buscamos el objeto actualizado por su Id
                var query = new GetAllPppProfilesQuery();
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de perfiles PPPoE después de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEProfileResponse>.Warning("Perfil PPPoE actualizado pero no se pudo recuperar la lista para confirmar", null);
                }
                var updatedProfile = getResult.Data!.FirstOrDefault(p => p.Id == updatePPPoEProfile.Id);
                if (updatedProfile != null)
                {
                    return ApiResponse<PPPoEProfileResponse>.Success(updatedProfile, "Perfil PPPoE actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el perfil PPPoE actualizado con ID {ProfileId} en router {RouterId} después de la actualización", updatePPPoEProfile.Id, routerId);
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
                var result = await _connectionManager.ExecuteMutationAsync(routerId, new DeletePppProfileOperation(), request);
                if (!result.IsSuccess)
                    return ApiResponse<PPPoEProfileResponse>.Error($"Error eliminando perfil PPPoE: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });

                return ApiResponse<PPPoEProfileResponse>.Success(new PPPoEProfileResponse { Id = request.Id }, "Perfil PPPoE eliminado exitosamente");

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error eliminando perfil PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEProfileResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PPPoESecretResponse>>> GetAllPPPoESecretAsync(int routerId)
        {
            try
            {
                var query = new GetAllPppSecretsQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!result.IsSuccess)
                {
                    return ApiResponse<List<PPPoESecretResponse>>.Error(
                        $"Error obteniendo secretos PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                return ApiResponse<List<PPPoESecretResponse>>.Success(
                    result.Data!,
                    $"Se encontraron {result.Data!.Count} secretos PPPoE"
                );

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error obteniendo secretos PPPoE del router {RouterId}", routerId);
                return ApiResponse<List<PPPoESecretResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoESecretResponse>> CreatePPPoESecretAsync(int routerId, CreatePPPoESecretRequest createPPPoESecret)
        {
            try
            {
                var operation = new CreatePPPoESecretOperation();
                var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, createPPPoESecret);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoESecretResponse>.Error(
                        $"Error creando secreto PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                // /add no retorna datos - buscamos el objeto creado por su nombre
                var query = new GetAllPppSecretsQuery();
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de secretos PPPoE después de crear uno nuevo en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se creó el secreto PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }
                var createdSecret = getResult.Data!.FirstOrDefault(s => s.Name == createPPPoESecret.Name);
                if (createdSecret != null)
                {
                    return ApiResponse<PPPoESecretResponse>.Success(createdSecret, "Secreto PPPoE creado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el secreto PPPoE recién creado con nombre {SecretName} en router {RouterId} después de la creación", createPPPoESecret.Name, routerId);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se creó el secreto PPPoE pero no se pudo confirmar su existencia en la lista", null);
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

                var operation = new UpdatePppSecretOperation();
                var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, updatePPPoESecret);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoESecretResponse>.Error(
                        $"Error actualizando secreto PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                // /set no retorna datos - buscamos el objeto actualizado por su Id
                var query = new GetAllPppSecretsQuery();
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de secretos PPPoE después de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se actualizó el secreto PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }
                var updatedSecret = getResult.Data!.FirstOrDefault(s => s.Id == updatePPPoESecret.Id);
                if (updatedSecret != null)
                {
                    return ApiResponse<PPPoESecretResponse>.Success(updatedSecret, "Secreto PPPoE actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el secreto PPPoE actualizado con ID {SecretId} en router {RouterId} después de la actualización", updatePPPoESecret.Id, routerId);
                    return ApiResponse<PPPoESecretResponse>.Warning("Se actualizó el secreto PPPoE pero no se pudo confirmar su existencia en la lista", null);
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
                var result = await _connectionManager.ExecuteMutationAsync(routerId, new DeletePppSecretOperation(), request);
                if (!result.IsSuccess)
                    return ApiResponse<PPPoESecretResponse>.Error($"Error eliminando secreto PPPoE: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                return ApiResponse<PPPoESecretResponse>.Success(new PPPoESecretResponse { Id = request.Id }, "Secreto PPPoE eliminado exitosamente");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando secreto PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoESecretResponse>.Error($"Error inesperado: {ex.Message}");

            }
        }

        public async Task<ApiResponse<List<PPPoEServerResponse>>> GetAllPPPoEServerAsync(int routerId)
        {
            try
            {

                var query = new GetAllPppServersQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!result.IsSuccess)
                {
                    return ApiResponse<List<PPPoEServerResponse>>.Error(
                        $"Error obteniendo servidores PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                return ApiResponse<List<PPPoEServerResponse>>.Success(
                    result.Data!,
                    $"Se encontraron {result.Data!.Count} servidores PPPoE"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo servidores PPPoE del router {RouterId}", routerId);
                return ApiResponse<List<PPPoEServerResponse>>.Error($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PPPoEServerResponse>> CreatePPPoEServerAsync(int routerId, CreatePPPoEServerRequest createPPPoEServer)
        {
            try
            {
                var operation = new CreatePPPoEServerOperation();
                var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, createPPPoEServer);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoEServerResponse>.Error(
                        $"Error creando servidor PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                // /add no retorna datos - buscamos el objeto creado por su nombre
                var query = new GetAllPppServersQuery();
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de servidores PPPoE después de crear uno nuevo en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se creó el servidor PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }
                var createdServer = getResult.Data!.FirstOrDefault(s => s.Name == createPPPoEServer.Name);
                if (createdServer != null)
                {
                    return ApiResponse<PPPoEServerResponse>.Success(createdServer, "Servidor PPPoE creado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el servidor PPPoE recién creado con nombre {ServerName} en router {RouterId} después de la creación", createPPPoEServer.Name, routerId);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se creó el servidor PPPoE pero no se pudo confirmar su existencia en la lista", null);
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
                var operation = new UpdatePppServerOperation();
                var result = await _connectionManager.ExecuteMutationAsync(routerId, operation, updatePPPoEServer);
                if (!result.IsSuccess)
                {
                    return ApiResponse<PPPoEServerResponse>.Error(
                        $"Error actualizando servidor PPPoE: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }
                // /set no retorna datos - buscamos el objeto actualizado por su Id
                var query = new GetAllPppServersQuery();
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, query);
                if (!getResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo recuperar la lista de servidores PPPoE después de actualizar uno en router {RouterId}: {ErrorMessage}", routerId, getResult.ErrorMessage);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se actualizó el servidor PPPoE pero no se pudo recuperar la lista para confirmar", null);
                }
                var updatedServer = getResult.Data!.FirstOrDefault(s => s.Id == updatePPPoEServer.Id);
                if (updatedServer != null)
                {
                    return ApiResponse<PPPoEServerResponse>.Success(updatedServer, "Servidor PPPoE actualizado exitosamente");
                }
                else
                {
                    _logger.LogWarning("No se pudo encontrar el servidor PPPoE actualizado con ID {ServerId} en router {RouterId} después de la actualización", updatePPPoEServer.Id, routerId);
                    return ApiResponse<PPPoEServerResponse>.Warning("Se actualizó el servidor PPPoE pero no se pudo confirmar su existencia en la lista", null);
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
                var result = await _connectionManager.ExecuteMutationAsync(routerId, new DeletePppServerOperation(), request);
                if (!result.IsSuccess)
                    return ApiResponse<PPPoEServerResponse>.Error($"Error eliminando servidor PPPoE: {result.ErrorMessage}", new { ErrorType = result.ErrorType.ToString() });
                return ApiResponse<PPPoEServerResponse>.Success(new PPPoEServerResponse { Id = request.Id }, "Servidor PPPoE eliminado exitosamente");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando servidor PPPoE en router {RouterId}", routerId);
                return ApiResponse<PPPoEServerResponse>.Error($"Error inesperado: {ex.Message}");

            }
        }
    }
}
