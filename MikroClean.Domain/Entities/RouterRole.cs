using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    public class RouterRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<UserRouterAccess> UserRouterAccesses { get; set; } = new List<UserRouterAccess>();
        public ICollection<RouterRolePermission> RolePermissions { get; set; } = new List<RouterRolePermission>();
    }
}
