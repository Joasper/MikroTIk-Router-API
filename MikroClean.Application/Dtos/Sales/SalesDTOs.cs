using MikroClean.Application.Dtos.Billing;

namespace MikroClean.Application.Dtos.Sales
{
    /// <summary>
    /// DTO para solicitar activación completa de un nuevo servicio
    /// </summary>
    public class ActivateServiceRequest
    {
        // Datos del cliente
        public string? ClienteNombre { get; set; }
        public string? ClienteCedula { get; set; }
        public string? ClienteEmail { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteDireccion { get; set; }
        public string? ClienteReferenciaPago { get; set; }
        public int OrganizationId { get; set; }

        // Datos del servicio
        public int PlanId { get; set; }
        public int RouterId { get; set; }
        
        // Credenciales PPPoE
        public string PppSecretName { get; set; } = string.Empty;
        public string PppPassword { get; set; } = string.Empty;
        public string PppProfile { get; set; } = string.Empty;
        public string PppService { get; set; } = "pppoe";

        // Configuración de facturación
        public int DiaInicio { get; set; } = 1;
        public int DiaCutoff { get; set; } = 25;
        public int DiasGracia { get; set; } = 5;
        public bool EsPrePago { get; set; } = true;

        // Opciones adicionales
        public bool GenerarPrimeraFactura { get; set; } = true;
        public string? PeriodoFactura { get; set; }
        public string? NotasSuscripcion { get; set; }
        public bool HabilitarSecretInmediatamente { get; set; } = true;
    }

    /// <summary>
    /// DTO para respuesta de activación de servicio
    /// </summary>
    public class ActivateServiceResponse
    {
        public int ClienteId { get; set; }
        public int SuscripcionId { get; set; }
        public int? PppSecretId { get; set; }
        public string PppSecretName { get; set; } = string.Empty;
        public int? FacturaId { get; set; }
        public string? NumeroFactura { get; set; }
        public bool ClienteExistia { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitar cambio de plan
    /// </summary>
    public class ChangePlanRequest
    {
        public int NuevoPlanId { get; set; }
        public bool AplicarProrrateo { get; set; } = true;
        public string? MotivoCambio { get; set; }
        public DateTime? FechaEfectiva { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de cambio de plan
    /// </summary>
    public class ChangePlanResponse
    {
        public int SuscripcionId { get; set; }
        public int PlanAnteriorId { get; set; }
        public string PlanAnteriorNombre { get; set; } = string.Empty;
        public int PlanNuevoId { get; set; }
        public string PlanNuevoNombre { get; set; } = string.Empty;
        public decimal MontoProrrateo { get; set; }
        public DateTime FechaEfectiva { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitud de suspensión de servicio
    /// </summary>
    public class SuspendServiceRequest
    {
        public string Motivo { get; set; } = string.Empty;
        public bool DesconectarInmediatamente { get; set; } = false;
    }

    /// <summary>
    /// DTO para solicitud de reactivación de servicio
    /// </summary>
    public class ReactivateServiceRequest
    {
        public string Motivo { get; set; } = string.Empty;
        public bool HabilitarSecretInmediatamente { get; set; } = true;
    }
}
