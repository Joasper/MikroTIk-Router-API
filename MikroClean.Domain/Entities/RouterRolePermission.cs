namespace MikroClean.Domain.Entities
{
    public class RouterRolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public RouterRole Role { get; set; } = null!;
        public RouterPermission Permission { get; set; } = null!;
    }
}
