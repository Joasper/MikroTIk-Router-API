using MikroClean.Domain.Enums;

namespace MikroClean.Application.Dtos.License
{
    public class LicenseDTO
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public TypeLicense Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? MaxRouters { get; set; }
        public int? MaxUsers { get; set; }
        public int? OrganizationId { get; set; }
        public bool IsExpired => EndDate < DateTime.UtcNow;
        public int DaysRemaining => (EndDate - DateTime.UtcNow).Days;
    }
}
