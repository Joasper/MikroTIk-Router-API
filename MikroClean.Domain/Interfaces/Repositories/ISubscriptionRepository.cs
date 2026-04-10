using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<IEnumerable<Subscription>> GetByClienteIdAsync(int clienteId);
        Task<Subscription?> GetActiveByClienteIdAsync(int clienteId);
        Task<IEnumerable<Subscription>> GetActiveByRouterIdAsync(int routerId);
        Task<Subscription?> GetByRouterAndPppSecretNameAsync(int routerId, string pppSecretName);
    }
}
