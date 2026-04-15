# Estado del Módulo de Facturación y Parte Comercial PPPoE

## Resumen General

El sistema cuenta con un **módulo de facturación completamente implementado** integrado con la infraestructura PPPoE de MikroTik. El módulo soporta gestión de clientes, planes de suscripción, generación de facturas con cumplimiento fiscal DGII (República Dominicana), procesamiento de pagos, cálculo de impuestos y enriquecimiento de conexiones PPPoE con datos comerciales.

---

## 📁 Estructura del Módulo

### Capa de Aplicación (`MikroClean.Application`)

#### DTOs de Facturación (`Dtos/Billing/BillingDTOs.cs`)

| DTO | Descripción |
|-----|-------------|
| `ClienteDTO` / `CreateClienteDTO` | Datos maestros del cliente (nombre, cédula/RNC, email, teléfono, dirección, referencia de pago, organización) |
| `CreateBillingTemplateDTO` | Configuración de facturación por cliente (día inicio, día corte, días de gracia, ciclo mensual/quincenal, pre-pago/post-pago) |
| `SubscriptionDTO` / `CreateSubscriptionDTO` | Vincula Cliente + Plan + PppSecret opcional, con seguimiento de estado |
| `InvoiceDetailDTO` | Línea de factura (número línea, descripción, período cubierto, cantidad, precio unitario, subtotal) |
| `InvoiceDTO` | Factura completa (número, tipo comprobante fiscal 31, período, fechas emisión/vencimiento, montos base/impuesto/total/pagado, estado, flag anticipo) |
| `GenerateInvoiceDTO` | Solicitud de generación de factura (clienteId, período opcional, flag anticipo, monto anticipo opcional) |
| `PaymentAllocationDTO` | Mapea monto de pago a factura específica |
| `RegisterPaymentDTO` | Registro de pago (clienteId, monto, fecha, referencia, método pago, notas, flag anticipo, asignaciones explícitas opcionales) |
| `PaymentDTO` | Registro de pago con asignaciones a facturas |
| `ActiveClientConnectionDTO` | Conexión PPPoE activa enriquecida con datos de cliente, plan y suscripción |

#### Interfaces de Servicio (`Interfaces/IBillingService.cs`)

| Método | Funcionalidad |
|--------|---------------|
| `CreateClienteAsync` | Crear cliente |
| `GetClientesByOrganizationAsync` | Obtener clientes por organización |
| `CreateSubscriptionAsync` | Crear suscripción |
| `GetActiveSubscriptionAsync` | Obtener suscripción activa |
| `ConfigureBillingTemplateAsync` | Configurar plantilla de facturación |
| `GenerateInvoiceAsync` | Generar factura |
| `GetInvoicesByClienteAsync` | Consultar facturas por cliente |
| `CancelInvoiceAsync` | Cancelar factura (protegida si está pagada) |
| `GenerateInvoicePdfAsync` | Generar PDF de factura |
| `RegisterPaymentAsync` | Registrar pago |
| `GetPaymentsByClienteAsync` | Consultar pagos por cliente |
| `GeneratePaymentReceiptPdfAsync` | Generar recibo de pago en PDF |
| `GetActiveClientConnectionsAsync` | Obtener conexiones PPPoE activas enriquecidas |

#### Servicio Principal (`Services/BillingService.cs` - 766 líneas)

**Dependencias inyectadas:** 11 repositorios/servicios
- `IClienteRepository`, `ISubscriptionRepository`, `IBillingTemplateRepository`
- `IInvoiceRepository`, `IPaymentRepository`, `ITaxRepository`
- `IPlanRepository`, `IPppSecretRepository`
- `IMikroTikService`, `IUnitOfWork`

**Operaciones clave:**

1. **Gestión de Clientes**
   - Creación con validación de cédula/email únicos
   - Consulta por organización

2. **Suscripciones**
   - Creación con desactivación de suscripción previa
   - Vinculación con secreto PPPoE
   - Seguimiento de estado

3. **Plantillas de Facturación**
   - Configuración por cliente (upsert)
   - Ciclos de día inicio/corte, período de gracia, pre/post-pago

4. **Generación de Facturas**
   - Cálculo automático de impuestos
   - Número de factura estilo DGII: `B31{YYYYMM}{secuencia}` (ej: `B31202604000001`)
   - Período de servicio automático según plantilla
   - Creación de factura + líneas de detalle

5. **Procesamiento de Pagos**
   - Asignación explícita o distribución automática en facturas pendientes (más antiguas primero)
   - Actualización de estados: `Vigente → Abonada → Pagada`
   - Soporte para pagos de anticipo

6. **Generación de PDFs**
   - Facturas con QuestPDF
   - Recibos de pago con QuestPDF

7. **Enriquecimiento de Conexiones**
   - Obtiene conexiones PPPoE activas de MikroTik
   - Enriquece con datos locales de suscripción/cliente/plan

**Métodos auxiliares:**
- `ComputeDueDate` - Calcula fecha vencimiento desde día corte + días gracia
- `GenerateInvoiceNumberAsync` - Numeración secuencial por período
- `BuildCoveredPeriod` - Calcula período de servicio (mensual/quincenal)
- Métodos de mapeo: `MapCliente`, `MapSubscription`, `MapInvoice`

---

### Capa de Dominio (`MikroClean.Domain`)

#### Entidades Comerciales

| Entidad | Tabla | Propósito |
|---------|-------|-----------|
| `Cliente` | Clientes | Cliente final (nombre, cédula/RNC, email, teléfono, dirección, referencia pago). Nav: Suscripciones, Facturas, Pagos, PlantillasFacturación |
| `Plan` | Planes | Plan de servicio PPPoE (nombre, descripción, velocidad Mbps, precio mensual, flag defecto). Nav: Router, Suscripciones |
| `Subscription` | Subscripciones | Vincula Cliente + Plan + PppSecret opcional. Fechas inicio/fin, estado, notas |
| `BillingTemplate` | BillingTemplates | Configuración facturación por cliente (día inicio, día corte, tipo ciclo, días gracia, pre/post-pago) |
| `Invoice` | Invoices | Factura fiscal (tipo DGII 31). Número, período, fechas emisión/vencimiento, montos base/impuesto/total/pagado, estado, flag anticipo. Nav: Cliente, ComprobanteFiscal, Detalles, PagosAplicados |
| `InvoiceDetail` | InvoiceDetails | Línea de factura (descripción, período cubierto, cantidad, precio unitario, subtotal, número línea) |
| `Payment` | Payments | Registro de pago/anticipo (monto, fecha, tipo, referencia, método pago, notas, usuario registrador). Nav: Cliente, FacturasAplicadas |
| `PaymentInvoiceMapping` | PaymentInvoiceMappings | Relación muchos a muchos: distribuye pago entre facturas (monto aplicado, fecha aplicación) |
| `Tax` | Taxes | Definición de impuesto (nombre como ITBIS, descripción, porcentaje, flag activo) |
| `FiscalVoucher` | FiscalVouchers | Seguimiento de comprobante fiscal DGII (número secuencia, fecha registro, rango autorizado, período) |

#### Entidades PPPoE / MikroTik

| Entidad | Tabla | Propósito |
|---------|-------|-----------|
| `PppProfile` | PppProfiles | Perfil PPPoE MikroTik (nombre, dirección local/remota, DNS, límite velocidad, router, sincronizado) |
| `PppSecret` | PppSecrets | Credenciales PPPoE (nombre, contraseña, servicio, perfil, flag deshabilitado, router, sincronizado). Vinculado por Suscripción |
| `PppServer` | PppServers | Configuración servidor PPPoE (nombre, interfaz, perfil defecto, MTU/MRU, keepalive, router, sincronizado) |

#### Enumeraciones

| Enum | Valores |
|------|---------|
| `BillingCycleType` | `Mensual = 0`, `Quincenal = 1` |
| `InvoiceStatus` | `Vigente = 0`, `Pagada = 1`, `Abonada = 2`, `Anulada = 3`, `Vencida = 4` |
| `PaymentType` | `Pago = 0`, `Abono = 1` |
| `SubscriptionStatus` | `Activa = 0`, `Suspendida = 1`, `Cancelada = 2`, `Prueba = 3` |

#### Operaciones PPPoE MikroTik

**Ubicación:** `MikroClean.Domain\MikroTik\Operations\PPPoE\`

| Directorio | Archivos | Propósito |
|---|---|---|
| Raíz | `GetAllPPPoEActiveConnectionsQuery.cs`, `DeletePPPoEActiveConnectionOperation.cs` | Gestión conexiones activas |
| `Profiles/` | CRUD completo (Create, Delete, GetAll, GetById, Update) | Perfiles PPPoE |
| `Secrets/` | CRUD completo | Credenciales PPPoE |
| `Servers/` | CRUD completo | Servidores PPPoE |

**Modelos de Respuesta (`OperationModels.cs` líneas 251-380):**
- `PPPoEProfileResponse` - Datos de perfil con límites de velocidad, direcciones
- `PPPoESecretResponse` - Datos de credenciales
- `PPPoEServerResponse` - Configuración de servidor
- `PPPoEActiveConnectionResponse` - Conexión activa con **campos comerciales enriquecidos**:
  - `TieneClienteAsociado`
  - `ClienteId`
  - `ClienteNombre`
  - `PlanNombre`
  - `PlanVelocidadMbps`
  - `EstadoSuscripcion`

---

### Capa de Infraestructura (`MikroClean.Infrastructure`)

#### Implementaciones de Repositorios

| Repositorio | Métodos Clave |
|-------------|---------------|
| `ClienteRepository` | Filtrado por OrganizationId, búsqueda por cédula/email únicos, respeto soft-delete |
| `PlanRepository` | Filtrado por RouterId, ordenado por velocidad |
| `SubscriptionRepository` | Includes Plan + PppSecret, activos por cliente, activos por router, búsqueda por router+nombre secreto |
| `BillingTemplateRepository` | Activos por cliente, plantillas con vencimiento en día específico |
| `InvoiceRepository` | Includes Detalles, por cliente con ordenamiento, por cliente+estado, pendientes (Vigente/Abonada/Vencida), último número por período |
| `PaymentRepository` | Includes mapeos, por cliente ordenado por fecha, pago individual con mapeos |
| `TaxRepository` | Solo impuestos activos, búsqueda por nombre |
| `PppSecretRepository` | Búsqueda por RouterId + Nombre |

#### Configuraciones EF Core

| Configuración | Detalles |
|---------------|----------|
| `ClienteConfiguration` | Índice único en Cédula, índice org+activo |
| `PlanConfiguration` | Índices router+defecto y router+activo |
| `SubscriptionConfiguration` | Índice cliente+estado, FK a Cliente(cascade), Plan(restrict), PppSecret(set null) |
| `BillingTemplateConfiguration` | Índice único en ClienteId+IsActive |
| `InvoiceConfiguration` | NúmeroFactura único, índices en ClienteId+Estado, Periodo+Estado, ClienteId+FechaVencimiento, FK a Cliente(cascade), FiscalVoucher(set null) |
| `InvoiceDetailConfiguration` | Índices en InvoiceId y InvoiceId+NumeroLinea |
| `PaymentConfiguration` | Índices en ClienteId+FechaPago y Referencia |
| `PaymentInvoiceMappingConfiguration` | PK compuesta (PaymentId, InvoiceId), no-delete en Payment, cascade en Invoice |
| `TaxConfiguration` | Índice único en Nombre |
| `PppSecretConfiguration` | Índice único en RouterId+Name |

#### DbContext (`MikroCleanContext.cs`)

**DbSets declarados:**
- `Clientes`, `Planes`, `Subscripciones`, `BillingTemplates`
- `Invoices`, `InvoiceDetails`, `Payments`, `PaymentInvoiceMappings`
- `Taxes`, `FiscalVouchers`

#### Migración de Facturación (`20260328111843_AddBillingTables.cs`)

**Tablas creadas:**
- Clientes, FiscalVouchers, Planes, Taxes
- BillingTemplates, Payments, Invoices
- Subscripciones, InvoiceDetails, PaymentInvoiceMappings

**Datos iniciales:**
- Seed de impuesto ITBIS al 18%

**Índices y claves foráneas:** Todos configurados

---

## 🔄 Mapa de Relaciones de Entidades

```
Organization
    │
    └── Cliente
            │
            ├── Subscripciones ────> Plan (scoped to Router)
            │                           │
            │                           └── PppSecret (Credenciales MikroTik)
            │
            ├── BillingTemplate (Config facturación por cliente)
            │
            ├── Invoices ────> InvoiceDetails
            │       │
            │       ├── FiscalVoucher (Cumplimiento DGII)
            │       │
            │       └── PaymentInvoiceMappings <── Payments
            │
            └── Payments ────> PaymentInvoiceMappings ────> Invoices
```

---

## ✅ Funcionalidades Completamente Implementadas

| Funcionalidad | Estado |
|---------------|--------|
| Gestión de clientes (CRUD, scoped por organización) | ✅ Completo |
| Gestión de planes (scoped por router, con velocidad y precio) | ✅ Completo |
| Gestión de suscripciones (cliente + plan + secreto PPPoE) | ✅ Completo |
| Plantillas de facturación (configuración por cliente) | ✅ Completo |
| Generación de facturas con cálculo automático de impuestos | ✅ Completo |
| Numeración de facturas compatible con DGII | ✅ Completo |
| Generación de PDF de facturas (QuestPDF) | ✅ Completo |
| Registro de pagos con asignación multi-factura | ✅ Completo |
| Generación de recibos de pago en PDF | ✅ Completo |
| Cancelación de facturas (con protección de pagadas) | ✅ Completo |
| Enriquecimiento de conexiones PPPoE activas con datos comerciales | ✅ Completo |
| CRUD completo de PPPoE (Perfiles, Secretos, Servidores) | ✅ Completo |
| Gestión de impuestos (seed con ITBIS 18%) | ✅ Completo |
| Seguimiento de comprobantes fiscales DGII | ✅ Completo |

---

## ⚠️ Áreas Incompletas o Pendientes

| Área | Descripción | Prioridad |
|------|-------------|-----------|
| **FiscalVoucherRepository** | No existe repositorio dedicado para FiscalVoucher, aunque la entidad está definida y es referenciada por Invoice | Media |
| **Generación automática de facturas por ciclo** | No hay servicio en background que genere facturas automáticamente según los ciclos de facturación. `PendingChangesProcessorService` solo maneja sincronización MikroTik | **Alta** |
| **Detección de facturas vencidas** | No hay lógica para detectar facturas vencidas y suspender automáticamente el servicio | **Alta** |
| **Generación masiva de facturas** | No existe funcionalidad para generar todas las facturas de un mes de una vez | Media |
| **Notificaciones al cliente** | No hay sistema de notificación (email/SMS) de facturas generadas, vencidas o pagos recibidos | Media |
| **Controladores API** | No se encontraron controladores API dedicados para facturación en esta búsqueda (pueden estar en proyecto Web/API separado) | Media |
| **Reportes comerciales** | No hay reportes de ingresos, morosidad, clientes activos, etc. | Baja |
| **Integración de suspensión automática** | No hay lógica que suspenda automáticamente servicios PPPoE cuando facturas están vencidas | **Alta** |
| **Historial de cambios de plan** | No hay auditoría de cambios de plan con prorrateo | Baja |
| **Descuentos y promociones** | No hay soporte para descuentos por pronto pago, promociones, etc. | Baja |

---

## 🛠️ Servicios en Background

### `PendingChangesProcessorService`

- **Propósito:** Llama periódicamente a `mikroTikService.ProcessPendingChangesAsync()`
- **Intervalo por defecto:** 30 segundos
- **Configuración:** `MikroTik:PendingChangesProcessor:IntervalSeconds` y `MikroTik:PendingChangesProcessor:LogLevel`
- **Limitación:** Solo maneja sincronización MikroTik, **no** procesa facturación automática

---

## 📊 Flujo de Generación de Factura

```
1. Solicitar GenerateInvoiceAsync(clienteId, período, flagAnticipo)
        ↓
2. Obtener Cliente y suscripción activa
        ↓
3. Obtener plantilla de facturación del cliente
        ↓
4. Calcular período de servicio cubierto
        ↓
5. Obtener precio base del plan
        ↓
6. Obtener impuestos activos y calcular montos
        ↓
7. Calcular fecha de vencimiento (día corte + días gracia)
        ↓
8. Generar número de factura secuencial (B31{YYYYMM}{secuencia})
        ↓
9. Crear Invoice + InvoiceDetails en transacción
        ↓
10. Retornar InvoiceDTO completa
```

---

## 💳 Flujo de Procesamiento de Pago

```
1. Solicitar RegisterPaymentAsync(clienteId, monto, referencia, método, etc.)
        ↓
2. Obtener Cliente
        ↓
3. ¿Asignaciones explícitas proporcionadas?
        ├─ SÍ → Aplicar pagos a facturas específicas
        └─ NO → Distribuir automáticamente en facturas pendientes (más antiguas primero)
        ↓
4. Para cada factura afectada:
        ├─ Actualizar MontoPagado
        ├─ Si MontoPagado >= MontoTotal → Estado = "Pagada"
        └─ Si MontoPagado > 0 && < MontoTotal → Estado = "Abonada"
        ↓
5. Crear registro Payment
        ↓
6. Crear registros PaymentInvoiceMapping
        ↓
7. Guardar todo en transacción (UnitOfWork)
        ↓
8. Retornar PaymentDTO con asignaciones
```

---

## 📌 Notas Técnicas

- **Framework:** .NET con arquitectura limpia (Application/Domain/Infrastructure)
- **ORM:** Entity Framework Core
- **Generación de PDFs:** QuestPDF
- **Cumplimiento Fiscal:** DGII República Dominicana (tipo comprobante 31, numeración B31)
- **Impuesto principal:** ITBIS 18% (configurable en Taxes)
- **Base de Datos:** SQL Server (por configuraciones de EF Core)
- **Patrones:** Repository, Unit of Work, DTOs, Service Layer

---

## 📅 Fecha de Documentación

**Última actualización:** miércoles, 15 de abril de 2026

---

*Documento generado automáticamente desde el análisis del código fuente del proyecto MikroClean.*
