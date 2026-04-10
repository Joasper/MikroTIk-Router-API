using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> GetByOrganizationIdAsync(int organizationId);
        Task<Cliente?> GetByCedulaAsync(string cedula);
        Task<Cliente?> GetByEmailAsync(string email);
    }
}
