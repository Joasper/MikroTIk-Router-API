using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.Billing;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class BillingController : BaseApiController
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpPost("clientes")]
        public async Task<IActionResult> CreateCliente([FromBody] CreateClienteDTO request)
        {
            var response = await _billingService.CreateClienteAsync(request);
            return HandleResponse(response);
        }

        [HttpGet("clientes/organization/{organizationId:int}")]
        public async Task<IActionResult> GetClientesByOrganization(int organizationId)
        {
            var response = await _billingService.GetClientesByOrganizationAsync(organizationId);
            return HandleResponse(response);
        }

        [HttpPost("subscriptions")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDTO request)
        {
            var response = await _billingService.CreateSubscriptionAsync(request);
            return HandleResponse(response);
        }

        [HttpGet("subscriptions/cliente/{clienteId:int}/active")]
        public async Task<IActionResult> GetActiveSubscription(int clienteId)
        {
            var response = await _billingService.GetActiveSubscriptionAsync(clienteId);
            return HandleResponse(response);
        }

        [HttpPost("templates")]
        public async Task<IActionResult> ConfigureTemplate([FromBody] CreateBillingTemplateDTO request)
        {
            var response = await _billingService.ConfigureBillingTemplateAsync(request);
            return HandleResponse(response);
        }

        [HttpPost("invoices/generate")]
        public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceDTO request)
        {
            var response = await _billingService.GenerateInvoiceAsync(request);
            return HandleResponse(response);
        }

        [HttpGet("invoices/cliente/{clienteId:int}")]
        public async Task<IActionResult> GetInvoicesByCliente(int clienteId)
        {
            var response = await _billingService.GetInvoicesByClienteAsync(clienteId);
            return HandleResponse(response);
        }

        [HttpGet("invoices/{invoiceId:int}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(int invoiceId)
        {
            var response = await _billingService.GenerateInvoicePdfAsync(invoiceId);
            if (response.Status != ResponseStatus.Success || response.Data == null)
            {
                return HandleResponse(response);
            }

            return File(response.Data, "application/pdf", $"factura-{invoiceId}.pdf");
        }

        [HttpPost("invoices/{invoiceId:int}/cancel")]
        public async Task<IActionResult> CancelInvoice(int invoiceId, [FromBody] string? reason = null)
        {
            var response = await _billingService.CancelInvoiceAsync(invoiceId, reason);
            return HandleResponse(response);
        }

        [HttpPost("payments")]
        public async Task<IActionResult> RegisterPayment([FromBody] RegisterPaymentDTO request)
        {
            var response = await _billingService.RegisterPaymentAsync(request);
            return HandleResponse(response);
        }

        [HttpGet("payments/cliente/{clienteId:int}")]
        public async Task<IActionResult> GetPaymentsByCliente(int clienteId)
        {
            var response = await _billingService.GetPaymentsByClienteAsync(clienteId);
            return HandleResponse(response);
        }

        [HttpGet("payments/{paymentId:int}/receipt")]
        public async Task<IActionResult> GetPaymentReceiptPdf(int paymentId)
        {
            var response = await _billingService.GeneratePaymentReceiptPdfAsync(paymentId);
            if (response.Status != ResponseStatus.Success || response.Data == null)
            {
                return HandleResponse(response);
            }

            return File(response.Data, "application/pdf", $"recibo-{paymentId}.pdf");
        }

        [HttpGet("connections/router/{routerId:int}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ActiveClientConnectionDTO>>), 200)]
        public async Task<IActionResult> GetActiveClientConnections(
            int routerId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            var pagination = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = "PppSecretName",
                SortDescending = false
            };

            var response = await _billingService.GetActiveClientConnectionsAsync(routerId, pagination);
            return HandleResponse(response);
        }
    }
}
