using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly MikroCleanContext _context;

        public UserRepository(MikroCleanContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.SystemRole)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Username == username && u.DeletedAt == null);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.SystemRole)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .Include(u => u.SystemRole)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => 
                    (u.Username == usernameOrEmail || u.Email == usernameOrEmail) 
                    && u.DeletedAt == null);
        }

        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.SystemRole)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);
        }

        public async Task<IEnumerable<User>> GetUsersByOrganizationAsync(int organizationId)
        {
            return await _context.Users
                .Include(u => u.SystemRole)
                .Where(u => u.OrganizationId == organizationId && u.DeletedAt == null)
                .ToListAsync();
        }
    }
}
