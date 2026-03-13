using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.User
{
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        public int? OrganizationId { get; set; }

        [Required(ErrorMessage = "El rol del sistema es requerido")]
        public int SystemRoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
