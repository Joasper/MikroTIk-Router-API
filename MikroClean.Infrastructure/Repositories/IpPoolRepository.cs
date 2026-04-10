using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class IpPoolRepository : BaseRepository<IpPool>, IIpPoolRepository
    {
        private readonly MikroCleanContext _ctx;

        public IpPoolRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<IpPool?> GetByRouterAndNameAsync(int routerId, string name)
        {
            return await _ctx.IpPools
                .FirstOrDefaultAsync(x => x.RouterId == routerId && x.Name == name);
        }
    }
}
