using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa un plan PPPoE con velocidad y precio
    /// </summary>
    public class Plan : BaseEntity
    {
        /// <summary>
        /// Nombre del plan (ej: "Básico 10MB")
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del plan
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Velocidad en Mbps
        /// </summary>
        public int VelocidadMbps { get; set; }

        /// <summary>
        /// Precio mensual en unidad de moneda local (RD$)
        /// </summary>
        public decimal PrecioMensual { get; set; }

        /// <summary>
        /// Indica si es el plan por defecto para el router
        /// </summary>
        public bool EsDefault { get; set; } = false;

        /// <summary>
        /// Indicador de plan activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Router al que pertenece este plan
        /// </summary>
        public int RouterId { get; set; }

        // Navigation properties
        public Router Router { get; set; } = null!;
        public ICollection<Subscription> Subscripciones { get; set; } = new List<Subscription>();
    }
}
