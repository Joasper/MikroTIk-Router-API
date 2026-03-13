using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class SystemRoleRepository : BaseRepository<SystemRole>, ISystemRoleRepository
    {
        private readonly MikroCleanContext _context;

        public SystemRoleRepository(MikroCleanContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SystemRole?> GetByNameAsync(string name)
        {
            return await _context.SystemRoles
                .FirstOrDefaultAsync(r => r.Name == name && r.DeletedAt == null);
        }

        public async Task<SystemRole?> GetRoleWithPermissionsAsync(int roleId)
        {
            return await _context.SystemRoles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId && r.DeletedAt == null);
        }
    }
}
