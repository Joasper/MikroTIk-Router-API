using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Enums;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Plantilla de facturación personalizada por cliente
    /// Define cuándo se genera la factura y cuándo se corta el servicio
    /// </summary>
    public class BillingTemplate : BaseEntity
    {
        /// <summary>
        /// Cliente al que aplica esta plantilla
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Día del mes en que INICIA el ciclo (ej: 28)
        /// </summary>
        public int DiaInicio { get; set; }

        /// <summary>
        /// Día del mes en que se CORTA el servicio si no está pagado (ej: 10 del siguiente mes)
        /// </summary>
        public int DiaCutoff { get; set; }

        /// <summary>
        /// Tipo de ciclo (Mensual, Quincenal)
        /// </summary>
        public BillingCycleType TipoCiclo { get; set; } = BillingCycleType.Mensual;

        /// <summary>
        /// Dias de gracia antes de corte definitivo
        /// </summary>
        public int DiasGracia { get; set; } = 0;

        /// <summary>
        /// Tipo de facturación: true=Prepago, false=Postpago
        /// </summary>
        public bool EsPrePago { get; set; } = false;

        /// <summary>
        /// Indicador de plantilla activa
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Cliente Cliente { get; set; } = null!;
    }
}
