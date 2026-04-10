using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class PppServer : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Service Name
        public string Interface { get; set; } = string.Empty;
        public string DefaultProfile { get; set; } = "default";
        public int? MaxMtu { get; set; } = 1480;
        public int? MaxMru { get; set; } = 1480;
        public int? KeepaliveTimeout { get; set; } = 10;
        public bool OneSessionPerHost { get; set; } = true;
        public bool Disabled { get; set; }
        public string? Comment { get; set; }

        public string MikroTikId { get; set; } = string.Empty;
        public SyncState SyncState { get; set; } = SyncState.Synced;

        public int RouterId { get; set; }
        public Router Router { get; set; } = null!;
    }
}
