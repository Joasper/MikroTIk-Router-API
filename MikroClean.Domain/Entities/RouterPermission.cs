using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class RouterPermission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<RouterRolePermission> RolePermissions { get; set; } = new List<RouterRolePermission>();
    }
}
