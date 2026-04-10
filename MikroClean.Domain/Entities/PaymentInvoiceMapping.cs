namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Relación muchos-a-muchos entre Pagos e Invoices
    /// Permite distribuir un pago/abono entre múltiples facturas
    /// Ej: Un pago de 1500 puede cubrir 1000 de factura A + 500 de factura B
    /// </summary>
    public class PaymentInvoiceMapping
    {
        /// <summary>
        /// Pago aplicado
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// Factura a la que se aplica el pago
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Monto del pago aplicado a esta factura específica
        /// </summary>
        public decimal MontoAplicado { get; set; }

        /// <summary>
        /// Fecha en que se aplicó el pago
        /// </summary>
        public DateTime FechaAplicacion { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Payment Payment { get; set; } = null!;
        public Invoice Invoice { get; set; } = null!;
    }
}
