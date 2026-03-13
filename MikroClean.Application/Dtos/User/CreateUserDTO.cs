using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.User
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Password { get; set; } = string.Empty;

        public int? OrganizationId { get; set; }

        [Required(ErrorMessage = "El rol del sistema es requerido")]
        public int SystemRoleId { get; set; }
    }
}
