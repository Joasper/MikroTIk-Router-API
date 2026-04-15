using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.Sales;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Application.Services;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class SalesController : BaseApiController
    {
        private readonly ISalesService _salesService;

        public SalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        /// <summary>
        /// Activa un nuevo servicio completo (cliente + PPPoE + suscripción + facturación)
        /// </summary>
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateService([FromBody] ActivateServiceRequest request)
        {
            var response = await _salesService.ActivateServiceAsync(request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Cambia el plan de una suscripción activa con opción de prorrateo
        /// </summary>
        [HttpPost("subscriptions/{subscriptionId:int}/change-plan")]
        public async Task<IActionResult> ChangePlan(int subscriptionId, [FromBody] ChangePlanRequest request)
        {
            var response = await _salesService.ChangePlanAsync(subscriptionId, request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Suspende un servicio activo
        /// </summary>
        [HttpPost("subscriptions/{subscriptionId:int}/suspend")]
        public async Task<IActionResult> SuspendService(int subscriptionId, [FromBody] SuspendServiceRequest request)
        {
            var response = await _salesService.SuspendServiceAsync(subscriptionId, request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Reactiva un servicio suspendido
        /// </summary>
        [HttpPost("subscriptions/{subscriptionId:int}/reactivate")]
        public async Task<IActionResult> ReactivateService(int subscriptionId, [FromBody] ReactivateServiceRequest request)
        {
            var response = await _salesService.ReactivateServiceAsync(subscriptionId, request);
            return HandleResponse(response);
        }
    }
}
