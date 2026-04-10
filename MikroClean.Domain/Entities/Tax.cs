using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities
{
    /// <summary>
    /// Representa un impuesto aplicable a las facturas
    /// (Ej: ITBIS 18% en República Dominicana)
    /// </summary>
    public class Tax : BaseEntity
    {
        /// <summary>
        /// Nombre del impuesto (ej: "ITBIS", "ISC", "IPI")
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del impuesto
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Porcentaje aplicable
        /// </summary>
        public decimal Porcentaje { get; set; }

        /// <summary>
        /// Indicador de impuesto activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Notas sobre el impuesto
        /// </summary>
        public string? Notas { get; set; }
    }
}
