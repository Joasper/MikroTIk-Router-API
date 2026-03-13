using MikroClean.Application.Dtos.User;

namespace MikroClean.Application.Dtos.Auth
{
    public class LoginResponseDTO
    {
        public UserDTO User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
