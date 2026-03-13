using MikroClean.Application.Dtos.Auth;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDTO>> LoginAsync(LoginRequestDTO loginRequest);
        Task<ApiResponse<bool>> ValidateTokenAsync(string token);
        Task<ApiResponse<bool>> LogoutAsync(int userId);
    }
}
