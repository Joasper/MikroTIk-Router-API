using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class PppServerRepository : BaseRepository<PppServer>, IPppServerRepository
    {
        private readonly MikroCleanContext _ctx;

        public PppServerRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<PppServer?> GetByRouterAndNameAsync(int routerId, string name)
        {
            return await _ctx.PppServers
                .FirstOrDefaultAsync(x => x.RouterId == routerId && x.Name == name);
        }
    }
}
