using MikroClean.Application.Dtos.License;

namespace MikroClean.Application.Dtos.Organization
{
    public class OrganizationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public LicenseDTO? License { get; set; }
    }
}
