using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class PppSecret : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Username
        public string? Password { get; set; }
        public string Service { get; set; } = "pppoe";
        public string Profile { get; set; } = string.Empty; // Profile Name or Id, usually Name
        public bool Disabled { get; set; }
        public string? Comment { get; set; }

        public string MikroTikId { get; set; } = string.Empty;
        public SyncState SyncState { get; set; } = SyncState.Synced;

        public int RouterId { get; set; }
        public Router Router { get; set; } = null!;
    }
}
