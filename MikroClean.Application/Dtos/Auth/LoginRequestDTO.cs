using System.ComponentModel.DataAnnotations;

namespace MikroClean.Application.Dtos.Auth
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El nombre de usuario o email es requerido")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
}
