using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class IpPool : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Ranges { get; set; } = string.Empty;
        public string? NextPool { get; set; }
        public string? Comment { get; set; }
        
        // Identificador interno de MikroTik (*1, *2, etc.) para sincronizaci�n
        public string MikroTikId { get; set; } = string.Empty;
        public SyncState SyncState { get; set; } = SyncState.Synced;

        public int RouterId { get; set; }
        public Router Router { get; set; } = null!;
    }
}
