using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
    {
        private readonly MikroCleanContext _ctx;

        public ClienteRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Cliente>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _ctx.Clientes
                .Where(x => x.OrganizationId == organizationId && x.DeletedAt == null)
                .OrderBy(x => x.Nombre)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByCedulaAsync(string cedula)
        {
            return await _ctx.Clientes
                .FirstOrDefaultAsync(x => x.Cedula == cedula && x.DeletedAt == null);
        }

        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            return await _ctx.Clientes
                .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null);
        }
    }
}
