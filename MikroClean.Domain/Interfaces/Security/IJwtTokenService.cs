using MikroClean.Domain.Entities;

namespace MikroClean.Domain.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
}
