using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MikroClean.Infrastructure.Repositories
{
    public class RouterRepository : BaseRepository<Router>, IRouterRepository
    {
        private readonly MikroCleanContext _context;

        public RouterRepository(MikroCleanContext ctx) : base(ctx)
        {
            _context = ctx;
        }

        public async Task<IEnumerable<Router>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _context.Set<Router>()
                .Where(r => r.OrganizationId == organizationId && r.DeletedAt == null)
                .Include(r => r.Organization)
                .Include(r => r.RouterStatus)
                .ToListAsync();
        }

        public async Task<Router?> GetByIpAsync(string ip)
        {
            return await _context.Set<Router>()
                .FirstOrDefaultAsync(r => r.Ip == ip && r.DeletedAt == null);
        }

        public async Task<IEnumerable<Router>> GetAvailableRoutersAsync(int organizationId)
        {
            return await _context.Set<Router>()
                .Where(r => r.OrganizationId == organizationId 
                    && r.IsActive 
                    && r.DeletedAt == null)
                .Include(r => r.Organization)
                .ToListAsync();
        }

        public async Task UpdateLastSeenAsync(int routerId, DateTime lastSeen)
        {
            var router = await _context.Set<Router>()
                .FirstOrDefaultAsync(r => r.Id == routerId);

            if (router != null)
            {
                router.LastSeen = lastSeen;
                _context.Set<Router>().Update(router);
                await _context.SaveChangesAsync();
            }
        }
    }
}
