namespace MikroClean.Domain.Enums
{
    /// <summary>
    /// Estados de una suscripción PPPoE
    /// </summary>
    public enum SubscriptionStatus
    {
        /// <summary>
        /// Suscripción activa
        /// </summary>
        Activa = 0,

        /// <summary>
        /// Suscripción suspendida (servicio cortado por deuda)
        /// </summary>
        Suspendida = 1,

        /// <summary>
        /// Suscripción cancelada
        /// </summary>
        Cancelada = 2,

        /// <summary>
        /// Suscripción en prueba
        /// </summary>
        Prueba = 3
    }
}
