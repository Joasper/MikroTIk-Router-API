using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.Interfaces;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;

namespace MikroClean.WebAPI.Controllers;

/// <summary>
/// Controlador para gestionar interfaces de routers MikroTik
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterfacesController : ControllerBase
{
    private readonly IInterfaceService _interfaceService;
    private readonly ILogger<InterfacesController> _logger;

    public InterfacesController(
        IInterfaceService interfaceService,
        ILogger<InterfacesController> logger)
    {
        _interfaceService = interfaceService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las interfaces de un router con paginación y filtrado
    /// </summary>
    /// <param name="routerId">ID del router</param>
    /// <param name="pageNumber">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 20)</param>
    /// <param name="searchTerm">Término de búsqueda (nombre, tipo, MAC, comentario)</param>
    /// <param name="sortBy">Campo para ordenar (Name, Type, Running, etc.)</param>
    /// <param name="sortDescending">Orden descendente (default: false)</param>
    /// <param name="typeFilter">Filtrar por tipo (ether, vlan, bridge, wireless, etc.)</param>
    /// <param name="runningFilter">Filtrar por running (true/false)</param>
    /// <param name="disabledFilter">Filtrar por disabled (true/false)</param>
    [HttpGet("router/{routerId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RouterInterfaceDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RouterInterfaceDTO>>>> GetInterfaces(
        [FromRoute] int routerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? typeFilter = null,
        [FromQuery] bool? runningFilter = null,
        [FromQuery] bool? disabledFilter = null)
    {
        try
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _interfaceService.GetInterfacesPagedAsync(
                routerId, 
                paginationParams,
                typeFilter,
                runningFilter,
                disabledFilter);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo interfaces del router {RouterId}", routerId);
            return StatusCode(500, ApiResponse<PagedResult<RouterInterfaceDTO>>.Error(
                $"Error interno del servidor: {ex.Message}"));
        }
    }

    /// <summary>
    /// Obtiene los detalles de una interfaz específica por su MikroTikId
    /// </summary>
    /// <param name="routerId">ID del router</param>
    /// <param name="mikroTikId">ID de MikroTik de la interfaz (ej: "*3")</param>
    [HttpGet("router/{routerId}/{mikroTikId}")]
    [ProducesResponseType(typeof(ApiResponse<RouterInterfaceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouterInterfaceDTO>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RouterInterfaceDTO>>> GetInterface(
        [FromRoute] int routerId,
        [FromRoute] string mikroTikId)
    {
        try
        {
            var result = await _interfaceService.GetInterfaceByIdAsync(routerId, mikroTikId);

            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo interfaz {MikroTikId} del router {RouterId}", 
                mikroTikId, routerId);
            return StatusCode(500, ApiResponse<RouterInterfaceDTO>.Error(
                $"Error interno del servidor: {ex.Message}"));
        }
    }

    /// <summary>
    /// Actualiza una interfaz existente en el router (nombre, MTU, disabled, comment)
    /// </summary>
    /// <param name="routerId">ID del router</param>
    /// <param name="request">Datos de actualización</param>
    [HttpPut("router/{routerId}")]
    [ProducesResponseType(typeof(ApiResponse<RouterInterfaceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouterInterfaceResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RouterInterfaceResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RouterInterfaceResponse>>> UpdateInterface(
        [FromRoute] int routerId,
        [FromBody] UpdateRouterInterfaceDTO request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.MikroTikId))
            {
                return BadRequest(ApiResponse<RouterInterfaceResponse>.Error(
                    "El campo MikroTikId es requerido"));
            }

            var result = await _interfaceService.UpdateInterfaceAsync(routerId, request);

            if (result.IsWarning)
            {
                return StatusCode(207, result); // Multi-Status para warning
            }

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando interfaz del router {RouterId}", routerId);
            return StatusCode(500, ApiResponse<RouterInterfaceResponse>.Error(
                $"Error interno del servidor: {ex.Message}"));
        }
    }

    /// <summary>
    /// Sincroniza manualmente todas las interfaces del router a la base de datos local
    /// </summary>
    /// <param name="routerId">ID del router</param>
    [HttpPost("router/{routerId}/sync")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> SyncInterfaces([FromRoute] int routerId)
    {
        try
        {
            var result = await _interfaceService.SyncInterfacesAsync(routerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando interfaces del router {RouterId}", routerId);
            return StatusCode(500, ApiResponse<bool>.Error(
                $"Error interno del servidor: {ex.Message}"));
        }
    }
}
