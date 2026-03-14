using MikroClean.Application.Models;
using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestionar operaciones de routers MikroTik
    /// </summary>
    public interface IMikroTikService
    {
        // ============= GESTI�N DE CONEXIONES =============
        
        /// <summary>
        /// Prueba la conexi�n a un router espec�fico
        /// </summary>
        Task<ApiResponse<RouterConnectionStatus>> TestRouterConnectionAsync(int routerId);

        /// <summary>
        /// Obtiene el estado de conexi�n de un router
        /// </summary>
        Task<ApiResponse<RouterConnectionStatus>> GetRouterStatusAsync(int routerId);

        /// <summary>
        /// Pre-calienta las conexiones de todos los routers de una organizaci�n
        /// </summary>
        Task<ApiResponse<Dictionary<int, bool>>> WarmUpOrganizationConnectionsAsync(int organizationId);

        // ============= INTERFACES =============
        
        /// <summary>
        /// Crea una interfaz bridge en un router
        /// </summary>
        Task<ApiResponse<BridgeResponse>> CreateBridgeAsync(int routerId, CreateBridgeRequest request);

        /// <summary>
        /// Crea una interfaz VLAN en un router
        /// </summary>
        Task<ApiResponse<VlanResponse>> CreateVlanAsync(int routerId, CreateVlanRequest request);

        /// <summary>
        /// Obtiene todas las interfaces de un router
        /// </summary>
        Task<ApiResponse<List<InterfaceResponse>>> GetAllInterfacesAsync(int routerId);

        // ============= IP ADDRESS =============
        
        /// <summary>
        /// Agrega una direcci�n IP a una interfaz
        /// </summary>
        Task<ApiResponse<IpAddressResponse>> CreateIpAddressAsync(int routerId, CreateIpAddressRequest request);

        // ============= FIREWALL =============
        
        /// <summary>
        /// Crea una regla de firewall
        /// </summary>
        Task<ApiResponse<FirewallRuleResponse>> CreateFirewallRuleAsync(int routerId, CreateFirewallRuleRequest request);

        // ============= SYSTEM INFO =============

        /// <summary>
        /// Obtiene informaci�n de recursos del sistema
        /// </summary>
        //Task<ApiResponse<FirewallRuleResponse>> CreateFirewallRuleAsync(int routerId, CreateFirewallRuleRequest request);

        // ============= IP POOLS =============

        /// <summary>
        /// Obtiene los pools de IP de un router con paginacin
        /// </summary>
        Task<ApiResponse<PagedResult<IpPoolResponse>>> GetIpPoolsPagedAsync(int routerId, PaginationParams paginationParams);

        Task<ApiResponse<IpPoolResponse>> CreateIpPoolAsync(int routerId, CreateIpPoolRequest createPoolRequest);
        Task<ApiResponse<IpPoolResponse>> UpdateIpPoolAsync(int routerId, UpdateIpPoolRequest updateIpPoolRequest);

        Task<ApiResponse<IpPoolResponse>> DeleteIpPoolAsync(int routerId, DeleteIpPoolRequest request);


        // ============= PPP =============

        /// <summary>
        /// Obtiene informaci�n de recursos del sistema
        /// </summary>
        Task<ApiResponse<List<PPPoEProfileResponse>>> GetAllPPPoEProfileAsync(int routerId);

        Task<ApiResponse<PPPoEProfileResponse>> CreatePPPoEProfileAsync(int routerId, CreatePPPoEProfile createPPPoEProfile);
        Task<ApiResponse<PPPoEProfileResponse>> UpdatePPPoEProfileAsync(int routerId, UpdatePPPoEProfile updatePPPoEProfile);

        Task<ApiResponse<PPPoEProfileResponse>> DeletePPPoEProfileAsync(int routerId, DeletePPPoEProfile request);

        Task<ApiResponse<List<PPPoESecretResponse>>> GetAllPPPoESecretAsync(int routerId);
        Task<ApiResponse<PPPoESecretResponse>> CreatePPPoESecretAsync(int routerId, CreatePPPoESecretRequest createPPPoESecret);
        Task<ApiResponse<PPPoESecretResponse>> UpdatePPPoESecretAsync(int routerId, UpdatePPPoESecretRequest updatePPPoESecret);
        Task<ApiResponse<PPPoESecretResponse>> DeletePPPoESecretAsync(int routerId, DeletePPPoESecretRequest request);

        Task<ApiResponse<List<PPPoEServerResponse>>> GetAllPPPoEServerAsync(int routerId);
        Task<ApiResponse<PPPoEServerResponse>> CreatePPPoEServerAsync(int routerId, CreatePPPoEServerRequest createPPPoEServer);
        Task<ApiResponse<PPPoEServerResponse>> UpdatePPPoEServerAsync(int routerId, UpdatePPPoEServerRequest updatePPPoEServer);
        Task<ApiResponse<PPPoEServerResponse>> DeletePPPoEServerAsync(int routerId, DeletePPPoEServerRequest request);



        // ============= OPERACIONES EN BATCH =============

        /// <summary>
        /// Ejecuta una operaci�n en m�ltiples routers de una organizaci�n
        /// </summary>
        Task<ApiResponse<Dictionary<int, MikroTikResult<TResponse>>>> ExecuteOnMultipleRoutersAsync<TRequest, TResponse>(
            int organizationId,
            IMikroTikOperation<TRequest, TResponse> operation,
            TRequest request);
    }
}
