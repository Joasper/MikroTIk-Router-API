using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
    {
        private readonly MikroCleanContext _ctx;

        public SubscriptionRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Subscription>> GetByClienteIdAsync(int clienteId)
        {
            return await _ctx.Subscripciones
                .Include(x => x.Plan)
                .Include(x => x.PppSecret)
                .Where(x => x.ClienteId == clienteId && x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetActiveByClienteIdAsync(int clienteId)
        {
            return await _ctx.Subscripciones
                .Include(x => x.Plan)
                .Include(x => x.PppSecret)
                .FirstOrDefaultAsync(x => x.ClienteId == clienteId && x.Estado == SubscriptionStatus.Activa && x.DeletedAt == null);
        }

        public async Task<IEnumerable<Subscription>> GetActiveByRouterIdAsync(int routerId)
        {
            return await _ctx.Subscripciones
                .Include(x => x.Cliente)
                .Include(x => x.Plan)
                .Include(x => x.PppSecret)
                .Where(x => x.Estado == SubscriptionStatus.Activa
                    && x.DeletedAt == null
                    && x.PppSecret != null
                    && x.PppSecret.RouterId == routerId)
                .ToListAsync();
        }

        public async Task<Subscription?> GetByRouterAndPppSecretNameAsync(int routerId, string pppSecretName)
        {
            return await _ctx.Subscripciones
                .Include(x => x.Cliente)
                .Include(x => x.Plan)
                .Include(x => x.PppSecret)
                .FirstOrDefaultAsync(x => x.PppSecret != null && x.PppSecret.RouterId == routerId && x.PppSecret.Name == pppSecretName && x.DeletedAt == null);
        }
    }
}
