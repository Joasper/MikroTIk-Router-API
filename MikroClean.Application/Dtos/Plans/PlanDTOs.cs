namespace MikroClean.Application.Dtos.Plans
{
    /// <summary>
    /// DTO para respuesta de plan
    /// </summary>
    public class PlanDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int VelocidadMbps { get; set; }
        public decimal PrecioMensual { get; set; }
        public bool EsDefault { get; set; }
        public bool IsActive { get; set; }
        public int RouterId { get; set; }
        public string? RouterName { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo plan
    /// </summary>
    public class CreatePlanDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int VelocidadMbps { get; set; }
        public decimal PrecioMensual { get; set; }
        public bool EsDefault { get; set; }
        public int RouterId { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un plan existente
    /// </summary>
    public class UpdatePlanDTO
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? VelocidadMbps { get; set; }
        public decimal? PrecioMensual { get; set; }
        public bool? EsDefault { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de estadísticas de plan
    /// </summary>
    public class PlanStatisticsDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int VelocidadMbps { get; set; }
        public decimal PrecioMensual { get; set; }
        public int TotalSuscripciones { get; set; }
        public int SuscripcionesActivas { get; set; }
        public int SuscripcionesSuspendidas { get; set; }
        public decimal IngresoMensualEstimado { get; set; }
    }
}
