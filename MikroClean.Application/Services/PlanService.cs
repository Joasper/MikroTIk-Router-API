using MikroClean.Application.Dtos.Plans;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;
using Microsoft.Extensions.Logging;

namespace MikroClean.Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IRouterRepository _routerRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PlanService> _logger;

        public PlanService(
            IPlanRepository planRepository,
            IRouterRepository routerRepository,
            ISubscriptionRepository subscriptionRepository,
            IUnitOfWork unitOfWork,
            ILogger<PlanService> logger)
        {
            _planRepository = planRepository;
            _routerRepository = routerRepository;
            _subscriptionRepository = subscriptionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<PlanDTO>> CreatePlanAsync(CreatePlanDTO request)
        {
            try
            {
                // Validar que el router existe
                var router = await _routerRepository.GetByIdAsync(request.RouterId);
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<PlanDTO>.NotFound("Router no encontrado");
                }

                // Validar que el nombre no esté duplicado en el mismo router
                var existingPlans = await _planRepository.GetByRouterIdAsync(request.RouterId);
                if (existingPlans.Any(p => p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) && p.IsActive))
                {
                    return ApiResponse<PlanDTO>.ValidationError(
                        "Ya existe un plan activo con ese nombre en el router especificado",
                        new { Nombre = $"El plan '{request.Nombre}' ya existe en este router" }
                    );
                }

                // Si es default, desactivar otros defaults
                if (request.EsDefault)
                {
                    foreach (var defaultPlan in existingPlans.Where(p => p.EsDefault))
                    {
                        defaultPlan.EsDefault = false;
                        _planRepository.UpdateAsync(defaultPlan);
                    }
                }

                // Crear plan
                var plan = new Plan
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    VelocidadMbps = request.VelocidadMbps,
                    PrecioMensual = request.PrecioMensual,
                    EsDefault = request.EsDefault,
                    IsActive = true,
                    RouterId = request.RouterId
                };

                _planRepository.Add(plan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Plan creado exitosamente: {PlanName} (ID: {PlanId})", plan.Nombre, plan.Id);

                return ApiResponse<PlanDTO>.Success(
                    MapToDto(plan, router.Name),
                    "Plan creado exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando plan {PlanName}", request.Nombre);
                return ApiResponse<PlanDTO>.Error($"Error al crear el plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PlanDTO>> GetPlanByIdAsync(int planId)
        {
            try
            {
                var plan = await _planRepository.GetByIdAsync(planId);
                if (plan == null || plan.DeletedAt != null)
                {
                    return ApiResponse<PlanDTO>.NotFound("Plan no encontrado");
                }

                var router = await _routerRepository.GetByIdAsync(plan.RouterId);
                return ApiResponse<PlanDTO>.Success(
                    MapToDto(plan, router?.Name ?? "Desconocido"),
                    "Plan obtenido exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo plan {PlanId}", planId);
                return ApiResponse<PlanDTO>.Error($"Error al obtener el plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<PlanDTO>>> GetPlansByRouterIdAsync(int routerId)
        {
            try
            {
                var router = await _routerRepository.GetByIdAsync(routerId);
                if (router == null || router.DeletedAt != null)
                {
                    return ApiResponse<IEnumerable<PlanDTO>>.NotFound("Router no encontrado");
                }

                var plans = await _planRepository.GetByRouterIdAsync(routerId);
                var planDtos = plans.Select(p => MapToDto(p, router.Name)).ToList();

                return ApiResponse<IEnumerable<PlanDTO>>.Success(
                    planDtos,
                    $"Se encontraron {planDtos.Count} planes"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo planes del router {RouterId}", routerId);
                return ApiResponse<IEnumerable<PlanDTO>>.Error($"Error al obtener planes: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<PlanDTO>>> GetAllActivePlansAsync()
        {
            try
            {
                // Note: Esto requeriría un método adicional en el repositorio
                // Por ahora obtenemos todos los planes y filtramos
                var allPlans = await _planRepository.GetAllAsync();
                var activePlans = allPlans.Where(p => p.IsActive && p.DeletedAt == null).ToList();

                var planDtos = new List<PlanDTO>();
                foreach (var plan in activePlans)
                {
                    var router = await _routerRepository.GetByIdAsync(plan.RouterId);
                    planDtos.Add(MapToDto(plan, router?.Name ?? "Desconocido"));
                }

                return ApiResponse<IEnumerable<PlanDTO>>.Success(
                    planDtos,
                    $"Se encontraron {planDtos.Count} planes activos"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los planes activos");
                return ApiResponse<IEnumerable<PlanDTO>>.Error($"Error al obtener planes: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PlanDTO>> UpdatePlanAsync(UpdatePlanDTO request)
        {
            try
            {
                var plan = await _planRepository.GetByIdAsync(request.Id);
                if (plan == null || plan.DeletedAt != null)
                {
                    return ApiResponse<PlanDTO>.NotFound("Plan no encontrado");
                }

                // Validar nombre único si se está cambiando
                if (!string.IsNullOrWhiteSpace(request.Nombre) && !request.Nombre.Equals(plan.Nombre))
                {
                    var existingPlans = await _planRepository.GetByRouterIdAsync(plan.RouterId);
                    if (existingPlans.Any(p => p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) 
                        && p.Id != plan.Id && p.IsActive))
                    {
                        return ApiResponse<PlanDTO>.ValidationError(
                            "Ya existe un plan activo con ese nombre en el router",
                            new { Nombre = $"El plan '{request.Nombre}' ya existe en este router" }
                        );
                    }
                }

                // Actualizar campos
                if (!string.IsNullOrWhiteSpace(request.Nombre))
                    plan.Nombre = request.Nombre;

                if (request.Descripcion != null)
                    plan.Descripcion = request.Descripcion;

                if (request.VelocidadMbps.HasValue)
                    plan.VelocidadMbps = request.VelocidadMbps.Value;

                if (request.PrecioMensual.HasValue)
                    plan.PrecioMensual = request.PrecioMensual.Value;

                if (request.EsDefault.HasValue && request.EsDefault.Value)
                {
                    // Desactivar otros defaults
                    var existingPlans = await _planRepository.GetByRouterIdAsync(plan.RouterId);
                    foreach (var p in existingPlans.Where(p => p.EsDefault && p.Id != plan.Id))
                    {
                        p.EsDefault = false;
                        _planRepository.UpdateAsync(p);
                    }
                    plan.EsDefault = true;
                }
                else if (request.EsDefault.HasValue)
                {
                    plan.EsDefault = request.EsDefault.Value;
                }

                _planRepository.UpdateAsync(plan);
                await _unitOfWork.SaveChangesAsync();

                var router = await _routerRepository.GetByIdAsync(plan.RouterId);
                _logger.LogInformation("Plan actualizado exitosamente: {PlanName} (ID: {PlanId})", plan.Nombre, plan.Id);

                return ApiResponse<PlanDTO>.Success(
                    MapToDto(plan, router?.Name ?? "Desconocido"),
                    "Plan actualizado exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando plan {PlanId}", request.Id);
                return ApiResponse<PlanDTO>.Error($"Error al actualizar el plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePlanAsync(int planId)
        {
            try
            {
                var plan = await _planRepository.GetByIdAsync(planId);
                if (plan == null || plan.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Plan no encontrado");
                }

                // Validar que no tenga suscripciones activas
                var activeSubscriptions = await _subscriptionRepository.GetActiveByRouterIdAsync(plan.RouterId);
                var planSubscriptions = activeSubscriptions.Where(s => s.PlanId == planId).ToList();

                if (planSubscriptions.Any())
                {
                    return ApiResponse<bool>.ValidationError(
                        $"No se puede eliminar el plan porque tiene {planSubscriptions.Count} suscripción(es) activa(s)",
                        new { SuscripcionesActivas = planSubscriptions.Count }
                    );
                }

                // Soft delete
                plan.IsActive = false;
                plan.DeletedAt = DateTime.UtcNow;

                _planRepository.UpdateAsync(plan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Plan elimininado: {PlanName} (ID: {PlanId})", plan.Nombre, plan.Id);

                return ApiResponse<bool>.Success(true, "Plan elimininado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando plan {PlanId}", planId);
                return ApiResponse<bool>.Error($"Error al eliminar el plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PlanDTO>> SetDefaultPlanAsync(int planId)
        {
            try
            {
                var plan = await _planRepository.GetByIdAsync(planId);
                if (plan == null || plan.DeletedAt != null)
                {
                    return ApiResponse<PlanDTO>.NotFound("Plan no encontrado");
                }

                // Desactivar otros defaults del mismo router
                var existingPlans = await _planRepository.GetByRouterIdAsync(plan.RouterId);
                foreach (var p in existingPlans.Where(p => p.EsDefault && p.Id != planId))
                {
                    p.EsDefault = false;
                    _planRepository.UpdateAsync(p);
                }

                plan.EsDefault = true;
                _planRepository.UpdateAsync(plan);
                await _unitOfWork.SaveChangesAsync();

                var router = await _routerRepository.GetByIdAsync(plan.RouterId);
                _logger.LogInformation("Plan establecido como default: {PlanName} (ID: {PlanId})", plan.Nombre, plan.Id);

                return ApiResponse<PlanDTO>.Success(
                    MapToDto(plan, router?.Name ?? "Desconocido"),
                    "Plan establecido como default exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo plan default {PlanId}", planId);
                return ApiResponse<PlanDTO>.Error($"Error al establecer plan default: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<PlanStatisticsDTO>>> GetPlanStatisticsAsync(int routerId)
        {
            try
            {
                var plans = await _planRepository.GetByRouterIdAsync(routerId);
                var statistics = new List<PlanStatisticsDTO>();

                foreach (var plan in plans)
                {
                    var subscriptions = await _subscriptionRepository.GetByRouterAndPppSecretNameAsync(routerId, "");
                    var planSubscriptions = await _subscriptionRepository.GetActiveByRouterIdAsync(routerId);
                    var filteredSubscriptions = planSubscriptions.Where(s => s.PlanId == plan.Id).ToList();

                    var stat = new PlanStatisticsDTO
                    {
                        Id = plan.Id,
                        Nombre = plan.Nombre,
                        VelocidadMbps = plan.VelocidadMbps,
                        PrecioMensual = plan.PrecioMensual,
                        TotalSuscripciones = filteredSubscriptions.Count,
                        SuscripcionesActivas = filteredSubscriptions.Count(s => s.Estado == Domain.Enums.SubscriptionStatus.Activa),
                        SuscripcionesSuspendidas = filteredSubscriptions.Count(s => s.Estado == Domain.Enums.SubscriptionStatus.Suspendida),
                        IngresoMensualEstimado = filteredSubscriptions.Count(s => s.Estado == Domain.Enums.SubscriptionStatus.Activa) * plan.PrecioMensual
                    };

                    statistics.Add(stat);
                }

                return ApiResponse<IEnumerable<PlanStatisticsDTO>>.Success(
                    statistics,
                    "Estadísticas de planes obtenidas exitosamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de planes del router {RouterId}", routerId);
                return ApiResponse<IEnumerable<PlanStatisticsDTO>>.Error($"Error al obtener estadísticas: {ex.Message}");
            }
        }

        private PlanDTO MapToDto(Plan plan, string routerName)
        {
            return new PlanDTO
            {
                Id = plan.Id,
                Nombre = plan.Nombre,
                Descripcion = plan.Descripcion,
                VelocidadMbps = plan.VelocidadMbps,
                PrecioMensual = plan.PrecioMensual,
                EsDefault = plan.EsDefault,
                IsActive = plan.IsActive,
                RouterId = plan.RouterId,
                RouterName = routerName
            };
        }
    }
}
