using MikroClean.Application.Dtos.Plans;
using MikroClean.Application.Models;

namespace MikroClean.Application.Interfaces
{
    public interface IPlanService
    {
        Task<ApiResponse<PlanDTO>> CreatePlanAsync(CreatePlanDTO request);
        Task<ApiResponse<PlanDTO>> GetPlanByIdAsync(int planId);
        Task<ApiResponse<IEnumerable<PlanDTO>>> GetPlansByRouterIdAsync(int routerId);
        Task<ApiResponse<IEnumerable<PlanDTO>>> GetAllActivePlansAsync();
        Task<ApiResponse<PlanDTO>> UpdatePlanAsync(UpdatePlanDTO request);
        Task<ApiResponse<bool>> DeletePlanAsync(int planId);
        Task<ApiResponse<PlanDTO>> SetDefaultPlanAsync(int planId);
        Task<ApiResponse<IEnumerable<PlanStatisticsDTO>>> GetPlanStatisticsAsync(int routerId);
    }
}
