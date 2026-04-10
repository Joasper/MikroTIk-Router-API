namespace MikroClean.Domain.Enums
{
    /// <summary>
    /// Tipos de pago
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// Pago completo de una o más facturas
        /// </summary>
        Pago = 0,

        /// <summary>
        /// Abono parcial a una factura
        /// </summary>
        Abono = 1
    }
}
