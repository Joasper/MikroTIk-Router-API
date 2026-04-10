using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IPppSecretRepository : IRepository<PppSecret>
    {
        Task<PppSecret?> GetByRouterAndNameAsync(int routerId, string name);
    }
}
