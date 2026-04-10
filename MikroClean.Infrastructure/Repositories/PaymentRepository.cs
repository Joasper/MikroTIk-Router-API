using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        private readonly MikroCleanContext _ctx;

        public PaymentRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Payment>> GetByClienteIdAsync(int clienteId)
        {
            return await _ctx.Payments
                .Include(x => x.FacturasAplicadas)
                .Where(x => x.ClienteId == clienteId && x.DeletedAt == null)
                .OrderByDescending(x => x.FechaPago)
                .ToListAsync();
        }

        public async Task<Payment?> GetWithMappingsAsync(int paymentId)
        {
            return await _ctx.Payments
                .Include(x => x.FacturasAplicadas)
                .FirstOrDefaultAsync(x => x.Id == paymentId && x.DeletedAt == null);
        }
    }
}
