using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        private readonly MikroCleanContext _ctx;

        public InvoiceRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<Invoice?> GetByIdWithDetailsAsync(int invoiceId)
        {
            return await _ctx.Invoices
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && x.DeletedAt == null);
        }

        public async Task<IEnumerable<Invoice>> GetByClienteIdAsync(int clienteId)
        {
            return await _ctx.Invoices
                .Include(x => x.Detalles)
                .Where(x => x.ClienteId == clienteId && x.DeletedAt == null)
                .OrderByDescending(x => x.FechaEmision)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByClienteAndStatusAsync(int clienteId, InvoiceStatus status)
        {
            return await _ctx.Invoices
                .Include(x => x.Detalles)
                .Where(x => x.ClienteId == clienteId && x.Estado == status && x.DeletedAt == null)
                .OrderByDescending(x => x.FechaEmision)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetPendingByClienteIdAsync(int clienteId)
        {
            return await _ctx.Invoices
                .Where(x => x.ClienteId == clienteId
                    && x.DeletedAt == null
                    && (x.Estado == InvoiceStatus.Vigente || x.Estado == InvoiceStatus.Abonada || x.Estado == InvoiceStatus.Vencida))
                .OrderBy(x => x.FechaVencimiento)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<string?> GetLastInvoiceNumberByPeriodAsync(string period)
        {
            return await _ctx.Invoices
                .Where(x => x.Periodo == period && x.DeletedAt == null)
                .OrderByDescending(x => x.NumeroFactura)
                .Select(x => x.NumeroFactura)
                .FirstOrDefaultAsync();
        }
    }
}
