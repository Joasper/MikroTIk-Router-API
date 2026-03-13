using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class LicenseRepository : BaseRepository<License>, ILicenseRepository
    {
        private readonly MikroCleanContext _context;

        public LicenseRepository(MikroCleanContext context) : base(context)
        {
            _context = context;
        }

        public async Task<License?> GetByOrganizationIdAsync(int organizationId)
        {
            return await _context.Licenses
                .Include(l => l.Organization)
                .FirstOrDefaultAsync(l => l.OrganizationId == organizationId && l.DeletedAt == null);
        }

        public async Task<License?> GetByKeyAsync(string key)
        {
            return await _context.Licenses
                .Include(l => l.Organization)
                .FirstOrDefaultAsync(l => l.Key == key && l.DeletedAt == null);
        }

        public async Task<IEnumerable<License>> GetExpiredLicensesAsync()
        {
            return await _context.Licenses
                .Include(l => l.Organization)
                .Where(l => l.EndDate < DateTime.UtcNow && l.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<License>> GetLicensesExpiringInDaysAsync(int days)
        {
            var targetDate = DateTime.UtcNow.AddDays(days);
            return await _context.Licenses
                .Include(l => l.Organization)
                .Where(l => l.EndDate <= targetDate && l.EndDate >= DateTime.UtcNow && l.DeletedAt == null)
                .ToListAsync();
        }
    }
}
