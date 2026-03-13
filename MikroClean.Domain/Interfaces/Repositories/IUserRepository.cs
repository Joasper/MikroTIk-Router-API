using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<User?> GetUserWithRoleAsync(int userId);
        Task<IEnumerable<User>> GetUsersByOrganizationAsync(int organizationId);
    }
}
