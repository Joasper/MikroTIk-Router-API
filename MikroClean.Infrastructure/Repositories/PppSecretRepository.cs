using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class PppSecretRepository : BaseRepository<PppSecret>, IPppSecretRepository
    {
        private readonly MikroCleanContext _ctx;

        public PppSecretRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<PppSecret?> GetByRouterAndNameAsync(int routerId, string name)
        {
            return await _ctx.PppSecrets
                .FirstOrDefaultAsync(x => x.RouterId == routerId && x.Name == name);
        }
    }
}
