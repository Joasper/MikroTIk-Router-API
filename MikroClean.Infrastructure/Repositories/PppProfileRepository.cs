using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class PppProfileRepository : BaseRepository<PppProfile>, IPppProfileRepository
    {
        private readonly MikroCleanContext _ctx;

        public PppProfileRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<PppProfile?> GetByRouterAndNameAsync(int routerId, string name)
        {
            return await _ctx.PppProfiles
                .FirstOrDefaultAsync(x => x.RouterId == routerId && x.Name == name);
        }
    }
}
