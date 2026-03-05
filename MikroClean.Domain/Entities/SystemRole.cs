using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class SystemRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<SystemRolePermission> RolePermissions { get; set; } = new List<SystemRolePermission>();
    }
}
