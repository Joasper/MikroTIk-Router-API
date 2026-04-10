using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa un comprobante fiscal registrado ante DGII (RD)
    /// Mantiene la secuencia de números para facturas tipo 31
    /// </summary>
    public class FiscalVoucher : BaseEntity
    {
        /// <summary>
        /// Factura asociada a este comprobante fiscal
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Número de secuencia asignado
        /// </summary>
        public string NumeroSecuencia { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en que se registró el comprobante
        /// </summary>
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Rango inicial de secuencia autorizado (para auditoría)
        /// </summary>
        public string? RangoDesde { get; set; }

        /// <summary>
        /// Rango final de secuencia autorizado (para auditoría)
        /// </summary>
        public string? RangoHasta { get; set; }

        /// <summary>
        /// Período fiscal al que pertenece (ej: "2026-03")
        /// </summary>
        public string Periodo { get; set; } = string.Empty;

        /// <summary>
        /// Notas sobre el comprobante
        /// </summary>
        public string? Notas { get; set; }

        // Navigation properties
        public Invoice Invoice { get; set; } = null!;
    }
}
