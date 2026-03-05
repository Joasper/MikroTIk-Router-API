namespace MikroClean.Domain.Entities
{
    public class UserRouterAccess
    {
        public int UserId { get; set; }
        public int RouterId { get; set; }
        public int RoleId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Router Router { get; set; } = null!;
        public RouterRole Role { get; set; } = null!;
    }
}
