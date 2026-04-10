using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IPppServerRepository : IRepository<PppServer>
    {
        Task<PppServer?> GetByRouterAndNameAsync(int routerId, string name);
    }
}
