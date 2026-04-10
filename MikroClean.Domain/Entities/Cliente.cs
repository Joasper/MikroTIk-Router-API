using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa un cliente PPPoE (usuario final B2C)
    /// Independiente de la tabla User (admin/operadores)
    /// </summary>
    public class Cliente : BaseEntity
    {
        /// <summary>
        /// Nombre completo del cliente
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Cédula o RNC del cliente (para facturación DGII)
        /// </summary>
        public string Cedula { get; set; } = string.Empty;

        /// <summary>
        /// Email de contacto
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Dirección física
        /// </summary>
        public string Direccion { get; set; } = string.Empty;

        /// <summary>
        /// Referencia de pago o notas adicionales
        /// </summary>
        public string? ReferenciaPago { get; set; }

        /// <summary>
        /// Indicador de cliente activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Organización a la que pertenece
        /// </summary>
        public int OrganizationId { get; set; }

        // Navigation properties
        public Organizations Organization { get; set; } = null!;
        public ICollection<Subscription> Subscripciones { get; set; } = new List<Subscription>();
        public ICollection<Invoice> Facturas { get; set; } = new List<Invoice>();
        public ICollection<Payment> Pagos { get; set; } = new List<Payment>();
        public ICollection<BillingTemplate> PlantillasBilling { get; set; } = new List<BillingTemplate>();
    }
}
