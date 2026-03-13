using MikroClean.Application.Dtos.Router;
using MikroClean.Application.Interfaces;
using MikroClean.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace MikroClean.WebAPI.Controllers
{
    /// <summary>
    /// Controlador para gestión de routers (CRUD)
    /// </summary>
    [Route("api/[controller]")]
    public class RoutersController : BaseApiController
    {
        private readonly IRouterService _routerService;

        public RoutersController(IRouterService routerService)
        {
            _routerService = routerService;
        }

        /// <summary>
        /// Obtiene todos los routers de una organización
        /// GET: api/routers/organization/{organizationId}
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<IActionResult> GetByOrganization(int organizationId)
        {
            var response = await _routerService.GetRoutersByOrganizationAsync(organizationId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene un router por ID
        /// GET: api/routers/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _routerService.GetRouterByIdAsync(id);
            return HandleResponse(response);
        }

        /// <summary>
        /// Crea un nuevo router (password se encripta automáticamente)
        /// POST: api/routers
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRouterDTO createDto)
        {
            if (!ModelState.IsValid)
                return HandleValidationError();

            var response = await _routerService.CreateRouterAsync(createDto);
            return HandleResponse(response);
        }

        /// <summary>
        /// Actualiza un router existente
        /// PUT: api/routers/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRouterDTO updateDto)
        {
            if (!ModelState.IsValid)
                return HandleValidationError();

            var response = await _routerService.UpdateRouterAsync(id, updateDto);
            return HandleResponse(response);
        }

        /// <summary>
        /// Elimina un router (soft delete)
        /// DELETE: api/routers/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _routerService.DeleteRouterAsync(id);
            return HandleResponse(response);
        }

        /// <summary>
        /// Prueba la conexión a un router y actualiza su estado
        /// POST: api/routers/{id}/test
        /// </summary>
        [HttpPost("{id}/test")]
        public async Task<IActionResult> TestConnection(int id)
        {
            var response = await _routerService.TestAndUpdateRouterStatusAsync(id);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene los IP Pools de un router
        /// GET: api/routers/{id}/ip/pools
        /// </summary>
        //[HttpGet("{id}/ip/pools")]
        //public async Task<IActionResult> GetIpPoools(int id)
        //{
        //    var response = await _routerService.GetRouterIpPoolsAsync(id);
        //    return HandleResponse(response);
        //}

        /// <summary>
        /// Reinicia un router MikroTik
        /// POST: api/routers/{id}/reboot
        /// </summary>
        [HttpPost("{id}/reboot")]
        public async Task<IActionResult> RebootRouter(int id)
        {
            var response = await _routerService.RebootRouterAsync(id);
            return HandleResponse(response);
        }
    }
}
