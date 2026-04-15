using Microsoft.AspNetCore.Mvc;
using MikroClean.Application.Dtos.Plans;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.WebAPI.Controllers.Base;

namespace MikroClean.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class PlanController : BaseApiController
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        /// <summary>
        /// Crea un nuevo plan de servicio PPPoE
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDTO request)
        {
            var response = await _planService.CreatePlanAsync(request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene un plan por su ID
        /// </summary>
        [HttpGet("{planId:int}")]
        public async Task<IActionResult> GetPlan(int planId)
        {
            var response = await _planService.GetPlanByIdAsync(planId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene todos los planes de un router
        /// </summary>
        [HttpGet("router/{routerId:int}")]
        public async Task<IActionResult> GetPlansByRouter(int routerId)
        {
            var response = await _planService.GetPlansByRouterIdAsync(routerId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene todos los planes activos del sistema
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActivePlans()
        {
            var response = await _planService.GetAllActivePlansAsync();
            return HandleResponse(response);
        }

        /// <summary>
        /// Actualiza un plan existente
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdatePlan([FromBody] UpdatePlanDTO request)
        {
            var response = await _planService.UpdatePlanAsync(request);
            return HandleResponse(response);
        }

        /// <summary>
        /// Elimina (desactiva) un plan
        /// </summary>
        [HttpDelete("{planId:int}")]
        public async Task<IActionResult> DeletePlan(int planId)
        {
            var response = await _planService.DeletePlanAsync(planId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Establece un plan como default para su router
        /// </summary>
        [HttpPost("{planId:int}/set-default")]
        public async Task<IActionResult> SetDefaultPlan(int planId)
        {
            var response = await _planService.SetDefaultPlanAsync(planId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Obtiene estadísticas de los planes de un router
        /// </summary>
        [HttpGet("router/{routerId:int}/statistics")]
        public async Task<IActionResult> GetPlanStatistics(int routerId)
        {
            var response = await _planService.GetPlanStatisticsAsync(routerId);
            return HandleResponse(response);
        }
    }
}
