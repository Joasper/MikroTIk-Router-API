using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class Router : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? Model { get; set; }
        public string? Version { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? MacAddress { get; set; }
        public string? Location { get; set; }

        // Foreign Key
        public int OrganizationId { get; set; }

        // Navigation properties
        public Organizations Organization { get; set; } = null!;
        public ICollection<UserRouterAccess> UserAccesses { get; set; } = new List<UserRouterAccess>();
        public RouterStatus? RouterStatus { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // MikroTik Entities
        public ICollection<IpPool> IpPools { get; set; } = new List<IpPool>();
        public ICollection<PppProfile> PppProfiles { get; set; } = new List<PppProfile>();
        public ICollection<PppSecret> PppSecrets { get; set; } = new List<PppSecret>();
        public ICollection<PppServer> PppServers { get; set; } = new List<PppServer>();
        public ICollection<PendingChange> PendingChanges { get; set; } = new List<PendingChange>();

        // Billing Entities
        public ICollection<Plan> Planes { get; set; } = new List<Plan>();
    }
}
