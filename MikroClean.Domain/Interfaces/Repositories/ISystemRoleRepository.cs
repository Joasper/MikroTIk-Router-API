using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface ISystemRoleRepository : IRepository<SystemRole>
    {
        Task<SystemRole?> GetByNameAsync(string name);
        Task<SystemRole?> GetRoleWithPermissionsAsync(int roleId);
    }
}
