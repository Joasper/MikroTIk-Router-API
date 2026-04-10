using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class BillingTemplateRepository : BaseRepository<BillingTemplate>, IBillingTemplateRepository
    {
        private readonly MikroCleanContext _ctx;

        public BillingTemplateRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<BillingTemplate?> GetActiveByClienteIdAsync(int clienteId)
        {
            return await _ctx.BillingTemplates
                .FirstOrDefaultAsync(x => x.ClienteId == clienteId && x.IsActive && x.DeletedAt == null);
        }

        public async Task<IEnumerable<BillingTemplate>> GetTemplatesDueOnDayAsync(int dayOfMonth)
        {
            return await _ctx.BillingTemplates
                .Where(x => x.IsActive && x.DiaCutoff == dayOfMonth && x.DeletedAt == null)
                .ToListAsync();
        }
    }
}
