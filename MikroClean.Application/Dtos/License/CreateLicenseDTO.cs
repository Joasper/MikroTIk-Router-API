using MikroClean.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.License
{
    public class CreateLicenseDTO
    {
        [Required(ErrorMessage = "El tipo de licencia es requerido")]
        public TypeLicense Type { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime EndDate { get; set; }

        public int? MaxRouters { get; set; }

        public int? MaxUsers { get; set; }
    }
}
