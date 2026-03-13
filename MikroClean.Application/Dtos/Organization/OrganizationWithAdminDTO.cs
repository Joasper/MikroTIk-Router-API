using MikroClean.Application.Dtos.License;
using MikroClean.Application.Dtos.User;

namespace MikroClean.Application.Dtos.Organization
{
    public class OrganizationWithAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public LicenseDTO? License { get; set; }
        public UserDTO? AdminUser { get; set; }
    }
}
