using MikroClean.Application.Models;
using MikroClean.Domain.MikroTik;

namespace MikroClean.Application.Dtos.Router
{
    public class CreateRouterDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Se encriptará antes de guardar
        public string? Model { get; set; }
        public string? Location { get; set; }
        public int OrganizationId { get; set; }
    }

    public class UpdateRouterDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string? Password { get; set; } // Opcional en update
        public string? Model { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class RouterDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Model { get; set; }
        public string? Version { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? MacAddress { get; set; }
        public string? Location { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public RouterConnectionStatus? ConnectionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RouterWithCredentialsDTO : RouterDTO
    {
        // Solo para uso interno/admin - NUNCA exponer en API pública
        public string DecryptedPassword { get; set; } = string.Empty;
    }
}
