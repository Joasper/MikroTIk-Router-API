using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class PendingChangeRepository : BaseRepository<PendingChange>, IPendingChangeRepository
    {
        private readonly MikroCleanContext _ctx;

        public PendingChangeRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<PendingChange>> GetReadyPendingByRouterAsync(int routerId, int maxItems = 100)
        {
            return await _ctx.PendingChanges
                .Where(x => x.RouterId == routerId
                            && x.Status == PendingChangeStatus.Pending
                            && x.NextRetryAt <= DateTime.UtcNow)
                .OrderBy(x => x.CreatedAt)
                .Take(maxItems)
                .ToListAsync();
        }

        public async Task<List<int>> GetRouterIdsWithReadyPendingAsync(int maxRouters = 100)
        {
            return await _ctx.PendingChanges
                .Where(x => x.Status == PendingChangeStatus.Pending
                            && x.NextRetryAt <= DateTime.UtcNow)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.RouterId)
                .Distinct()
                .Take(maxRouters)
                .ToListAsync();
        }
    }
}
