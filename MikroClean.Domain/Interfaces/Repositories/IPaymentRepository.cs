using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByClienteIdAsync(int clienteId);
        Task<Payment?> GetWithMappingsAsync(int paymentId);
    }
}
