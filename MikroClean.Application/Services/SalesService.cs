using MikroClean.Application.Dtos.Sales;
using MikroClean.Application.Dtos.Billing;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;
using Microsoft.Extensions.Logging;

namespace MikroClean.Application.Services
{
    public interface ISalesService
    {
        Task<ApiResponse<ActivateServiceResponse>> ActivateServiceAsync(ActivateServiceRequest request);
        Task<ApiResponse<ChangePlanResponse>> ChangePlanAsync(int subscriptionId, ChangePlanRequest request);
        Task<ApiResponse<bool>> SuspendServiceAsync(int subscriptionId, SuspendServiceRequest request);
        Task<ApiResponse<bool>> ReactivateServiceAsync(int subscriptionId, ReactivateServiceRequest request);
    }

    public class SalesService : ISalesService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IPlanRepository _planRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IBillingTemplateRepository _billingTemplateRepository;
        private readonly IBillingService _billingService;
        private readonly IMikroTikService _mikroTikService;
        private readonly IPppSecretRepository _pppSecretRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalesService> _logger;

        public SalesService(
            IClienteRepository clienteRepository,
            IPlanRepository planRepository,
            ISubscriptionRepository subscriptionRepository,
            IBillingTemplateRepository billingTemplateRepository,
            IBillingService billingService,
            IMikroTikService mikroTikService,
            IPppSecretRepository pppSecretRepository,
            IUnitOfWork unitOfWork,
            ILogger<SalesService> logger)
        {
            _clienteRepository = clienteRepository;
            _planRepository = planRepository;
            _subscriptionRepository = subscriptionRepository;
            _billingTemplateRepository = billingTemplateRepository;
            _billingService = billingService;
            _mikroTikService = mikroTikService;
            _pppSecretRepository = pppSecretRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ActivateServiceResponse>> ActivateServiceAsync(ActivateServiceRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando activación de servicio para cliente: {ClienteNombre}", request.ClienteNombre);

                int clienteId = 0;
                bool clienteExistia = false;

                // 1. Buscar o crear cliente
                if (!string.IsNullOrWhiteSpace(request.ClienteCedula))
                {
                    var existingCliente = await _clienteRepository.GetByCedulaAsync(request.ClienteCedula);
                    if (existingCliente != null)
                    {
                        clienteId = existingCliente.Id;
                        clienteExistia = true;
                        _logger.LogInformation("Cliente encontrado por cédula: {ClienteId}", clienteId);
                    }
                }

                if (!clienteExistia && !string.IsNullOrWhiteSpace(request.ClienteEmail))
                {
                    var existingCliente = await _clienteRepository.GetByEmailAsync(request.ClienteEmail);
                    if (existingCliente != null)
                    {
                        clienteId = existingCliente.Id;
                        clienteExistia = true;
                        _logger.LogInformation("Cliente encontrado por email: {ClienteId}", clienteId);
                    }
                }

                if (!clienteExistia)
                {
                    // Validar datos requeridos para crear cliente
                    if (string.IsNullOrWhiteSpace(request.ClienteNombre) ||
                        string.IsNullOrWhiteSpace(request.ClienteCedula) ||
                        string.IsNullOrWhiteSpace(request.ClienteEmail))
                    {
                        return ApiResponse<ActivateServiceResponse>.ValidationError(
                            "Datos de cliente incompletos. Se requiere nombre, cédula y email para crear un nuevo cliente.",
                            new { ClienteNombre = "Requerido", ClienteCedula = "Requerido", ClienteEmail = "Requerido" }
                        );
                    }

                    // Crear nuevo cliente
                    var createClienteResult = await _billingService.CreateClienteAsync(new CreateClienteDTO
                    {
                        Nombre = request.ClienteNombre!,
                        Cedula = request.ClienteCedula!,
                        Email = request.ClienteEmail!,
                        Telefono = request.ClienteTelefono ?? string.Empty,
                        Direccion = request.ClienteDireccion ?? string.Empty,
                        ReferenciaPago = request.ClienteReferenciaPago,
                        OrganizationId = request.OrganizationId
                    });

                    if (createClienteResult.Status != ResponseStatus.Success || createClienteResult.Data == null)
                    {
                        return ApiResponse<ActivateServiceResponse>.Error(
                            $"Error al crear cliente: {createClienteResult.Message}"
                        );
                    }

                    clienteId = createClienteResult.Data.Id;
                    _logger.LogInformation("Nuevo cliente creado: {ClienteId}", clienteId);
                }

                // 2. Validar que el plan existe
                var plan = await _planRepository.GetByIdAsync(request.PlanId);
                if (plan == null || plan.DeletedAt != null || !plan.IsActive)
                {
                    return ApiResponse<ActivateServiceResponse>.NotFound("Plan no encontrado o inactivo");
                }

                // 3. Crear secreto PPPoE en MikroTik
                string pppSecretId = string.Empty;
                try
                {
                    var createSecretResult = await _mikroTikService.CreatePPPoESecretAsync(request.RouterId, new Domain.MikroTik.Operations.CreatePPPoESecretRequest
                    {
                        Name = request.PppSecretName,
                        Password = request.PppPassword,
                        Service = request.PppService,
                        Profile = request.PppProfile,
                        Disabled = !request.HabilitarSecretInmediatamente,
                        Comment = $"{request.ClienteNombre ?? "Cliente"} - {plan.Nombre}"
                    });

                    if (createSecretResult.Status != ResponseStatus.Success || createSecretResult.Data == null)
                    {
                        return ApiResponse<ActivateServiceResponse>.Error(
                            $"Error al crear secreto PPPoE en MikroTik: {createSecretResult.Message}"
                        );
                    }

                    pppSecretId = createSecretResult.Data.Id;
                    _logger.LogInformation("Secreto PPPoE creado en MikroTik: {SecretId}", pppSecretId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creando secreto PPPoE en MikroTik");
                    return ApiResponse<ActivateServiceResponse>.Error(
                        $"Error al crear secreto PPPoE en el router: {ex.Message}"
                    );
                }

                // 4. Crear suscripción
                var createSubscriptionResult = await _billingService.CreateSubscriptionAsync(new CreateSubscriptionDTO
                {
                    ClienteId = clienteId,
                    PlanId = request.PlanId,
                    Notas = request.NotasSuscripcion
                });

                if (createSubscriptionResult.Status != ResponseStatus.Success || createSubscriptionResult.Data == null)
                {
                    return ApiResponse<ActivateServiceResponse>.Error(
                        $"Error al crear suscripción: {createSubscriptionResult.Message}"
                    );
                }

                int suscripcionId = createSubscriptionResult.Data.Id;
                _logger.LogInformation("Suscripción creada: {SuscripcionId}", suscripcionId);

                // 5. Configurar facturación
                var templateResult = await _billingService.ConfigureBillingTemplateAsync(new CreateBillingTemplateDTO
                {
                    ClienteId = clienteId,
                    DiaInicio = request.DiaInicio,
                    DiaCutoff = request.DiaCutoff,
                    DiasGracia = request.DiasGracia,
                    TipoCiclo = BillingCycleType.Mensual,
                    EsPrePago = request.EsPrePago
                });

                if (templateResult.Status != ResponseStatus.Success)
                {
                    _logger.LogWarning("Error configurando plantilla de facturación: {Message}", templateResult.Message);
                }

                // 6. Generar primera factura (opcional)
                int? facturaId = null;
                string? numeroFactura = null;

                if (request.GenerarPrimeraFactura)
                {
                    var invoiceResult = await _billingService.GenerateInvoiceAsync(new GenerateInvoiceDTO
                    {
                        ClienteId = clienteId,
                        Periodo = request.PeriodoFactura,
                        EsAbono = false
                    });

                    if (invoiceResult.Status == ResponseStatus.Success && invoiceResult.Data != null)
                    {
                        facturaId = invoiceResult.Data.Id;
                        numeroFactura = invoiceResult.Data.NumeroFactura;
                        _logger.LogInformation("Primera factura generada: {FacturaId}", facturaId);
                    }
                    else
                    {
                        _logger.LogWarning("Error generando primera factura: {Message}", invoiceResult.Message);
                    }
                }

                var response = new ActivateServiceResponse
                {
                    ClienteId = clienteId,
                    SuscripcionId = suscripcionId,
                    PppSecretName = request.PppSecretName,
                    FacturaId = facturaId,
                    NumeroFactura = numeroFactura,
                    ClienteExistia = clienteExistia,
                    Message = clienteExistia 
                        ? "Servicio activado exitosamente para cliente existente" 
                        : "Cliente creado y servicio activado exitosamente"
                };

                _logger.LogInformation("Activación de servicio completada: Cliente {ClienteId}, Suscripción {SuscripcionId}", 
                    clienteId, suscripcionId);

                return ApiResponse<ActivateServiceResponse>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en activación de servicio");
                return ApiResponse<ActivateServiceResponse>.Error($"Error al activar el servicio: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ChangePlanResponse>> ChangePlanAsync(int subscriptionId, ChangePlanRequest request)
        {
            try
            {
                // Obtener suscripción actual
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null || subscription.DeletedAt != null)
                {
                    return ApiResponse<ChangePlanResponse>.NotFound("Suscripción no encontrada");
                }

                // Validar que la suscripción esté activa
                if (subscription.Estado != SubscriptionStatus.Activa)
                {
                    return ApiResponse<ChangePlanResponse>.ValidationError(
                        "Solo se puede cambiar el plan de suscripciones activas",
                        new { EstadoActual = subscription.Estado.ToString() }
                    );
                }

                // Validar nuevo plan
                var newPlan = await _planRepository.GetByIdAsync(request.NuevoPlanId);
                if (newPlan == null || newPlan.DeletedAt != null || !newPlan.IsActive)
                {
                    return ApiResponse<ChangePlanResponse>.NotFound("Plan no encontrado o inactivo");
                }

                var oldPlan = await _planRepository.GetByIdAsync(subscription.PlanId);

                // Calcular prorrateo si aplica
                decimal montoProrrateo = 0;
                if (request.AplicarProrrateo && oldPlan != null)
                {
                    // Calcular días restantes en el ciclo actual
                    var daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                    var daysRemaining = daysInMonth - DateTime.Now.Day;
                    var dailyRate = oldPlan.PrecioMensual / daysInMonth;
                    var creditAmount = dailyRate * daysRemaining;

                    var newDailyRate = newPlan.PrecioMensual / daysInMonth;
                    var newChargeAmount = newDailyRate * daysRemaining;

                    montoProrrateo = newChargeAmount - creditAmount;
                }

                // Actualizar suscripción
                subscription.PlanId = request.NuevoPlanId;
                subscription.Notas = $"{request.MotivoCambio ?? "Cambio de plan"} (Prorrateo: {montoProrrateo:C})";

                _subscriptionRepository.UpdateAsync(subscription);
                await _unitOfWork.SaveChangesAsync();

                var fechaEfectiva = request.FechaEfectiva ?? DateTime.UtcNow;

                _logger.LogInformation("Cambio de plan realizado: Suscripción {SubscriptionId}, Plan {OldPlanId} -> {NewPlanId}",
                    subscriptionId, subscription.PlanId, request.NuevoPlanId);

                return ApiResponse<ChangePlanResponse>.Success(new ChangePlanResponse
                {
                    SuscripcionId = subscriptionId,
                    PlanAnteriorId = oldPlan?.Id ?? 0,
                    PlanAnteriorNombre = oldPlan?.Nombre ?? "Desconocido",
                    PlanNuevoId = newPlan.Id,
                    PlanNuevoNombre = newPlan.Nombre,
                    MontoProrrateo = montoProrrateo,
                    FechaEfectiva = fechaEfectiva,
                    Message = $"Plan cambiado exitosamente. {(montoProrrateo != 0 ? $"Prorrateo: {montoProrrateo:C}" : "")}" 
                }, "Plan cambiado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando plan de suscripción {SubscriptionId}", subscriptionId);
                return ApiResponse<ChangePlanResponse>.Error($"Error al cambiar el plan: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SuspendServiceAsync(int subscriptionId, SuspendServiceRequest request)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null || subscription.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Suscripción no encontrada");
                }

                if (subscription.Estado == SubscriptionStatus.Suspendida)
                {
                    return ApiResponse<bool>.Warning("La suscripción ya está suspendida", false);
                }

                // Actualizar estado de suscripción
                subscription.Estado = SubscriptionStatus.Suspendida;
                subscription.Notas = $"{subscription.Notas} | Suspendido: {request.Motivo} ({DateTime.UtcNow:yyyy-MM-dd HH:mm})";

                _subscriptionRepository.UpdateAsync(subscription);

                // Deshabilitar secreto PPPoE en MikroTik si existe
                if (subscription.PppSecretId.HasValue)
                {
                    try
                    {
                        var pppSecret = await _pppSecretRepository.GetByIdAsync(subscription.PppSecretId.Value);
                        if (pppSecret != null && !string.IsNullOrEmpty(pppSecret.MikroTikId))
                        {
                            await _mikroTikService.UpdatePPPoESecretAsync(pppSecret.RouterId, new Domain.MikroTik.Operations.UpdatePPPoESecretRequest
                            {
                                Id = pppSecret.MikroTikId,
                                Disabled = true,
                                Comment = $"Suspendido: {request.Motivo}"
                            });

                            _logger.LogInformation("Secreto PPPoE deshabilitado en MikroTik: {SecretId}", pppSecret.MikroTikId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deshabilitando secreto PPPoE en MikroTik");
                    }
                }

                // Desconectar cliente activo si se solicita
                if (request.DesconectarInmediatamente && subscription.PppSecret != null)
                {
                    try
                    {
                        // Buscar conexión activa y eliminarla
                        var connectionsResult = await _mikroTikService.GetActivePPPoEConnectionsAsync(
                            subscription.PppSecret.RouterId, 
                            new PaginationParams { PageNumber = 1, PageSize = 100 });

                        if (connectionsResult.Data != null)
                        {
                            var activeConnection = connectionsResult.Data.Items
                                .FirstOrDefault(c => c.Name == subscription.PppSecret.Name);

                            if (activeConnection != null)
                            {
                                await _mikroTikService.DeleteActivePPPoEConnectionAsync(
                                    subscription.PppSecret.RouterId,
                                    new Domain.MikroTik.Operations.DeletePPPoEActiveConnectionRequest
                                    {
                                        Id = activeConnection.Id
                                    });

                                _logger.LogInformation("Conexión activa eliminada: {ConnectionId}", activeConnection.Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error desconectando cliente activo");
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Servicio suspendido: Suscripción {SubscriptionId}, Motivo: {Reason}", 
                    subscriptionId, request.Motivo);

                return ApiResponse<bool>.Success(true, "Servicio suspendido exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspendiendo servicio {SubscriptionId}", subscriptionId);
                return ApiResponse<bool>.Error($"Error al suspender el servicio: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ReactivateServiceAsync(int subscriptionId, ReactivateServiceRequest request)
        {
            try
            {
                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId);
                if (subscription == null || subscription.DeletedAt != null)
                {
                    return ApiResponse<bool>.NotFound("Suscripción no encontrada");
                }

                if (subscription.Estado != SubscriptionStatus.Suspendida)
                {
                    return ApiResponse<bool>.ValidationError(
                        "Solo se pueden reactivar suscripciones suspendidas",
                        new { EstadoActual = subscription.Estado.ToString() }
                    );
                }

                // Reactivar suscripción
                subscription.Estado = SubscriptionStatus.Activa;
                subscription.Notas = $"{subscription.Notas} | Reactivado: {request.Motivo} ({DateTime.UtcNow:yyyy-MM-dd HH:mm})";

                _subscriptionRepository.UpdateAsync(subscription);

                // Habilitar secreto PPPoE en MikroTik si existe
                if (subscription.PppSecretId.HasValue && request.HabilitarSecretInmediatamente)
                {
                    try
                    {
                        var pppSecret = await _pppSecretRepository.GetByIdAsync(subscription.PppSecretId.Value);
                        if (pppSecret != null && !string.IsNullOrEmpty(pppSecret.MikroTikId))
                        {
                            await _mikroTikService.UpdatePPPoESecretAsync(pppSecret.RouterId, new Domain.MikroTik.Operations.UpdatePPPoESecretRequest
                            {
                                Id = pppSecret.MikroTikId,
                                Disabled = false,
                                Comment = $"Reactivado: {request.Motivo}"
                            });

                            _logger.LogInformation("Secreto PPPoE habilitado en MikroTik: {SecretId}", pppSecret.MikroTikId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error habilitando secreto PPPoE en MikroTik");
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Servicio reactivado: Suscripción {SubscriptionId}, Motivo: {Reason}", 
                    subscriptionId, request.Motivo);

                return ApiResponse<bool>.Success(true, "Servicio reactivado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivando servicio {SubscriptionId}", subscriptionId);
                return ApiResponse<bool>.Error($"Error al reactivar el servicio: {ex.Message}");
            }
        }
    }
}
