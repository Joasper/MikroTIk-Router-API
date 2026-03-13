using MikroClean.Application.Dtos.SystemRole;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface ISystemRoleService
    {
        Task<ApiResponse<SystemRoleDTO>> GetRoleByIdAsync(int id);
        Task<ApiResponse<SystemRoleDTO>> GetRoleByNameAsync(string name);
        Task<ApiResponse<IEnumerable<SystemRoleDTO>>> GetAllRolesAsync();
        Task<ApiResponse<SystemRoleDTO>> EnsureDefaultRoleExistsAsync(string roleName);
    }
}
