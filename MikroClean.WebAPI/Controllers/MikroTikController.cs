using MikroClean.Application.Interfaces;
using MikroClean.Domain.MikroTik.Operations;
using MikroClean.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace MikroClean.WebAPI.Controllers
{
    /// <summary>
    /// Controlador para operaciones MikroTik
    /// </summary>
    [Route("api/[controller]")]
    public class MikroTikController : BaseApiController
    {
        private readonly IMikroTikService _mikroTikService;

        public MikroTikController(IMikroTikService mikroTikService)
        {
            _mikroTikService = mikroTikService;
        }

        // ============= GESTIÓN DE CONEXIONES =============

        /// <summary>
        /// Prueba la conexión a un router específico
        /// GET: api/mikrotik/routers/{routerId}/test-connection
        /// </summary>
        [HttpGet("routers/{routerId}/test-connection")]
        public async Task<IActionResult> TestConnection(int routerId)
        {
            var response = await _mikroTikService.TestRouterConnectionAsync(routerId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene el estado de conexión de un router
        /// GET: api/mikrotik/routers/{routerId}/status
        /// </summary>
        [HttpGet("routers/{routerId}/status")]
        public async Task<IActionResult> GetRouterStatus(int routerId)
        {
            var response = await _mikroTikService.GetRouterStatusAsync(routerId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Pre-calienta las conexiones de una organización
        /// POST: api/mikrotik/organizations/{organizationId}/warm-up
        /// </summary>
        [HttpPost("organizations/{organizationId}/warm-up")]
        public async Task<IActionResult> WarmUpConnections(int organizationId)
        {
            var response = await _mikroTikService.WarmUpOrganizationConnectionsAsync(organizationId);
            return HandleResponse(response);
        }

        // ============= INTERFACES =============

        /// <summary>
        /// Crea una interfaz bridge en un router
        /// POST: api/mikrotik/routers/{routerId}/interfaces/bridge
        /// </summary>
        [HttpPost("routers/{routerId}/interfaces/bridge")]
        public async Task<IActionResult> CreateBridge(int routerId, [FromBody] CreateBridgeRequest request)
        {
            var response = await _mikroTikService.CreateBridgeAsync(routerId, request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Crea una interfaz VLAN en un router
        /// POST: api/mikrotik/routers/{routerId}/interfaces/vlan
        /// </summary>
        [HttpPost("routers/{routerId}/interfaces/vlan")]
        public async Task<IActionResult> CreateVlan(int routerId, [FromBody] CreateVlanRequest request)
        {
            var response = await _mikroTikService.CreateVlanAsync(routerId, request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene todas las interfaces de un router
        /// GET: api/mikrotik/routers/{routerId}/interfaces
        /// </summary>
        [HttpGet("routers/{routerId}/interfaces")]
        public async Task<IActionResult> GetAllInterfaces(int routerId)
        {
            var response = await _mikroTikService.GetAllInterfacesAsync(routerId);
            return HandleResponse(response);
        }

        // ============= IP ADDRESS =============

        /// <summary>
        /// Agrega una dirección IP a una interfaz
        /// POST: api/mikrotik/routers/{routerId}/ip/address
        /// </summary>
        [HttpPost("routers/{routerId}/ip/address")]
        public async Task<IActionResult> CreateIpAddress(int routerId, [FromBody] CreateIpAddressRequest request)
        {
            var response = await _mikroTikService.CreateIpAddressAsync(routerId, request);
            return HandleResponse(response);
        }

        // ============= FIREWALL =============

        /// <summary>
        /// Crea una regla de firewall
        /// POST: api/mikrotik/routers/{routerId}/firewall/rules
        /// </summary>
        [HttpPost("routers/{routerId}/firewall/rules")]
        public async Task<IActionResult> CreateFirewallRule(int routerId, [FromBody] CreateFirewallRuleRequest request)
        {
            var response = await _mikroTikService.CreateFirewallRuleAsync(routerId, request);
            return HandleResponse(response);
        }

        // ============= IP POOLS =============

        /// <summary>
        /// Obtiene todos los IP pools de un router
        /// GET: api/mikrotik/routers/{routerId}/ip/pools
        /// </summary>
        [HttpGet("routers/{routerId}/ip/pools")]
        public async Task<IActionResult> GetAllIpPools(int routerId)
        {
            var response = await _mikroTikService.GetAllIpPoolsAsync(routerId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Crea un Pool de una IP
        /// POST: api/mikrotik/routers/{routerId}/ip/pools
        /// </summary>
        [HttpPost("routers/{routerId}/ip/pools")]
        public async Task<IActionResult> CreateIpPoolAsync(int routerId, CreateIpPoolRequest request)
        {
            var response = await _mikroTikService.CreateIpPoolAsync(routerId, request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Actualiza la Pool de una Ip
        /// PUT: api/mikrotik/routers/{routerId}/ip/pools
        /// </summary>
        [HttpPut("routers/{routerId}/ip/pools")]
        public async Task<IActionResult> UpdateIpPoolAsync(int routerId, UpdateIpPoolRequest request)
        {
            var response = await _mikroTikService.UpdateIpPoolAsync(routerId, request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Elimina la pool de una IP
        /// DELETE: api/mikrotik/routers/{routerId}/ip/pools
        /// </summary>
        [HttpDelete("routers/{routerId}/ip/pools")]
        public async Task<IActionResult> DeleteIpPoolAsync(int routerId, DeleteIpPoolRequest request)
        {
            var response = await _mikroTikService.DeleteIpPoolAsync(routerId, request);
            return HandleResponse(response);
        }

        // ============= PPP =============


        [HttpGet("routers/{routerId}/ppp/profiles")]
        public async Task<IActionResult> GetAllPppProfiles(int routerId)
        {
            var response = await _mikroTikService.GetAllPPPoEProfileAsync(routerId);
            return HandleResponse(response);
        }



        /// <summary>
        /// </summary>
        /// <param name="routerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// 

        [HttpPost("routers/{routerId}/ppp/profile")]
        public async Task<IActionResult> CreatePppProfileAsync(int routerId, CreatePPPoEProfile request)
        {
            var response = await _mikroTikService.CreatePPPoEProfileAsync(routerId, request);
            return HandleResponse(response);
        }


        [HttpPut("routers/{routerId}/ppp/profile")]
        public async Task<IActionResult> UpdatePppProfileAsync(int routerId, UpdatePPPoEProfile request)
        {
            var response = await _mikroTikService.UpdatePPPoEProfileAsync(routerId, request);
            return HandleResponse(response);
        }

        [HttpDelete("routers/{routerId}/ppp/profile")]
        public async Task<IActionResult> DeletePppProfileAsync(int routerId, DeletePPPoEProfile request)
        {
            var response = await _mikroTikService.DeletePPPoEProfileAsync(routerId, request);
            return HandleResponse(response);
        }

        [HttpGet("routers/{routerId}/ppp/secrets")]
        public async Task<IActionResult> GetAllPppSecrets(int routerId)
        {
            var response = await _mikroTikService.GetAllPPPoESecretAsync(routerId);
            return HandleResponse(response);
        }

        [HttpPut("routers/{routerId}/ppp/secret")]
        public async Task<IActionResult> UpdatePppSecretAsync(int routerId, UpdatePPPoESecretRequest request)
        {
            var response = await _mikroTikService.UpdatePPPoESecretAsync(routerId, request);
            return HandleResponse(response);
        }

        [HttpPost("routers/{routerId}/ppp/secret")]
        public async Task<IActionResult> CreatePppSecretAsync(int routerId, CreatePPPoESecretRequest request)
        {
            var response = await _mikroTikService.CreatePPPoESecretAsync(routerId, request);
            return HandleResponse(response);
        }

        [HttpDelete("routers/{routerId}/ppp/secret")]
        public async Task<IActionResult> DeletePppSecretAsync(int routerId, DeletePPPoESecretRequest request)
        {
            var response = await _mikroTikService.DeletePPPoESecretAsync(routerId, request);
            return HandleResponse(response);
        }


        [HttpGet("routers/{routerId}/ppp/server")]
        public async Task<IActionResult> GetPppServer(int routerId)
        {
            var response = await _mikroTikService.GetAllPPPoEServerAsync(routerId);
            return HandleResponse(response);
        }

        [HttpPost("routers/{routerId}/ppp/server")]
        public async Task<IActionResult> CreatePppServerAsync(int routerId, CreatePPPoEServerRequest request)
        {
            var response = await _mikroTikService.CreatePPPoEServerAsync(routerId, request);
            return HandleResponse(response);
        }

        [HttpPut("routers/{routerId}/ppp/server")]
        public async Task<IActionResult> UpdatePppServerAsync(int routerId, UpdatePPPoEServerRequest request)
        {
            var response = await _mikroTikService.UpdatePPPoEServerAsync(routerId, request);
            return HandleResponse(response);
        }

        [HttpDelete("routers/{routerId}/ppp/server")]
        public async Task<IActionResult> DeletePppServerAsync(int routerId, DeletePPPoEServerRequest request)
        {
            var response = await _mikroTikService.DeletePPPoEServerAsync(routerId, request);
            return HandleResponse(response);
        }





        // ============= SYSTEM INFO =============

        /// <summary>
        /// Obtiene información de recursos del sistema
        /// GET: api/mikrotik/routers/{routerId}/system/resources
        /// </summary>
        //[HttpGet("routers/{routerId}/system/resources")]
        //public async Task<IActionResult> GetSystemResources(int routerId)
        //{
        //    var response = await _mikroTikService.GetSystemResourcesAsync(routerId);
        //    return HandleResponse(response);
        //}
    }
}
