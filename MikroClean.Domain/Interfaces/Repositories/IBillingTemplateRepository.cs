using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IBillingTemplateRepository : IRepository<BillingTemplate>
    {
        Task<BillingTemplate?> GetActiveByClienteIdAsync(int clienteId);
        Task<IEnumerable<BillingTemplate>> GetTemplatesDueOnDayAsync(int dayOfMonth);
    }
}
