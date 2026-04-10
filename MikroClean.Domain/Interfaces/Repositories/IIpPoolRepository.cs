using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IIpPoolRepository : IRepository<IpPool>
    {
        Task<IpPool?> GetByRouterAndNameAsync(int routerId, string name);
    }
}
