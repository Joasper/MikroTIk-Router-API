using MikroClean.Application.Dtos.Interfaces;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces;

/// <summary>
/// Servicio para gestionar interfaces de routers MikroTik
/// </summary>
public interface IInterfaceService
{
    /// <summary>
    /// Obtiene todas las interfaces de un router con paginación y filtrado
    /// </summary>
    Task<ApiResponse<PagedResult<RouterInterfaceDTO>>> GetInterfacesPagedAsync(
        int routerId, 
        PaginationParams paginationParams,
        string? typeFilter = null,
        bool? runningFilter = null,
        bool? disabledFilter = null);

    /// <summary>
    /// Obtiene los detalles de una interfaz específica por su MikroTikId
    /// </summary>
    Task<ApiResponse<RouterInterfaceDTO>> GetInterfaceByIdAsync(int routerId, string mikroTikId);

    /// <summary>
    /// Actualiza una interfaz existente en el router (nombre, MTU, disabled, comment)
    /// </summary>
    Task<ApiResponse<RouterInterfaceResponse>> UpdateInterfaceAsync(
        int routerId, 
        UpdateRouterInterfaceDTO request);

    /// <summary>
    /// Sincroniza todas las interfaces del router a la base de datos local
    /// </summary>
    Task<ApiResponse<bool>> SyncInterfacesAsync(int routerId);
}
