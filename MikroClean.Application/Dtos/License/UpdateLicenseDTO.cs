using MikroClean.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.License
{
    public class UpdateLicenseDTO
    {
        [Required(ErrorMessage = "El tipo de licencia es requerido")]
        public TypeLicense Type { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número máximo de routers debe ser mayor a 0")]
        public int? MaxRouters { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número máximo de usuarios debe ser mayor a 0")]
        public int? MaxUsers { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
