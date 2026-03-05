namespace MikroClean.Domain.Entities
{
    public class SystemRolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public SystemRole Role { get; set; } = null!;
        public SystemPermission Permission { get; set; } = null!;
    }
}
