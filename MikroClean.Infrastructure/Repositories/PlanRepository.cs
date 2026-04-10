using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class PlanRepository : BaseRepository<Plan>, IPlanRepository
    {
        private readonly MikroCleanContext _ctx;

        public PlanRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Plan>> GetByRouterIdAsync(int routerId)
        {
            return await _ctx.Planes
                .Where(x => x.RouterId == routerId && x.DeletedAt == null)
                .OrderBy(x => x.VelocidadMbps)
                .ToListAsync();
        }
    }
}
