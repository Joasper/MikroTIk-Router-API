using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface ITaxRepository : IRepository<Tax>
    {
        Task<IEnumerable<Tax>> GetActiveAsync();
        Task<Tax?> GetByNameAsync(string name);
    }
}
