namespace MikroClean.Domain.Entities
{
    public class RouterStatus
    {
        public int RouterId { get; set; }
        public bool IsOnline { get; set; } = false;
        public int? CpuUsage { get; set; }
        public int? MemoryUsage { get; set; }
        public long? Uptime { get; set; }
        public int? ActiveUsers { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Router Router { get; set; } = null!;
    }
}
