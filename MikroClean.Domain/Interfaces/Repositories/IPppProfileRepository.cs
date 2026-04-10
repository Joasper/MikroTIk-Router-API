using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IPppProfileRepository : IRepository<PppProfile>
    {
        Task<PppProfile?> GetByRouterAndNameAsync(int routerId, string name);
    }
}
