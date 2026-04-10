using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice?> GetByIdWithDetailsAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<Invoice>> GetByClienteAndStatusAsync(int clienteId, InvoiceStatus status);
        Task<IEnumerable<Invoice>> GetPendingByClienteIdAsync(int clienteId);
        Task<string?> GetLastInvoiceNumberByPeriodAsync(string period);
    }
}
