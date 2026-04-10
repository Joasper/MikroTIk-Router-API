namespace MikroClean.Domain.Enums
{
    /// <summary>
    /// Estados posibles de una factura
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>
        /// Factura vigente, pendiente de pago
        /// </summary>
        Vigente = 0,

        /// <summary>
        /// Factura pagada completamente
        /// </summary>
        Pagada = 1,

        /// <summary>
        /// Factura parcialmente pagada (con abonos)
        /// </summary>
        Abonada = 2,

        /// <summary>
        /// Factura anulada o cancelada
        /// </summary>
        Anulada = 3,

        /// <summary>
        /// Factura vencida sin pago
        /// </summary>
        Vencida = 4
    }
}
