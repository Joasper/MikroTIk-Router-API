using MikroClean.Application.Dtos.Billing;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface IBillingService
    {
        Task<ApiResponse<ClienteDTO>> CreateClienteAsync(CreateClienteDTO request);
        Task<ApiResponse<IEnumerable<ClienteDTO>>> GetClientesByOrganizationAsync(int organizationId);

        Task<ApiResponse<SubscriptionDTO>> CreateSubscriptionAsync(CreateSubscriptionDTO request);
        Task<ApiResponse<SubscriptionDTO>> GetActiveSubscriptionAsync(int clienteId);

        Task<ApiResponse<bool>> ConfigureBillingTemplateAsync(CreateBillingTemplateDTO request);

        Task<ApiResponse<InvoiceDTO>> GenerateInvoiceAsync(GenerateInvoiceDTO request);
        Task<ApiResponse<IEnumerable<InvoiceDTO>>> GetInvoicesByClienteAsync(int clienteId);
        Task<ApiResponse<bool>> CancelInvoiceAsync(int invoiceId, string? reason);
        Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(int invoiceId);

        Task<ApiResponse<PaymentDTO>> RegisterPaymentAsync(RegisterPaymentDTO request);
        Task<ApiResponse<IEnumerable<PaymentDTO>>> GetPaymentsByClienteAsync(int clienteId);
        Task<ApiResponse<byte[]>> GeneratePaymentReceiptPdfAsync(int paymentId);

        Task<ApiResponse<PagedResult<ActiveClientConnectionDTO>>> GetActiveClientConnectionsAsync(int routerId, PaginationParams paginationParams);
    }
}
