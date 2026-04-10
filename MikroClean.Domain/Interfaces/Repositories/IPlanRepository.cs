using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IPlanRepository : IRepository<Plan>
    {
        Task<IEnumerable<Plan>> GetByRouterIdAsync(int routerId);
    }
}
