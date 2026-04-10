using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa la suscripción de un cliente a un plan PPPoE
    /// Vincsula Cliente + Plan + PppSecret (credenciales)
    /// </summary>
    public class Subscription : BaseEntity
    {
        /// <summary>
        /// Cliente suscrito
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Plan contratado
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Credenciales PPPoE vinculadas
        /// </summary>
        public int? PppSecretId { get; set; }

        /// <summary>
        /// Fecha de inicio de la suscripción
        /// </summary>
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de fin de la suscripción (opcional)
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Estado actual de la suscripción
        /// </summary>
        public SubscriptionStatus Estado { get; set; } = SubscriptionStatus.Activa;

        /// <summary>
        /// Notas adicionales
        /// </summary>
        public string? Notas { get; set; }

        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
        public Plan Plan { get; set; } = null!;
        public PppSecret? PppSecret { get; set; }
    }
}
