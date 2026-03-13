using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    public interface ILicenseRepository : IRepository<License>
    {
        Task<License?> GetByOrganizationIdAsync(int organizationId);
        Task<License?> GetByKeyAsync(string key);
        Task<IEnumerable<License>> GetExpiredLicensesAsync();
        Task<IEnumerable<License>> GetLicensesExpiringInDaysAsync(int days);
    }
}
