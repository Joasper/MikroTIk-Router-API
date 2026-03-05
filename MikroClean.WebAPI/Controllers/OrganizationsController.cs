using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.Organization;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : BaseApiController
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        /// <summary>
        /// Obtiene todas las organizaciones activas
        /// </summary>
        /// <returns>Lista de organizaciones</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrganizationDTO>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _organizationService.GetAllOrganizationsAsync();
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene una organización por su ID
        /// </summary>
        /// <param name="id">ID de la organización</param>
        /// <returns>Datos de la organización</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            var response = await _organizationService.GetOrganizationByIdAsync(id);
            return HandleResponse(response);
        }

        /// <summary>
        /// Crea una nueva organización
        /// </summary>
        /// <param name="createDto">Datos de la organización a crear</param>
        /// <returns>Organización creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 400)]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationDTO createDto)
        {
            if (!ModelState.IsValid)
                return HandleValidationError();

            var response = await _organizationService.CreateOrganizationAsync(createDto);
            
            // Si es exitoso, retornar 201 Created con Location header
            if (response.Status == "success" && response.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = response.Data.Id }, 
                    response
                );
            }

            return HandleResponse(response);
        }

        /// <summary>
        /// Actualiza una organización existente
        /// </summary>
        /// <param name="id">ID de la organización</param>
        /// <param name="updateDto">Datos a actualizar</param>
        /// <returns>Organización actualizada</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 404)]
        [ProducesResponseType(typeof(ApiResponse<OrganizationDTO>), 400)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationDTO updateDto)
        {
            if (!ModelState.IsValid)
                return HandleValidationError();

            var response = await _organizationService.UpdateOrganizationAsync(id, updateDto);
            return HandleResponse(response);
        }

        /// <summary>
        /// Elimina (soft delete) una organización
        /// </summary>
        /// <param name="id">ID de la organización</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _organizationService.DeleteOrganizationAsync(id);
            return HandleResponse(response);
        }
    }
}
