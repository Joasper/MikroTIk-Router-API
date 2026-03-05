using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        public int? RouterId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Router? Router { get; set; }
    }
}
