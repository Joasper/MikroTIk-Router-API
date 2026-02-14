using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos
{
    public class UpdateDeviceDto
    {
        [Required(ErrorMessage = "Device name is required.")]
        public string Name { get; set; }
        public string Model { get; set; }
    }
}
