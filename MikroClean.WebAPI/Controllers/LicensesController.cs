using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.License;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class LicensesController : BaseApiController
    {
        private readonly ILicenseService _licenseService;

        public LicensesController(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        /// <summary>
        /// Obtiene todas las licencias activas
        /// </summary>
        /// <returns>Lista de licencias</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllLicenses()
        {
            var response = await _licenseService.GetAllLicensesAsync();
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene licencias disponibles para asignar (sin organización)
        /// </summary>
        /// <returns>Lista de licencias disponibles</returns>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableLicenses()
        {
            var response = await _licenseService.GetAvailableLicensesAsync();
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene licencias con paginación, búsqueda y filtros
        /// </summary>
        /// <param name="pageNumber">Número de página (default: 1)</param>
        /// <param name="pageSize">Tamaño de página (default: 10, max: 100)</param>
        /// <param name="sortBy">Campo para ordenar (key, type, startDate, endDate, isActive, createdAt)</param>
        /// <param name="sortDescending">Orden descendente (default: false)</param>
        /// <param name="searchTerm">Término de búsqueda (busca en key)</param>
        /// <param name="filterByType">Filtrar por tipo (1=Basic, 2=Pro, 3=Enterprise, 4=Trial)</param>
        /// <param name="filterByStatus">Filtrar por estado activo (true/false)</param>
        /// <param name="filterExpired">Filtrar expiradas (true) o vigentes (false)</param>
        /// <returns>Resultado paginado de licencias</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LicenseDTO>>), 200)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? filterByType = null,
            [FromQuery] bool? filterByStatus = null,
            [FromQuery] bool? filterExpired = null)
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending,
                SearchTerm = searchTerm
            };

            var response = await _licenseService.GetLicensesPagedAsync(
                paginationParams, 
                filterByType, 
                filterByStatus, 
                filterExpired
            );
            return HandleResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLicenseById(int id)
        {
            var response = await _licenseService.GetLicenseByIdAsync(id);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene una licencia por su clave
        /// </summary>
        /// <param name="key">Clave de la licencia</param>
        /// <returns>Datos de la licencia</returns>
        [HttpGet("by-key/{key}")]
        [ProducesResponseType(typeof(ApiResponse<LicenseDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<LicenseDTO>), 404)]
        public async Task<IActionResult> GetLicenseByKey(string key)
        {
            var response = await _licenseService.GetLicenseByKeyAsync(key);
            return HandleResponse(response);
        }

        [HttpGet("organization/{organizationId}")]
        public async Task<IActionResult> GetLicenseByOrganization(int organizationId)
        {
            var response = await _licenseService.GetLicenseByOrganizationIdAsync(organizationId);
            return HandleResponse(response);
        }

        [HttpGet("expired")]
        public async Task<IActionResult> GetExpiredLicenses()
        {
            var response = await _licenseService.GetExpiredLicensesAsync();
            return HandleResponse(response);
        }

        [HttpGet("expiring/{days}")]
        public async Task<IActionResult> GetLicensesExpiringInDays(int days)
        {
            var response = await _licenseService.GetLicensesExpiringInDaysAsync(days);
            return HandleResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLicense([FromBody] CreateLicenseDTO createLicenseDTO)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationError();
            }

            var response = await _licenseService.CreateLicenseAsync(createLicenseDTO);
            return HandleResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLicense(int id, [FromBody] UpdateLicenseDTO updateLicenseDTO)
        {
            if (!ModelState.IsValid)
            {
                return HandleValidationError();
            }

            var response = await _licenseService.UpdateLicenseAsync(id, updateLicenseDTO);
            return HandleResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLicense(int id)
        {
            var response = await _licenseService.DeleteLicenseAsync(id);
            return HandleResponse(response);
        }

        [HttpPost("validate/{organizationId}")]
        public async Task<IActionResult> ValidateLicense(int organizationId)
        {
            var response = await _licenseService.ValidateLicenseAsync(organizationId);
            return HandleResponse(response);
        }
    }
}
