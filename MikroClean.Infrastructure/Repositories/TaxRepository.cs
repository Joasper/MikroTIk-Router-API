using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class TaxRepository : BaseRepository<Tax>, ITaxRepository
    {
        private readonly MikroCleanContext _ctx;

        public TaxRepository(MikroCleanContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<Tax>> GetActiveAsync()
        {
            return await _ctx.Taxes
                .Where(x => x.IsActive && x.DeletedAt == null)
                .OrderBy(x => x.Nombre)
                .ToListAsync();
        }

        public async Task<Tax?> GetByNameAsync(string name)
        {
            return await _ctx.Taxes
                .FirstOrDefaultAsync(x => x.Nombre == name && x.DeletedAt == null);
        }
    }
}
