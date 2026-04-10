using MikroClean.Domain.Enums;

namespace MikroClean.Application.Dtos.Billing
{
    public class ClienteDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? ReferenciaPago { get; set; }
        public bool IsActive { get; set; }
        public int OrganizationId { get; set; }
    }

    public class CreateClienteDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? ReferenciaPago { get; set; }
        public int OrganizationId { get; set; }
    }

    public class CreateBillingTemplateDTO
    {
        public int ClienteId { get; set; }
        public int DiaInicio { get; set; }
        public int DiaCutoff { get; set; }
        public int DiasGracia { get; set; }
        public BillingCycleType TipoCiclo { get; set; } = BillingCycleType.Mensual;
        public bool EsPrePago { get; set; }
    }

    public class SubscriptionDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public int? PppSecretId { get; set; }
        public string? PppSecretName { get; set; }
        public string PlanNombre { get; set; } = string.Empty;
        public int VelocidadMbps { get; set; }
        public decimal PrecioMensual { get; set; }
        public SubscriptionStatus Estado { get; set; }
    }

    public class CreateSubscriptionDTO
    {
        public int ClienteId { get; set; }
        public int PlanId { get; set; }
        public int? PppSecretId { get; set; }
        public string? Notas { get; set; }
    }

    public class InvoiceDetailDTO
    {
        public int NumeroLinea { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? PeriodoCubierto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class InvoiceDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public int TipoComprobante { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoBase { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal Total { get; set; }
        public decimal MontoPagado { get; set; }
        public InvoiceStatus Estado { get; set; }
        public bool EsAbono { get; set; }
        public List<InvoiceDetailDTO> Detalles { get; set; } = new();
    }

    public class GenerateInvoiceDTO
    {
        public int ClienteId { get; set; }
        public string? Periodo { get; set; }
        public bool EsAbono { get; set; }
        public decimal? MontoAbono { get; set; }
    }

    public class PaymentAllocationDTO
    {
        public int InvoiceId { get; set; }
        public decimal MontoAplicado { get; set; }
    }

    public class RegisterPaymentDTO
    {
        public int ClienteId { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaPago { get; set; }
        public string? Referencia { get; set; }
        public string? MetodoPago { get; set; }
        public string? Notas { get; set; }
        public bool EsAbono { get; set; }
        public List<PaymentAllocationDTO>? Allocations { get; set; }
    }

    public class PaymentDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public PaymentType Tipo { get; set; }
        public string? Referencia { get; set; }
        public string? MetodoPago { get; set; }
        public List<PaymentAllocationDTO> FacturasAplicadas { get; set; } = new();
    }

    public class ActiveClientConnectionDTO
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string PppSecretName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string CallerId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
        public bool TieneClienteAsociado { get; set; }
        public int? ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? PlanNombre { get; set; }
        public int? PlanVelocidadMbps { get; set; }
        public SubscriptionStatus? EstadoSuscripcion { get; set; }
    }
}
