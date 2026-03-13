using MikroClean.Application.Dtos.Router;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestionar routers (CRUD con encriptación de passwords)
    /// </summary>
    public interface IRouterService
    {
        /// <summary>
        /// Obtiene todos los routers de una organización
        /// </summary>
        Task<ApiResponse<IEnumerable<RouterDTO>>> GetRoutersByOrganizationAsync(int organizationId);

        /// <summary>
        /// Obtiene un router por ID
        /// </summary>
        Task<ApiResponse<RouterDTO>> GetRouterByIdAsync(int routerId);

        /// <summary>
        /// Crea un nuevo router (encripta la contraseña automáticamente)
        /// </summary>
        Task<ApiResponse<RouterDTO>> CreateRouterAsync(CreateRouterDTO createDto);

        /// <summary>
        /// Actualiza un router existente
        /// </summary>
        Task<ApiResponse<RouterDTO>> UpdateRouterAsync(int routerId, UpdateRouterDTO updateDto);

        /// <summary>
        /// Elimina (soft delete) un router
        /// </summary>
        Task<ApiResponse<bool>> DeleteRouterAsync(int routerId);

        /// <summary>
        /// Prueba la conexión a un router y actualiza su estado
        /// </summary>
        Task<ApiResponse<bool>> TestAndUpdateRouterStatusAsync(int routerId);

        /// <summary>
        /// Obtiene los Ip Pools de un router específico
        /// </summary>
        //Task<ApiResponse<IEnumerable<RouterIpPoolDTO>>> GetRouterIpPoolsAsync(int routerId);

        /// <summary>
        /// Reinicia un router MikroTik
        /// </summary>
        Task<ApiResponse<bool>> RebootRouterAsync(int routerId);
    }
}
