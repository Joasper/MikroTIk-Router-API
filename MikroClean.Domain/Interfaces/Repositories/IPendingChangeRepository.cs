using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IPendingChangeRepository : IRepository<PendingChange>
    {
        Task<List<PendingChange>> GetReadyPendingByRouterAsync(int routerId, int maxItems = 100);
        Task<List<int>> GetRouterIdsWithReadyPendingAsync(int maxRouters = 100);
    }
}
