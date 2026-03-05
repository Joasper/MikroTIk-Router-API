using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class SystemPermission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<SystemRolePermission> RolePermissions { get; set; } = new List<SystemRolePermission>();
    }
}
