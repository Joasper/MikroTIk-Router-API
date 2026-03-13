using MikroClean.Application.Dtos.User;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserDTO>> GetUserByIdAsync(int id);
        Task<ApiResponse<IEnumerable<UserDTO>>> GetAllUsersAsync();
        Task<ApiResponse<PagedResult<UserDTO>>> GetUsersPagedAsync(PaginationParams paginationParams);
        Task<ApiResponse<IEnumerable<UserDTO>>> GetUsersByOrganizationAsync(int organizationId);
        Task<ApiResponse<UserDTO>> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<ApiResponse<UserDTO>> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO);
        Task<ApiResponse<bool>> DeleteUserAsync(int id);
        Task<ApiResponse<bool>> ChangePasswordAsync(int id, string currentPassword, string newPassword);
    }
}
