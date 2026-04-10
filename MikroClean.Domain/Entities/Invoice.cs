using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa una factura (comprobante fiscal tipo 31 DGII)
    /// </summary>
    public class Invoice : BaseEntity
    {
        /// <summary>
        /// Cliente facturado
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Número de comprobante fiscal (tipo 31) asignado por DGII
        /// </summary>
        public string NumeroFactura { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de comprobante (siempre 31 para factura de crédito)
        /// </summary>
        public int TipoComprobante { get; set; } = 31;

        /// <summary>
        /// Período facturado (mes/año, ej: "2026-03")
        /// </summary>
        public string Periodo { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de emisión de la factura
        /// </summary>
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de vencimiento de la factura
        /// </summary>
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Monto base (sin impuestos)
        /// </summary>
        public decimal MontoBase { get; set; }

        /// <summary>
        /// Monto total de impuestos (ITBIS, etc.)
        /// </summary>
        public decimal MontoImpuesto { get; set; }

        /// <summary>
        /// Monto total a pagar (base + impuestos)
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Monto pagado hasta el momento
        /// </summary>
        public decimal MontoPagado { get; set; } = 0m;

        /// <summary>
        /// Estado actual de la factura
        /// </summary>
        public InvoiceStatus Estado { get; set; } = InvoiceStatus.Vigente;

        /// <summary>
        /// Indicador de si esta factura cubre un abono/pago adelantado
        /// </summary>
        public bool EsAbono { get; set; } = false;

        /// <summary>
        /// Notas o comentarios de la factura
        /// </summary>
        public string? Notas { get; set; }

        /// <summary>
        /// ID del comprobante fiscal (para auditoría DGII)
        /// </summary>
        public int? FiscalVoucherId { get; set; }

        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
        public FiscalVoucher? FiscalVoucher { get; set; }
        public ICollection<InvoiceDetail> Detalles { get; set; } = new List<InvoiceDetail>();
        public ICollection<PaymentInvoiceMapping> PagosAplicados { get; set; } = new List<PaymentInvoiceMapping>();
    }
}
