using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa un pago o abono realizado por un cliente
    /// Puede ser un pago completo de una o más facturas, o un abono parcial
    /// </summary>
    public class Payment : BaseEntity
    {
        /// <summary>
        /// Cliente que realiza el pago
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Monto pagado/abonado
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha en que se registra el pago
        /// </summary>
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tipo de pago (Pago completo o Abono parcial)
        /// </summary>
        public PaymentType Tipo { get; set; } = PaymentType.Pago;

        /// <summary>
        /// Referencia del pago (ej: número de transferencia, cheque, etc.)
        /// </summary>
        public string? Referencia { get; set; }

        /// <summary>
        /// Método de pago (Transferencia, Efectivo, Cheque, etc.)
        /// </summary>
        public string? MetodoPago { get; set; }

        /// <summary>
        /// Notas adicionales sobre el pago
        /// </summary>
        public string? Notas { get; set; }

        /// <summary>
        /// Usuario que registró el pago
        /// </summary>
        public int? RegistradoPorUserId { get; set; }

        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
        public ICollection<PaymentInvoiceMapping> FacturasAplicadas { get; set; } = new List<PaymentInvoiceMapping>();
    }
}
