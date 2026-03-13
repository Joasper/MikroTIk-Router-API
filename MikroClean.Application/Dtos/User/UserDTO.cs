namespace MikroClean.Application.Dtos.User
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public int SystemRoleId { get; set; }
        public string SystemRoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
