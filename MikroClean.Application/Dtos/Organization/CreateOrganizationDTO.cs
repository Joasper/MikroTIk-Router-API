using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.Organization
{
    public class CreateOrganizationDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Phone { get; set; } = string.Empty;
    }
}
