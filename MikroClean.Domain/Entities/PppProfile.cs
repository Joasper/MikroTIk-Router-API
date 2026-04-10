using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class PppProfile : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? LocalAddress { get; set; }
        public string? RemoteAddress { get; set; }
        public string? DnsServers { get; set; }
        public string? RateLimit { get; set; }
        public string OnlyOne { get; set; } = "default";
        public bool IsDefault { get; set; } // If we want to mark it as default
        public string? Comment { get; set; }
        
        // Identificador interno de MikroTik (*1, *2, etc.) para sincronizaci�n
        public string MikroTikId { get; set; } = string.Empty;
        public SyncState SyncState { get; set; } = SyncState.Synced;

        public int RouterId { get; set; }
        public Router Router { get; set; } = null!;
    }
}
