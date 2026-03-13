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

        public async Task<ApiResponse<List<IpPoolResponse>>> GetAllIpPoolsAsync(int routerId)
        {
            try
            {

                var query = new GetAllIpPoolsQuery();
                var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

                if (!result.IsSuccess)                 {
                    return ApiResponse<List<IpPoolResponse>>.Error(
                        $"Error obteniendo pools de IP: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<List<IpPoolResponse>>.Success(
                    result.Data,
                    $"Se encontraron {result.Data.Count} pools de IP"
                );

            }
            catch (Exception e)
            {

                _logger.LogError(e, "Error obteniendo pools de IP del router {RouterId}", routerId);
                return ApiResponse<List<IpPoolResponse>>.Error($"Error inesperado: {e.Message}");

            }
        }

        public async Task<ApiResponse<IpPoolResponse>> CreateIpPoolAsync(int routerId, CreateIpPoolRequest createPoolRequest)
        {
            try
            {

                var operation = new CreateIpPoolOperation();

                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, createPoolRequest);

                if (!result.IsSuccess)
                {
                    return ApiResponse<IpPoolResponse>.Error(
                        $"Error creando IP pool: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                var createdId = result.Data!.ToString(); // IpPoolResponse con solo el Id retornado por /add
                var getByIdQuery = new GetIpPoolByIdOperation(createdId);
                var getResult = await _connectionManager.ExecuteQueryAsync(routerId, getByIdQuery);

                if (!getResult.IsSuccess || getResult.Data == null)
                {
                    return ApiResponse<IpPoolResponse>.Error("Pool creado pero no se pudo recuperar el objeto");
                }

                return ApiResponse<IpPoolResponse>.Success(getResult.Data, "IP Pool creado exitosamente");


                //return ApiResponse<IpPoolResponse>.Success(result.Data!, "IP Pool creado exitosamente");


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

                var operation = new UpdateIpPoolOperation();

                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, updateIpPoolRequest);

                if (!result.IsSuccess)
                {
                    return ApiResponse<IpPoolResponse>.Error(
                        $"Error actualizando IP pool: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<IpPoolResponse>.Success(result.Data!, "IP Pool actualizado exitosamente");


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
                var operation = new DeleteIpPoolOperation();
                var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

                if (!result.IsSuccess)
                {
                    return ApiResponse<IpPoolResponse>.Error(
                        $"Error eliminando IP pool: {result.ErrorMessage}",
                        new { ErrorType = result.ErrorType.ToString() }
                    );
                }

                return ApiResponse<IpPoolResponse>.Success(result.Data!, "IP Pool eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando IP pool en router {RouterId}", routerId);
                return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
            }
        }
    }
}
