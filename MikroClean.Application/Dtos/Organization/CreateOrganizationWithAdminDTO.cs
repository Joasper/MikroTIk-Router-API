using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.Organization
{
    public class CreateOrganizationWithAdminDTO
    {
        [Required(ErrorMessage = "El nombre de la organización es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email de la organización es requerido")]
        [EmailAddress(ErrorMessage = "Email de organización inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string OrganizationEmail { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string OrganizationPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario del administrador es requerido")]
        [StringLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
        public string AdminUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email del administrador es requerido")]
        [EmailAddress(ErrorMessage = "Email del administrador inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string AdminEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña del administrador es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string AdminPassword { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "La clave de licencia no puede exceder 50 caracteres")]
        public string? LicenseKey { get; set; }
    }
}
