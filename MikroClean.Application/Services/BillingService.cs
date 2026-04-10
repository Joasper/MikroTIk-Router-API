using MikroClean.Application.Dtos.Billing;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MikroClean.Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IBillingTemplateRepository _billingTemplateRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ITaxRepository _taxRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IPppSecretRepository _pppSecretRepository;
        private readonly IMikroTikService _mikroTikService;
        private readonly IUnitOfWork _unitOfWork;

        public BillingService(
            IClienteRepository clienteRepository,
            ISubscriptionRepository subscriptionRepository,
            IBillingTemplateRepository billingTemplateRepository,
            IInvoiceRepository invoiceRepository,
            IPaymentRepository paymentRepository,
            ITaxRepository taxRepository,
            IPlanRepository planRepository,
            IPppSecretRepository pppSecretRepository,
            IMikroTikService mikroTikService,
            IUnitOfWork unitOfWork)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _clienteRepository = clienteRepository;
            _subscriptionRepository = subscriptionRepository;
            _billingTemplateRepository = billingTemplateRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
            _taxRepository = taxRepository;
            _planRepository = planRepository;
            _pppSecretRepository = pppSecretRepository;
            _mikroTikService = mikroTikService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ClienteDTO>> CreateClienteAsync(CreateClienteDTO request)
        {
            var existingCedula = await _clienteRepository.GetByCedulaAsync(request.Cedula);
            if (existingCedula != null)
            {
                return ApiResponse<ClienteDTO>.ValidationError("Ya existe un cliente con esa cédula", new { Cedula = request.Cedula });
            }

            var existingEmail = await _clienteRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return ApiResponse<ClienteDTO>.ValidationError("Ya existe un cliente con ese email", new { Email = request.Email });
            }

            var entity = new Cliente
            {
                Nombre = request.Nombre,
                Cedula = request.Cedula,
                Email = request.Email,
                Telefono = request.Telefono,
                Direccion = request.Direccion,
                ReferenciaPago = request.ReferenciaPago,
                OrganizationId = request.OrganizationId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _clienteRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ClienteDTO>.Success(MapCliente(entity), "Cliente creado exitosamente");
        }

        public async Task<ApiResponse<IEnumerable<ClienteDTO>>> GetClientesByOrganizationAsync(int organizationId)
        {
            var list = await _clienteRepository.GetByOrganizationIdAsync(organizationId);
            return ApiResponse<IEnumerable<ClienteDTO>>.Success(list.Select(MapCliente).ToList(), "Clientes obtenidos");
        }

        public async Task<ApiResponse<SubscriptionDTO>> CreateSubscriptionAsync(CreateSubscriptionDTO request)
        {
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
            if (cliente == null || cliente.DeletedAt != null)
            {
                return ApiResponse<SubscriptionDTO>.NotFound("Cliente no encontrado");
            }

            var plan = await _planRepository.GetByIdAsync(request.PlanId);
            if (plan == null || plan.DeletedAt != null)
            {
                return ApiResponse<SubscriptionDTO>.NotFound("Plan no encontrado");
            }

            var existing = await _subscriptionRepository.GetActiveByClienteIdAsync(request.ClienteId);
            if (existing != null)
            {
                existing.Estado = SubscriptionStatus.Cancelada;
                existing.FechaFin = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
                _subscriptionRepository.UpdateAsync(existing);
            }

            PppSecret? pppSecret = null;
            if (request.PppSecretId.HasValue)
            {
                pppSecret = await _pppSecretRepository.GetByIdAsync(request.PppSecretId.Value);
            }

            var entity = new Subscription
            {
                ClienteId = request.ClienteId,
                PlanId = request.PlanId,
                PppSecretId = request.PppSecretId,
                Estado = SubscriptionStatus.Activa,
                FechaInicio = DateTime.UtcNow,
                Notas = request.Notas,
                CreatedAt = DateTime.UtcNow
            };

            _subscriptionRepository.Add(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SubscriptionDTO>.Success(new SubscriptionDTO
            {
                Id = entity.Id,
                ClienteId = entity.ClienteId,
                PlanId = entity.PlanId,
                PppSecretId = entity.PppSecretId,
                PppSecretName = pppSecret?.Name,
                PlanNombre = plan.Nombre,
                VelocidadMbps = plan.VelocidadMbps,
                PrecioMensual = plan.PrecioMensual,
                Estado = entity.Estado
            }, "Suscripción creada exitosamente");
        }

        public async Task<ApiResponse<SubscriptionDTO>> GetActiveSubscriptionAsync(int clienteId)
        {
            var sub = await _subscriptionRepository.GetActiveByClienteIdAsync(clienteId);
            if (sub == null)
            {
                return ApiResponse<SubscriptionDTO>.NotFound("No hay suscripción activa");
            }

            return ApiResponse<SubscriptionDTO>.Success(MapSubscription(sub), "Suscripción activa");
        }

        public async Task<ApiResponse<bool>> ConfigureBillingTemplateAsync(CreateBillingTemplateDTO request)
        {
            if (request.DiaInicio < 1 || request.DiaInicio > 31 || request.DiaCutoff < 1 || request.DiaCutoff > 31)
            {
                return ApiResponse<bool>.ValidationError("Días inválidos en plantilla", new { request.DiaInicio, request.DiaCutoff });
            }

            if (request.DiasGracia < 0 || request.DiasGracia > 30)
            {
                return ApiResponse<bool>.ValidationError("Días de gracia inválidos", new { request.DiasGracia });
            }

            var existing = await _billingTemplateRepository.GetActiveByClienteIdAsync(request.ClienteId);
            if (existing != null)
            {
                existing.DiaInicio = request.DiaInicio;
                existing.DiaCutoff = request.DiaCutoff;
                existing.DiasGracia = request.DiasGracia;
                existing.TipoCiclo = request.TipoCiclo;
                existing.EsPrePago = request.EsPrePago;
                existing.UpdatedAt = DateTime.UtcNow;
                _billingTemplateRepository.UpdateAsync(existing);
            }
            else
            {
                var entity = new BillingTemplate
                {
                    ClienteId = request.ClienteId,
                    DiaInicio = request.DiaInicio,
                    DiaCutoff = request.DiaCutoff,
                    DiasGracia = request.DiasGracia,
                    TipoCiclo = request.TipoCiclo,
                    EsPrePago = request.EsPrePago,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _billingTemplateRepository.Add(entity);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Plantilla de facturación configurada");
        }

        public async Task<ApiResponse<InvoiceDTO>> GenerateInvoiceAsync(GenerateInvoiceDTO request)
        {
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
            if (cliente == null || cliente.DeletedAt != null)
            {
                return ApiResponse<InvoiceDTO>.NotFound("Cliente no encontrado");
            }

            var sub = await _subscriptionRepository.GetActiveByClienteIdAsync(request.ClienteId);
            if (sub == null)
            {
                return ApiResponse<InvoiceDTO>.ValidationError("Cliente no tiene suscripción activa", new { request.ClienteId });
            }

            var template = await _billingTemplateRepository.GetActiveByClienteIdAsync(request.ClienteId);
            if (template == null)
            {
                return ApiResponse<InvoiceDTO>.ValidationError("Cliente no tiene plantilla de facturación", new { request.ClienteId });
            }

            var period = string.IsNullOrWhiteSpace(request.Periodo)
                ? DateTime.UtcNow.ToString("yyyy-MM")
                : request.Periodo;

            var lineBase = request.EsAbono && request.MontoAbono.HasValue
                ? request.MontoAbono.Value
                : sub.Plan.PrecioMensual;

            if (lineBase <= 0)
            {
                return ApiResponse<InvoiceDTO>.ValidationError("Monto de factura inválido", new { request.MontoAbono, request.EsAbono });
            }

            var activeTaxes = (await _taxRepository.GetActiveAsync()).ToList();
            var taxRate = activeTaxes.Sum(x => x.Porcentaje) / 100m;
            var taxAmount = decimal.Round(lineBase * taxRate, 2, MidpointRounding.AwayFromZero);
            var total = lineBase + taxAmount;

            var invoiceDate = DateTime.UtcNow;
            var dueDate = ComputeDueDate(invoiceDate, template.DiaCutoff, template.DiasGracia);

            var invoiceNumber = await GenerateInvoiceNumberAsync(period);

            var invoice = new Invoice
            {
                ClienteId = request.ClienteId,
                NumeroFactura = invoiceNumber,
                TipoComprobante = 31,
                Periodo = period,
                FechaEmision = invoiceDate,
                FechaVencimiento = dueDate,
                MontoBase = lineBase,
                MontoImpuesto = taxAmount,
                Total = total,
                MontoPagado = 0,
                Estado = InvoiceStatus.Vigente,
                EsAbono = request.EsAbono,
                Notas = request.EsAbono ? "Factura por abono" : "Factura de ciclo regular",
                CreatedAt = DateTime.UtcNow
            };

            _invoiceRepository.Add(invoice);
            await _unitOfWork.SaveChangesAsync();

            var detail = new InvoiceDetail
            {
                InvoiceId = invoice.Id,
                NumeroLinea = 1,
                Descripcion = request.EsAbono ? $"Abono de servicio PPPoE {sub.Plan.Nombre}" : $"Servicio PPPoE {sub.Plan.Nombre}",
                PeriodoCubierto = BuildCoveredPeriod(template, period),
                Cantidad = 1,
                PrecioUnitario = lineBase,
                Subtotal = lineBase,
                CreatedAt = DateTime.UtcNow
            };

            invoice.Detalles.Add(detail);
            _invoiceRepository.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<InvoiceDTO>.Success(MapInvoice(invoice), "Factura generada exitosamente");
        }

        public async Task<ApiResponse<IEnumerable<InvoiceDTO>>> GetInvoicesByClienteAsync(int clienteId)
        {
            var invoices = await _invoiceRepository.GetByClienteIdAsync(clienteId);
            return ApiResponse<IEnumerable<InvoiceDTO>>.Success(invoices.Select(MapInvoice).ToList(), "Facturas obtenidas");
        }

        public async Task<ApiResponse<bool>> CancelInvoiceAsync(int invoiceId, string? reason)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null || invoice.DeletedAt != null)
            {
                return ApiResponse<bool>.NotFound("Factura no encontrada");
            }

            if (invoice.Estado == InvoiceStatus.Pagada)
            {
                return ApiResponse<bool>.ValidationError("No se puede anular una factura pagada", new { invoiceId });
            }

            invoice.Estado = InvoiceStatus.Anulada;
            invoice.Notas = string.IsNullOrWhiteSpace(reason)
                ? "Factura anulada"
                : $"Factura anulada: {reason}";
            invoice.UpdatedAt = DateTime.UtcNow;

            _invoiceRepository.UpdateAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Success(true, "Factura anulada");
        }

        public async Task<ApiResponse<byte[]>> GenerateInvoicePdfAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
            if (invoice == null)
            {
                return ApiResponse<byte[]>.NotFound("Factura no encontrada");
            }

            var cliente = await _clienteRepository.GetByIdAsync(invoice.ClienteId);
            if (cliente == null)
            {
                return ApiResponse<byte[]>.NotFound("Cliente de la factura no encontrado");
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"FACTURA {invoice.NumeroFactura}").FontSize(18).Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text($"Cliente: {cliente.Nombre} - {cliente.Cedula}");
                        col.Item().Text($"Periodo: {invoice.Periodo}");
                        col.Item().Text($"Emisión: {invoice.FechaEmision:dd/MM/yyyy}  Vence: {invoice.FechaVencimiento:dd/MM/yyyy}");
                        col.Item().Text($"Comprobante fiscal tipo: {invoice.TipoComprobante}");

                        foreach (var d in invoice.Detalles.OrderBy(x => x.NumeroLinea))
                        {
                            col.Item().Text($"{d.NumeroLinea}. {d.Descripcion} | {d.PeriodoCubierto} | RD$ {d.Subtotal:N2}");
                        }

                        col.Item().LineHorizontal(1);
                        col.Item().Text($"Subtotal: RD$ {invoice.MontoBase:N2}").Bold();
                        col.Item().Text($"Impuestos: RD$ {invoice.MontoImpuesto:N2}").Bold();
                        col.Item().Text($"Total: RD$ {invoice.Total:N2}").FontSize(14).Bold();
                        col.Item().Text($"Pagado: RD$ {invoice.MontoPagado:N2}");
                        col.Item().Text($"Estado: {invoice.Estado}");
                    });

                    page.Footer().AlignCenter().Text($"Generado {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(9);
                });
            }).GeneratePdf();

            return ApiResponse<byte[]>.Success(pdf, "PDF de factura generado");
        }

        public async Task<ApiResponse<PaymentDTO>> RegisterPaymentAsync(RegisterPaymentDTO request)
        {
            if (request.Monto <= 0)
            {
                return ApiResponse<PaymentDTO>.ValidationError("El monto debe ser mayor que cero", new { request.Monto });
            }

            var pendingInvoices = (await _invoiceRepository.GetPendingByClienteIdAsync(request.ClienteId)).ToList();
            if (!pendingInvoices.Any())
            {
                return ApiResponse<PaymentDTO>.ValidationError("No hay facturas pendientes para aplicar el pago", new { request.ClienteId });
            }

            var allocations = new List<PaymentAllocationDTO>();
            var remaining = request.Monto;

            if (request.Allocations != null && request.Allocations.Any())
            {
                foreach (var reqAlloc in request.Allocations.Where(x => x.MontoAplicado > 0))
                {
                    var invoice = pendingInvoices.FirstOrDefault(x => x.Id == reqAlloc.InvoiceId);
                    if (invoice == null)
                    {
                        return ApiResponse<PaymentDTO>.ValidationError($"Factura {reqAlloc.InvoiceId} no encontrada o no pendiente", new { reqAlloc.InvoiceId });
                    }

                    var outstanding = invoice.Total - invoice.MontoPagado;
                    if (reqAlloc.MontoAplicado > outstanding)
                    {
                        return ApiResponse<PaymentDTO>.ValidationError($"Asignación excede saldo pendiente en factura {invoice.NumeroFactura}", new { invoice.Id, reqAlloc.MontoAplicado, outstanding });
                    }

                    allocations.Add(new PaymentAllocationDTO
                    {
                        InvoiceId = invoice.Id,
                        MontoAplicado = reqAlloc.MontoAplicado
                    });
                }

                var totalAllocated = allocations.Sum(x => x.MontoAplicado);
                if (totalAllocated > request.Monto)
                {
                    return ApiResponse<PaymentDTO>.ValidationError("La suma de asignaciones excede el monto pagado", new { request.Monto, totalAllocated });
                }
                remaining = request.Monto - totalAllocated;
            }

            if (remaining > 0)
            {
                foreach (var invoice in pendingInvoices)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var outstanding = invoice.Total - invoice.MontoPagado;
                    if (outstanding <= 0)
                    {
                        continue;
                    }

                    var amount = Math.Min(outstanding, remaining);
                    var existingAlloc = allocations.FirstOrDefault(x => x.InvoiceId == invoice.Id);
                    if (existingAlloc != null)
                    {
                        existingAlloc.MontoAplicado += amount;
                    }
                    else
                    {
                        allocations.Add(new PaymentAllocationDTO
                        {
                            InvoiceId = invoice.Id,
                            MontoAplicado = amount
                        });
                    }
                    remaining -= amount;
                }
            }

            if (!allocations.Any())
            {
                return ApiResponse<PaymentDTO>.ValidationError("No se pudo aplicar el pago a ninguna factura", new { request.ClienteId, request.Monto });
            }

            var payment = new Payment
            {
                ClienteId = request.ClienteId,
                Monto = request.Monto,
                FechaPago = request.FechaPago ?? DateTime.UtcNow,
                Tipo = request.EsAbono ? PaymentType.Abono : PaymentType.Pago,
                Referencia = request.Referencia,
                MetodoPago = request.MetodoPago,
                Notas = request.Notas,
                CreatedAt = DateTime.UtcNow
            };

            _paymentRepository.Add(payment);
            await _unitOfWork.SaveChangesAsync();

            foreach (var alloc in allocations)
            {
                var invoice = pendingInvoices.First(x => x.Id == alloc.InvoiceId);
                invoice.MontoPagado += alloc.MontoAplicado;
                invoice.UpdatedAt = DateTime.UtcNow;

                if (invoice.MontoPagado >= invoice.Total)
                {
                    invoice.Estado = InvoiceStatus.Pagada;
                }
                else
                {
                    invoice.Estado = InvoiceStatus.Abonada;
                }

                _invoiceRepository.UpdateAsync(invoice);

                payment.FacturasAplicadas.Add(new PaymentInvoiceMapping
                {
                    PaymentId = payment.Id,
                    InvoiceId = invoice.Id,
                    MontoAplicado = alloc.MontoAplicado,
                    FechaAplicacion = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();

            var response = new PaymentDTO
            {
                Id = payment.Id,
                ClienteId = payment.ClienteId,
                Monto = payment.Monto,
                FechaPago = payment.FechaPago,
                Tipo = payment.Tipo,
                Referencia = payment.Referencia,
                MetodoPago = payment.MetodoPago,
                FacturasAplicadas = allocations
            };

            return ApiResponse<PaymentDTO>.Success(response, "Pago registrado y aplicado exitosamente");
        }

        public async Task<ApiResponse<IEnumerable<PaymentDTO>>> GetPaymentsByClienteAsync(int clienteId)
        {
            var payments = await _paymentRepository.GetByClienteIdAsync(clienteId);
            var mapped = payments.Select(p => new PaymentDTO
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                Monto = p.Monto,
                FechaPago = p.FechaPago,
                Tipo = p.Tipo,
                Referencia = p.Referencia,
                MetodoPago = p.MetodoPago,
                FacturasAplicadas = p.FacturasAplicadas.Select(x => new PaymentAllocationDTO
                {
                    InvoiceId = x.InvoiceId,
                    MontoAplicado = x.MontoAplicado
                }).ToList()
            }).ToList();

            return ApiResponse<IEnumerable<PaymentDTO>>.Success(mapped, "Pagos obtenidos");
        }

        public async Task<ApiResponse<byte[]>> GeneratePaymentReceiptPdfAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetWithMappingsAsync(paymentId);
            if (payment == null)
            {
                return ApiResponse<byte[]>.NotFound("Pago no encontrado");
            }

            var cliente = await _clienteRepository.GetByIdAsync(payment.ClienteId);
            if (cliente == null)
            {
                return ApiResponse<byte[]>.NotFound("Cliente del pago no encontrado");
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"RECIBO DE PAGO #{payment.Id}").FontSize(18).Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text($"Cliente: {cliente.Nombre} - {cliente.Cedula}");
                        col.Item().Text($"Fecha: {payment.FechaPago:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Referencia: {payment.Referencia ?? "N/A"}");
                        col.Item().Text($"Método: {payment.MetodoPago ?? "Manual"}");
                        col.Item().Text($"Tipo: {payment.Tipo}");
                        col.Item().Text($"Monto: RD$ {payment.Monto:N2}").FontSize(14).Bold();

                        col.Item().Text("Aplicación a facturas:").Bold();
                        foreach (var m in payment.FacturasAplicadas)
                        {
                            col.Item().Text($"Factura #{m.InvoiceId} -> RD$ {m.MontoAplicado:N2}");
                        }
                    });

                    page.Footer().AlignCenter().Text($"Generado {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(9);
                });
            }).GeneratePdf();

            return ApiResponse<byte[]>.Success(pdf, "PDF de recibo generado");
        }

        public async Task<ApiResponse<PagedResult<ActiveClientConnectionDTO>>> GetActiveClientConnectionsAsync(int routerId, PaginationParams paginationParams)
        {
            var activeResult = await _mikroTikService.GetActivePPPoEConnectionsAsync(routerId, new PaginationParams
            {
                PageNumber = 1,
                PageSize = 1000,
                SearchTerm = null,
                SortBy = "name",
                SortDescending = false
            });

            if (activeResult.Status != ResponseStatus.Success || activeResult.Data == null)
            {
                return ApiResponse<PagedResult<ActiveClientConnectionDTO>>.Error(activeResult.Message);
            }

            var localSubs = (await _subscriptionRepository.GetActiveByRouterIdAsync(routerId)).ToList();
            var bySecretName = localSubs
                .Where(x => x.PppSecret != null)
                .ToDictionary(x => x.PppSecret!.Name, x => x, StringComparer.OrdinalIgnoreCase);

            var enriched = activeResult.Data.Items.Select(x =>
            {
                bySecretName.TryGetValue(x.Name, out var sub);
                return new ActiveClientConnectionDTO
                {
                    ConnectionId = x.Id,
                    PppSecretName = x.Name,
                    Service = x.Service,
                    CallerId = x.CallerId,
                    Address = x.Address,
                    Uptime = x.Uptime,
                    TieneClienteAsociado = sub != null,
                    ClienteId = sub?.ClienteId,
                    ClienteNombre = sub?.Cliente?.Nombre,
                    PlanNombre = sub?.Plan?.Nombre,
                    PlanVelocidadMbps = sub?.Plan?.VelocidadMbps,
                    EstadoSuscripcion = sub?.Estado
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var term = paginationParams.SearchTerm.ToLowerInvariant();
                enriched = enriched.Where(x =>
                    x.PppSecretName.ToLowerInvariant().Contains(term)
                    || (x.ClienteNombre?.ToLowerInvariant().Contains(term) ?? false)
                    || (x.PlanNombre?.ToLowerInvariant().Contains(term) ?? false)
                    || x.Address.ToLowerInvariant().Contains(term)
                    || x.CallerId.ToLowerInvariant().Contains(term)
                ).ToList();
            }

            var total = enriched.Count;
            var items = enriched
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            var paged = new PagedResult<ActiveClientConnectionDTO>
            {
                Items = items,
                TotalCount = total,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };

            return ApiResponse<PagedResult<ActiveClientConnectionDTO>>.Success(paged, "Conexiones activas enriquecidas obtenidas");
        }

        private static DateTime ComputeDueDate(DateTime nowUtc, int cutoffDay, int graceDays)
        {
            var year = nowUtc.Year;
            var month = nowUtc.Month;
            var day = Math.Min(cutoffDay, DateTime.DaysInMonth(year, month));
            var dueDate = new DateTime(year, month, day, 23, 59, 59, DateTimeKind.Utc);

            if (dueDate < nowUtc)
            {
                var next = nowUtc.AddMonths(1);
                day = Math.Min(cutoffDay, DateTime.DaysInMonth(next.Year, next.Month));
                dueDate = new DateTime(next.Year, next.Month, day, 23, 59, 59, DateTimeKind.Utc);
            }

            return dueDate.AddDays(graceDays);
        }

        private async Task<string> GenerateInvoiceNumberAsync(string period)
        {
            var compactPeriod = period.Replace("-", string.Empty);
            var last = await _invoiceRepository.GetLastInvoiceNumberByPeriodAsync(period);
            var seq = 1;

            if (!string.IsNullOrWhiteSpace(last) && last.Length >= 6)
            {
                var suffix = last[^6..];
                if (int.TryParse(suffix, out var parsed))
                {
                    seq = parsed + 1;
                }
            }

            return $"B31{compactPeriod}{seq:000000}";
        }

        private static string BuildCoveredPeriod(BillingTemplate template, string period)
        {
            var parts = period.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var year) || !int.TryParse(parts[1], out var month))
            {
                return period;
            }

            var startDay = Math.Min(template.DiaInicio, DateTime.DaysInMonth(year, month));
            var start = new DateTime(year, month, startDay, 0, 0, 0, DateTimeKind.Utc);

            DateTime end;
            if (template.TipoCiclo == BillingCycleType.Quincenal)
            {
                end = start.AddDays(14);
            }
            else
            {
                end = start.AddMonths(1).AddDays(-1);
            }

            return $"{start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
        }

        private static ClienteDTO MapCliente(Cliente x)
        {
            return new ClienteDTO
            {
                Id = x.Id,
                Nombre = x.Nombre,
                Cedula = x.Cedula,
                Email = x.Email,
                Telefono = x.Telefono,
                Direccion = x.Direccion,
                ReferenciaPago = x.ReferenciaPago,
                IsActive = x.IsActive,
                OrganizationId = x.OrganizationId
            };
        }

        private static SubscriptionDTO MapSubscription(Subscription x)
        {
            return new SubscriptionDTO
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                PlanId = x.PlanId,
                PppSecretId = x.PppSecretId,
                PppSecretName = x.PppSecret?.Name,
                PlanNombre = x.Plan.Nombre,
                VelocidadMbps = x.Plan.VelocidadMbps,
                PrecioMensual = x.Plan.PrecioMensual,
                Estado = x.Estado
            };
        }

        private static InvoiceDTO MapInvoice(Invoice x)
        {
            return new InvoiceDTO
            {
                Id = x.Id,
                ClienteId = x.ClienteId,
                NumeroFactura = x.NumeroFactura,
                TipoComprobante = x.TipoComprobante,
                Periodo = x.Periodo,
                FechaEmision = x.FechaEmision,
                FechaVencimiento = x.FechaVencimiento,
                MontoBase = x.MontoBase,
                MontoImpuesto = x.MontoImpuesto,
                Total = x.Total,
                MontoPagado = x.MontoPagado,
                Estado = x.Estado,
                EsAbono = x.EsAbono,
                Detalles = x.Detalles.Select(d => new InvoiceDetailDTO
                {
                    NumeroLinea = d.NumeroLinea,
                    Descripcion = d.Descripcion,
                    PeriodoCubierto = d.PeriodoCubierto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList()
            };
        }
    }
}
