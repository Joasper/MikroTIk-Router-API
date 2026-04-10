using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Línea de detalle en una factura
    /// Representa servicios facturados (ej: "Servicio PPPoE 30MB del 28/02 al 27/03")
    /// </summary>
    public class InvoiceDetail : BaseEntity
    {
        /// <summary>
        /// Factura a la que pertenece este detalle
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Descripción del servicio (ej: "Servicio PPPoE 30MB")
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Período cubierto por esta línea (ej: "28/02/2026 - 27/03/2026")
        /// </summary>
        public string? PeriodoCubierto { get; set; }

        /// <summary>
        /// Cantidad (generalmente 1 para servicios, pero puede ser días en algunos casos)
        /// </summary>
        public decimal Cantidad { get; set; } = 1m;

        /// <summary>
        /// Precio unitario
        /// </summary>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Subtotal (Cantidad * PrecioUnitario)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Número de línea en la factura
        /// </summary>
        public int NumeroLinea { get; set; }

        // Navigation properties
        public Invoice Invoice { get; set; } = null!;
    }
}
