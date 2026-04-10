using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class PendingChange : BaseEntity
    {
        public int RouterId { get; set; }
        public Router Router { get; set; } = null!;

        public PendingChangeResource Resource { get; set; }
        public PendingChangeOperation Operation { get; set; }
        public PendingChangeStatus Status { get; set; } = PendingChangeStatus.Pending;

        public string PayloadJson { get; set; } = string.Empty;
        public string? EntityKey { get; set; }

        public int RetryCount { get; set; }
        public DateTime NextRetryAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? LastError { get; set; }
    }
}
