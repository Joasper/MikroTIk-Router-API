using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public NotificationModule Module { get; set; }
        public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
        public string Message { get; set; } = string.Empty;
        public bool IsViewed { get; set; } = false;
        public string? ActionUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Foreign Keys
        public int UserId { get; set; }
        public int? RouterId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Router? Router { get; set; }
    }
}
