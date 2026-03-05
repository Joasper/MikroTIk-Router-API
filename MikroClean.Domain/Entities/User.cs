using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }

        // Foreign Keys
        public int? OrganizationId { get; set; }
        public int SystemRoleId { get; set; }

        // Navigation properties
        public Organizations? Organization { get; set; }
        public SystemRole SystemRole { get; set; } = null!;
        public ICollection<UserRouterAccess> RouterAccesses { get; set; } = new List<UserRouterAccess>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
