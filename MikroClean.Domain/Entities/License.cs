using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    public class License : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public TypeLicense Type { get; set; } = TypeLicense.Trial;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int? MaxRouters { get; set; }
        public int? MaxUsers { get; set; }

        // Foreign Key
        public int? OrganizationId { get; set; }

        // Navigation property
        public Organizations? Organization { get; set; } = null!;
    }
}
