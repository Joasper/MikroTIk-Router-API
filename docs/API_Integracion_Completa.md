# API MikroClean - Documentación Completa de Integración Comercial y PPPoE

## Información General

| Propiedad | Valor |
|-----------|-------|
| **Base URL** | `http://localhost:5000/api` (desarrollo) |
| **Autenticación** | JWT Bearer Token |
| **Formato Request/Response** | JSON |
| **CORS Habilitado** | `http://localhost:4200`, `http://localhost:4201` |
| **Swagger** | Disponible en entorno de desarrollo en `/swagger` |

---

## Tabla de Contenidos

1. [Módulo de Facturación (Billing)](#1-módulo-de-facturación-billing)
2. [Módulo de Gestión de Planes (Plans)](#2-módulo-de-gestión-de-planes-plans)
3. [Módulo de Ventas (Sales)](#3-módulo-de-ventas-sales)
4. [Módulo PPPoE MikroTik](#4-módulo-pppoe-mikrotik)
5. [Modelos de Respuesta Estándar](#5-modelos-de-respuesta-estándar)
6. [Catálogo de Errores](#6-catálogo-de-errores)
7. [Guía de Integración PPPoE](#7-guía-de-integración-pppoe)

---

## 1. Módulo de Facturación (Billing)

**Base Path:** `/api/billing`

### 1.1 Gestión de Clientes

#### 1.1.1 Crear Cliente

**Endpoint:** `POST /api/billing/clientes`

**Descripción:** Crea un nuevo cliente de facturación. Valida unicidad de cédula y email dentro de la organización.

**Request Body:**
```json
{
  "nombre": "Juan Pérez",
  "cedula": "001-1234567-8",
  "email": "juan.perez@email.com",
  "telefono": "809-555-1234",
  "direccion": "Calle Principal #123, Santo Domingo",
  "referenciaPago": "Transferencia Bancaria",
  "organizationId": 1
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `nombre` | string | ✅ | Nombre completo del cliente |
| `cedula` | string | ✅ | Cédula o RNC para facturación DGII |
| `email` | string | ✅ | Email de contacto (debe ser único en la organización) |
| `telefono` | string | ✅ | Teléfono de contacto |
| `direccion` | string | ✅ | Dirección física del cliente |
| `referenciaPago` | string | ❌ | Referencia o método de pago preferido |
| `organizationId` | int | ✅ | ID de la organización a la que pertenece |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Cliente creado exitosamente",
  "data": {
    "id": 1,
    "nombre": "Juan Pérez",
    "cedula": "001-1234567-8",
    "email": "juan.perez@email.com",
    "telefono": "809-555-1234",
    "direccion": "Calle Principal #123, Santo Domingo",
    "referenciaPago": "Transferencia Bancaria",
    "isActive": true,
    "organizationId": 1
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response de Error (400 Bad Request):**
```json
{
  "status": "ValidationError",
  "message": "Ya existe un cliente con la cédula especificada",
  "errors": {
    "cedula": ["La cédula '001-1234567-8' ya está registrada en la organización"]
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 1.1.2 Obtener Clientes por Organización

**Endpoint:** `GET /api/billing/clientes/organization/{organizationId}`

**Descripción:** Obtiene todos los clientes activos de una organización.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `organizationId` | int | ✅ | ID de la organización |

**Request:**
```
GET /api/billing/clientes/organization/1
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Clientes obtenidos exitosamente",
  "data": [
    {
      "id": 1,
      "nombre": "Juan Pérez",
      "cedula": "001-1234567-8",
      "email": "juan.perez@email.com",
      "telefono": "809-555-1234",
      "direccion": "Calle Principal #123, Santo Domingo",
      "referenciaPago": "Transferencia Bancaria",
      "isActive": true,
      "organizationId": 1
    },
    {
      "id": 2,
      "nombre": "María García",
      "cedula": "001-9876543-2",
      "email": "maria.garcia@email.com",
      "telefono": "809-555-5678",
      "direccion": "Avenida Central #456, Santiago",
      "referenciaPago": null,
      "isActive": true,
      "organizationId": 1
    }
  ],
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 1.2 Gestión de Suscripciones

#### 1.2.1 Crear Suscripción

**Endpoint:** `POST /api/billing/subscriptions`

**Descripción:** Crea una nueva suscripción de un cliente a un plan PPPoE. Desactiva automáticamente cualquier suscripción activa previa del mismo cliente.

**Request Body:**
```json
{
  "clienteId": 1,
  "planId": 3,
  "pppSecretId": 5,
  "notas": "Cliente nuevo, instalación el 15/04/2026"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |
| `planId` | int | ✅ | ID del plan contratado |
| `pppSecretId` | int | ❌ | ID del secreto PPPoE (credenciales) |
| `notas` | string | ❌ | Notas adicionales sobre la suscripción |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Suscripción creada exitosamente",
  "data": {
    "id": 10,
    "clienteId": 1,
    "planId": 3,
    "pppSecretId": 5,
    "pppSecretName": "cliente_jp_001",
    "planNombre": "Plan Premium 20MB",
    "velocidadMbps": 20,
    "precioMensual": 1500.00,
    "estado": 0
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Notas:** 
- `estado`: 0=Activa, 1=Suspendida, 2=Cancelada, 3=Prueba
- Al crear una nueva suscripción, cualquier suscripción activa previa del cliente se desactiva automáticamente

---

#### 1.2.2 Obtener Suscripción Activa

**Endpoint:** `GET /api/billing/subscriptions/cliente/{clienteId}/active`

**Descripción:** Obtiene la suscripción activa actual de un cliente.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Suscripción activa obtenida exitosamente",
  "data": {
    "id": 10,
    "clienteId": 1,
    "planId": 3,
    "pppSecretId": 5,
    "pppSecretName": "cliente_jp_001",
    "planNombre": "Plan Premium 20MB",
    "velocidadMbps": 20,
    "precioMensual": 1500.00,
    "estado": 0
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response Sin Suscripción Activa (404 Not Found):**
```json
{
  "status": "NotFound",
  "message": "No se encontró una suscripción activa para el cliente",
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 1.3 Configuración de Facturación

#### 1.3.1 Configurar Plantilla de Facturación

**Endpoint:** `POST /api/billing/templates`

**Descripción:** Configura o actualiza la plantilla de facturación de un cliente. Define el ciclo de facturación, día de corte, días de gracia, etc.

**Request Body:**
```json
{
  "clienteId": 1,
  "diaInicio": 1,
  "diaCutoff": 25,
  "diasGracia": 5,
  "tipoCiclo": 0,
  "esPrePago": true
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |
| `diaInicio` | int | ✅ | Día del mes que inicia el ciclo de facturación (1-28) |
| `diaCutoff` | int | ✅ | Día del mes de corte para facturación (1-28) |
| `diasGracia` | int | ✅ | Días de gracia después del vencimiento antes de suspensión |
| `tipoCiclo` | int | ✅ | 0=Mensual, 1=Quincenal |
| `esPrePago` | bool | ✅ | `true` = Pre-pago, `false` = Post-pago |

**Enum BillingCycleType:**
| Valor | Descripción |
|-------|-------------|
| 0 | Mensual |
| 1 | Quincenal |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plantilla de facturación configurada exitosamente",
  "data": true,
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 1.4 Gestión de Facturas

#### 1.4.1 Generar Factura

**Endpoint:** `POST /api/billing/invoices/generate`

**Descripción:** Genera una nueva factura para un cliente. Calcula automáticamente impuestos (ITBIS 18% por defecto) y genera número de comprobante fiscal DGII.

**Request Body:**
```json
{
  "clienteId": 1,
  "periodo": "2026-04",
  "esAbono": false,
  "montoAbono": null
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |
| `periodo` | string | ❌ | Período de facturación (formato YYYY-MM). Si se omite, usa el período actual |
| `esAbono` | bool | ✅ | Indica si es un pago de anticipo |
| `montoAbono` | decimal | ❌ | Monto específico para anticipo (solo si `esAbono` = true) |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Factura generada exitosamente",
  "data": {
    "id": 100,
    "clienteId": 1,
    "numeroFactura": "B31202604000001",
    "tipoComprobante": 31,
    "periodo": "2026-04",
    "fechaEmision": "2026-04-15T00:00:00",
    "fechaVencimiento": "2026-05-05T00:00:00",
    "montoBase": 1500.00,
    "montoImpuesto": 270.00,
    "total": 1770.00,
    "montoPagado": 0.00,
    "estado": 0,
    "esAbono": false,
    "detalles": [
      {
        "numeroLinea": 1,
        "descripcion": "Servicio Internet - Plan Premium 20MB",
        "periodoCubierto": "01/04/2026 - 30/04/2026",
        "cantidad": 1,
        "precioUnitario": 1500.00,
        "subtotal": 1500.00
      }
    ]
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Enum InvoiceStatus:**
| Valor | Estado |
|-------|--------|
| 0 | Vigente |
| 1 | Pagada |
| 2 | Abonada |
| 3 | Anulada |
| 4 | Vencida |

**Notas:**
- El número de factura sigue el formato DGII: `B31{YYYYMM}{secuencia}` (ej: `B31202604000001`)
- La fecha de vencimiento se calcula automáticamente según la plantilla de facturación del cliente (día de corte + días de gracia)
- Los impuestos se calculan automáticamente desde la tabla `Taxes` (ITBIS 18% por defecto)

---

#### 1.4.2 Obtener Facturas por Cliente

**Endpoint:** `GET /api/billing/invoices/cliente/{clienteId}`

**Descripción:** Obtiene todas las facturas de un cliente, ordenadas por fecha de emisión descendente.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Facturas obtenidas exitosamente",
  "data": [
    {
      "id": 100,
      "clienteId": 1,
      "numeroFactura": "B31202604000001",
      "tipoComprobante": 31,
      "periodo": "2026-04",
      "fechaEmision": "2026-04-15T00:00:00",
      "fechaVencimiento": "2026-05-05T00:00:00",
      "montoBase": 1500.00,
      "montoImpuesto": 270.00,
      "total": 1770.00,
      "montoPagado": 1770.00,
      "estado": 1,
      "esAbono": false,
      "detalles": [
        {
          "numeroLinea": 1,
          "descripcion": "Servicio Internet - Plan Premium 20MB",
          "periodoCubierto": "01/04/2026 - 30/04/2026",
          "cantidad": 1,
          "precioUnitario": 1500.00,
          "subtotal": 1500.00
        }
      ]
    }
  ],
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 1.4.3 Descargar PDF de Factura

**Endpoint:** `GET /api/billing/invoices/{invoiceId}/pdf`

**Descripción:** Genera y descarga el PDF de una factura.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `invoiceId` | int | ✅ | ID de la factura |

**Response Exitosa (200 OK):**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="factura-100.pdf"

[Binary PDF Data]
```

**Response de Error (404 Not Found):**
```json
{
  "status": "NotFound",
  "message": "No se encontró la factura especificada",
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 1.4.4 Cancelar Factura

**Endpoint:** `POST /api/billing/invoices/{invoiceId}/cancel`

**Descripción:** Cancela/anula una factura. No permite cancelar facturas que ya están completamente pagadas.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `invoiceId` | int | ✅ | ID de la factura |

**Request Body (Opcional):**
```json
"Factura cancelada por error en la facturación"
```

**Campos del Request:**
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| (body) | string | ❌ | Motivo de la cancelación |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Factura cancelada exitosamente",
  "data": true,
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response de Error - Factura Pagada (400 Bad Request):**
```json
{
  "status": "ValidationError",
  "message": "No se puede cancelar una factura que ya está completamente pagada",
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 1.5 Gestión de Pagos

#### 1.5.1 Registrar Pago

**Endpoint:** `POST /api/billing/payments`

**Descripción:** Registra un pago y lo aplica a facturas pendientes. Soporta asignación explícita a facturas específicas o distribución automática (facturas más antiguas primero).

**Request Body - Con asignación automática:**
```json
{
  "clienteId": 1,
  "monto": 3540.00,
  "fechaPago": "2026-04-15T10:30:00",
  "referencia": "REF-2026-001",
  "metodoPago": "Transferencia Bancaria",
  "notas": "Pago de facturas de abril y marzo",
  "esAbono": false,
  "allocations": null
}
```

**Request Body - Con asignación explícita:**
```json
{
  "clienteId": 1,
  "monto": 1770.00,
  "fechaPago": "2026-04-15T10:30:00",
  "referencia": "REF-2026-001",
  "metodoPago": "Efectivo",
  "notas": "Pago factura abril",
  "esAbono": false,
  "allocations": [
    {
      "invoiceId": 100,
      "montoAplicado": 1770.00
    }
  ]
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |
| `monto` | decimal | ✅ | Monto total del pago |
| `fechaPago` | DateTime | ❌ | Fecha del pago (default: ahora) |
| `referencia` | string | ❌ | Número de referencia del pago |
| `metodoPago` | string | ❌ | Método de pago (Efectivo, Transferencia, Tarjeta, etc.) |
| `notas` | string | ❌ | Notas adicionales |
| `esAbono` | bool | ✅ | Indica si es un pago de anticipo |
| `allocations` | array | ❌ | Asignaciones explícitas a facturas específicas |

**Campo de Allocation:**
| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `invoiceId` | int | ✅ | ID de la factura |
| `montoAplicado` | decimal | ✅ | Monto a aplicar a esta factura |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Pago registrado exitosamente",
  "data": {
    "id": 50,
    "clienteId": 1,
    "monto": 1770.00,
    "fechaPago": "2026-04-15T10:30:00",
    "tipo": 0,
    "referencia": "REF-2026-001",
    "metodoPago": "Transferencia Bancaria",
    "facturasAplicadas": [
      {
        "invoiceId": 100,
        "montoAplicado": 1770.00
      }
    ]
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Enum PaymentType:**
| Valor | Tipo |
|-------|------|
| 0 | Pago |
| 1 | Abono |

**Lógica de Estados de Factura tras Pago:**
- Si `MontoPagado >= Total` → Estado cambia a `Pagada` (1)
- Si `MontoPagado > 0 && < Total` → Estado cambia a `Abonada` (2)
- Si `MontoPagado == 0` → Estado permanece `Vigente` (0)

---

#### 1.5.2 Obtener Pagos por Cliente

**Endpoint:** `GET /api/billing/payments/cliente/{clienteId}`

**Descripción:** Obtiene todos los pagos registrados de un cliente, ordenados por fecha descendente.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `clienteId` | int | ✅ | ID del cliente |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Pagos obtenidos exitosamente",
  "data": [
    {
      "id": 50,
      "clienteId": 1,
      "monto": 1770.00,
      "fechaPago": "2026-04-15T10:30:00",
      "tipo": 0,
      "referencia": "REF-2026-001",
      "metodoPago": "Transferencia Bancaria",
      "facturasAplicadas": [
        {
          "invoiceId": 100,
          "montoAplicado": 1770.00
        }
      ]
    }
  ],
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 1.5.3 Descargar Recibo de Pago

**Endpoint:** `GET /api/billing/payments/{paymentId}/receipt`

**Descripción:** Genera y descarga el PDF del recibo de un pago.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `paymentId` | int | ✅ | ID del pago |

**Response Exitosa (200 OK):**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="recibo-50.pdf"

[Binary PDF Data]
```

---

### 1.6 Conexiones PPPoE Activas Enriquecidas

#### 1.6.1 Obtener Conexiones Activas por Router

**Endpoint:** `GET /api/billing/connections/router/{routerId}`

**Descripción:** Obtiene las conexiones PPPoE activas de un router, enriquecidas con datos comerciales del cliente y plan.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Query Parameters:**

| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| `pageNumber` | int | ❌ | 1 | Número de página |
| `pageSize` | int | ❌ | 20 | Registros por página |
| `searchTerm` | string | ❌ | null | Término de búsqueda (filtra por nombre de secreto) |

**Request:**
```
GET /api/billing/connections/router/1?pageNumber=1&pageSize=20&searchTerm=cliente_jp
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Conexiones activas obtenidas exitosamente",
  "data": {
    "items": [
      {
        "connectionId": "*1A2B",
        "pppSecretName": "cliente_jp_001",
        "service": "pppoe",
        "callerId": "AA:BB:CC:DD:EE:FF",
        "address": "192.168.100.50",
        "uptime": "2d 15h 30m",
        "tieneClienteAsociado": true,
        "clienteId": 1,
        "clienteNombre": "Juan Pérez",
        "planNombre": "Plan Premium 20MB",
        "planVelocidadMbps": 20,
        "estadoSuscripcion": 0
      }
    ],
    "totalRecords": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalRecords": 1,
    "totalPages": 1,
    "hasPrevious": false,
    "hasNext": false
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Campos de ActiveClientConnectionDTO:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `connectionId` | string | ID de conexión en MikroTik |
| `pppSecretName` | string | Nombre del secreto PPPoE |
| `service` | string | Tipo de servicio (pppoe, pptp, etc.) |
| `callerId` | string | MAC address del cliente |
| `address` | string | IP asignada al cliente |
| `uptime` | string | Tiempo de conexión activa |
| `tieneClienteAsociado` | bool | Indica si tiene cliente comercial asociado |
| `clienteId` | int? | ID del cliente (si existe) |
| `clienteNombre` | string? | Nombre del cliente |
| `planNombre` | string? | Nombre del plan contratado |
| `planVelocidadMbps` | int? | Velocidad del plan en Mbps |
| `estadoSuscripcion` | int? | Estado de suscripción (0=Activa, 1=Suspendida, 2=Cancelada, 3=Prueba) |

---

## 2. Módulo de Gestión de Planes (Plans)

**Base Path:** `/api/plan`

### 2.1 ¿Qué son los Planes?

Los **Planes** representan los paquetes de servicio PPPoE que ofreces a tus clientes. Cada plan define:

- **Velocidad** del servicio (en Mbps)
- **Precio mensual** (en RD$)
- **Perfil PPPoE** asociado en el router MikroTik
- Configuración específica de velocidad, DNS, etc.

**Relación con otros componentes:**

```
Plan (define velocidad y precio)
    │
    ├── PPPoE Profile (perfil en MikroTik con rate-limit)
    │       │
    │       └── PPPoE Secret (credenciales del cliente)
    │               │
    │               └── Subscription (vincula Cliente + Plan + PPPoE Secret)
    │                       │
    │                       └── Cliente (datos de facturación)
    │
    └── Invoice (factura generada según precio del plan)
```

### 2.2 Crear Plan

**Endpoint:** `POST /api/plan`

**Descripción:** Crea un nuevo plan de servicio PPPoE asociado a un router. Valida que el nombre sea único dentro del router.

**Request Body:**
```json
{
  "nombre": "Plan Premium 20MB",
  "descripcion": "Plan de alta velocidad para usuarios exigentes",
  "velocidadMbps": 20,
  "precioMensual": 1500.00,
  "esDefault": false,
  "routerId": 1
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `nombre` | string | ✅ | Nombre del plan (debe ser único en el router) |
| `descripcion` | string | ❌ | Descripción detallada del plan |
| `velocidadMbps` | int | ✅ | Velocidad del servicio en Mbps |
| `precioMensual` | decimal | ✅ | Precio mensual en moneda local (RD$) |
| `esDefault` | bool | ❌ | Indica si es el plan por defecto (default: false) |
| `routerId` | int | ✅ | ID del router al que pertenece el plan |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plan creado exitosamente",
  "data": {
    "id": 3,
    "nombre": "Plan Premium 20MB",
    "descripcion": "Plan de alta velocidad para usuarios exigentes",
    "velocidadMbps": 20,
    "precioMensual": 1500.00,
    "esDefault": false,
    "isActive": true,
    "routerId": 1,
    "routerName": "Router Principal"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response de Error - Nombre Duplicado (400 Bad Request):**
```json
{
  "status": "ValidationError",
  "message": "Ya existe un plan activo con ese nombre en el router especificado",
  "errors": {
    "nombre": ["El plan 'Plan Premium 20MB' ya existe en este router"]
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Notas Importantes:**
- Si estableces `esDefault: true`, automáticamente se desactivan otros planes default del mismo router
- El nombre del plan debe ser único dentro del mismo router
- Un plan puede tener múltiples perfiles PPPoE asociados en el MikroTik

---

### 2.3 Obtener Plan por ID

**Endpoint:** `GET /api/plan/{planId}`

**Descripción:** Obtiene los detalles completos de un plan específico.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `planId` | int | ✅ | ID del plan |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plan obtenido exitosamente",
  "data": {
    "id": 3,
    "nombre": "Plan Premium 20MB",
    "descripcion": "Plan de alta velocidad para usuarios exigentes",
    "velocidadMbps": 20,
    "precioMensual": 1500.00,
    "esDefault": false,
    "isActive": true,
    "routerId": 1,
    "routerName": "Router Principal"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 2.4 Obtener Planes por Router

**Endpoint:** `GET /api/plan/router/{routerId}`

**Descripción:** Obtiene todos los planes activos de un router específico, ordenados por velocidad (menor a mayor).

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request:**
```
GET /api/plan/router/1
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Se encontraron 3 planes",
  "data": [
    {
      "id": 1,
      "nombre": "Plan Básico 5MB",
      "descripcion": "Plan económico para uso básico",
      "velocidadMbps": 5,
      "precioMensual": 500.00,
      "esDefault": true,
      "isActive": true,
      "routerId": 1,
      "routerName": "Router Principal"
    },
    {
      "id": 2,
      "nombre": "Plan Estándar 10MB",
      "descripcion": "Plan para usuarios regulares",
      "velocidadMbps": 10,
      "precioMensual": 800.00,
      "esDefault": false,
      "isActive": true,
      "routerId": 1,
      "routerName": "Router Principal"
    },
    {
      "id": 3,
      "nombre": "Plan Premium 20MB",
      "descripcion": "Plan de alta velocidad para usuarios exigentes",
      "velocidadMbps": 20,
      "precioMensual": 1500.00,
      "esDefault": false,
      "isActive": true,
      "routerId": 1,
      "routerName": "Router Principal"
    }
  ],
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 2.5 Obtener Todos los Planes Activos

**Endpoint:** `GET /api/plan/active`

**Descripción:** Obtiene todos los planes activos de todos los routers del sistema.

**Request:**
```
GET /api/plan/active
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Se encontraron 5 planes activos",
  "data": [
    {
      "id": 1,
      "nombre": "Plan Básico 5MB",
      "descripcion": "Plan económico para uso básico",
      "velocidadMbps": 5,
      "precioMensual": 500.00,
      "esDefault": true,
      "isActive": true,
      "routerId": 1,
      "routerName": "Router Principal"
    },
    {
      "id": 4,
      "nombre": "Plan Básico 5MB",
      "descripcion": "Plan estándar zona norte",
      "velocidadMbps": 5,
      "precioMensual": 550.00,
      "esDefault": true,
      "isActive": true,
      "routerId": 2,
      "routerName": "Router Norte"
    }
  ],
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 2.6 Actualizar Plan

**Endpoint:** `PUT /api/plan`

**Descripción:** Actualiza un plan existente. Solo los campos proporcionados serán actualizados. Permite cambiar nombre, velocidad, precio, descripción, o establecer como default.

**Request Body:**
```json
{
  "id": 3,
  "nombre": "Plan Premium 25MB",
  "velocidadMbps": 25,
  "precioMensual": 1800.00,
  "esDefault": true
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `id` | int | ✅ | ID del plan a actualizar |
| `nombre` | string | ❌ | Nuevo nombre del plan |
| `descripcion` | string | ❌ | Nueva descripción |
| `velocidadMbps` | int | ❌ | Nueva velocidad en Mbps |
| `precioMensual` | decimal | ❌ | Nuevo precio mensual |
| `esDefault` | bool | ❌ | Establecer como plan default |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plan actualizado exitosamente",
  "data": {
    "id": 3,
    "nombre": "Plan Premium 25MB",
    "descripcion": "Plan de alta velocidad para usuarios exigentes",
    "velocidadMbps": 25,
    "precioMensual": 1800.00,
    "esDefault": true,
    "isActive": true,
    "routerId": 1,
    "routerName": "Router Principal"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Notas:**
- Al establecer `esDefault: true`, automáticamente se desactivan otros planes default del mismo router
- El nombre debe seguir siendo único dentro del router

---

### 2.7 Eliminar Plan

**Endpoint:** `DELETE /api/plan/{planId}`

**Descripción:** Desactiva un plan (soft delete). No permite eliminar planes que tienen suscripciones activas.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `planId` | int | ✅ | ID del plan |

**Request:**
```
DELETE /api/plan/3
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plan elimininado exitosamente",
  "data": true,
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response de Error - Con Suscripciones Activas (400 Bad Request):**
```json
{
  "status": "ValidationError",
  "message": "No se puede eliminar el plan porque tiene 5 suscripción(es) activa(s)",
  "errors": {
    "suscripcionesActivas": 5
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 2.8 Establecer Plan como Default

**Endpoint:** `POST /api/plan/{planId}/set-default`

**Descripción:** Establece un plan como el plan por defecto para su router. Automáticamente desactiva otros planes default del mismo router.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `planId` | int | ✅ | ID del plan |

**Request:**
```
POST /api/plan/3/set-default
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plan establecido como default exitosamente",
  "data": {
    "id": 3,
    "nombre": "Plan Premium 20MB",
    "descripcion": "Plan de alta velocidad para usuarios exigentes",
    "velocidadMbps": 20,
    "precioMensual": 1500.00,
    "esDefault": true,
    "isActive": true,
    "routerId": 1,
    "routerName": "Router Principal"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 2.9 Obtener Estadísticas de Planes

**Endpoint:** `GET /api/plan/router/{routerId}/statistics`

**Descripción:** Obtiene estadísticas de todos los planes de un router, incluyendo número de suscripciones activas, suspendidas e ingreso mensual estimado.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request:**
```
GET /api/plan/router/1/statistics
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Estadísticas de planes obtenidas exitosamente",
  "data": [
    {
      "id": 1,
      "nombre": "Plan Básico 5MB",
      "velocidadMbps": 5,
      "precioMensual": 500.00,
      "totalSuscripciones": 25,
      "suscripcionesActivas": 23,
      "suscripcionesSuspendidas": 2,
      "ingresoMensualEstimado": 11500.00
    },
    {
      "id": 2,
      "nombre": "Plan Estándar 10MB",
      "velocidadMbps": 10,
      "precioMensual": 800.00,
      "totalSuscripciones": 15,
      "suscripcionesActivas": 14,
      "suscripcionesSuspendidas": 1,
      "ingresoMensualEstimado": 11200.00
    },
    {
      "id": 3,
      "nombre": "Plan Premium 20MB",
      "velocidadMbps": 20,
      "precioMensual": 1500.00,
      "totalSuscripciones": 8,
      "suscripcionesActivas": 8,
      "suscripcionesSuspendidas": 0,
      "ingresoMensualEstimado": 12000.00
    }
  ],
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Campos de PlanStatisticsDTO:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | int | ID del plan |
| `nombre` | string | Nombre del plan |
| `velocidadMbps` | int | Velocidad en Mbps |
| `precioMensual` | decimal | Precio mensual |
| `totalSuscripciones` | int | Total de suscripciones (activas + suspendidas) |
| `suscripcionesActivas` | int | Número de suscripciones con estado "Activa" |
| `suscripcionesSuspendidas` | int | Número de suscripciones con estado "Suspendida" |
| `ingresoMensualEstimado` | decimal | Ingreso mensual estimado (suscripciones activas * precio) |

---

## 3. Módulo de Ventas (Sales)

**Base Path:** `/api/sales`

### 3.1 Flujo Completo de Activación de Servicio

#### 3.1.1 Activar Nuevo Servicio

**Endpoint:** `POST /api/sales/activate`

**Descripción:** Este endpoint orquesta la activación completa de un nuevo servicio de internet para un cliente. Realiza las siguientes operaciones en secuencia:

1. **Busca o crea el cliente** (por cédula o email)
2. **Crea el secreto PPPoE** en el router MikroTik
3. **Crea la suscripción** (vincula cliente + plan + PPPoE secret)
4. **Configura la facturación** (plantilla de ciclo de facturación)
5. **Genera la primera factura** (opcional)
6. **Habilita el secreto PPPoE** inmediatamente (opcional)

**Request Body - Cliente Nuevo:**
```json
{
  "clienteNombre": "Juan Pérez",
  "clienteCedula": "001-1234567-8",
  "clienteEmail": "juan.perez@email.com",
  "clienteTelefono": "809-555-1234",
  "clienteDireccion": "Calle Principal #123, Santo Domingo",
  "clienteReferenciaPago": "Transferencia Bancaria",
  "organizationId": 1,
  
  "planId": 3,
  "routerId": 1,
  
  "pppSecretName": "cliente_jp_001",
  "pppPassword": "SecureP@ss123!",
  "pppProfile": "Plan-Premium-20MB",
  "pppService": "pppoe",
  
  "diaInicio": 1,
  "diaCutoff": 25,
  "diasGracia": 5,
  "esPrePago": true,
  
  "generarPrimeraFactura": true,
  "periodoFactura": "2026-04",
  "notasSuscripcion": "Cliente nuevo, instalación el 15/04/2026",
  "habilitarSecretInmediatamente": true
}
```

**Request Body - Cliente Existente:**
```json
{
  "clienteCedula": "001-1234567-8",
  "organizationId": 1,
  
  "planId": 3,
  "routerId": 1,
  
  "pppSecretName": "cliente_jp_002",
  "pppPassword": "NewP@ss456!",
  "pppProfile": "Plan-Premium-20MB",
  "pppService": "pppoe",
  
  "generarPrimeraFactura": true,
  "habilitarSecretInmediatamente": true
}
```

**Campos del Request (ActivateServiceRequest):**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `clienteNombre` | string | ⚠️ | Nombre del cliente (requerido si es cliente nuevo) |
| `clienteCedula` | string | ⚠️ | Cédula/RNC (requerido si es cliente nuevo) |
| `clienteEmail` | string | ⚠️ | Email (requerido si es cliente nuevo) |
| `clienteTelefono` | string | ❌ | Teléfono |
| `clienteDireccion` | string | ⚠️ | Dirección (requerido si es cliente nuevo) |
| `clienteReferenciaPago` | string | ❌ | Referencia de pago |
| `organizationId` | int | ✅ | ID de la organización |
| `planId` | int | ✅ | ID del plan contratado |
| `routerId` | int | ✅ | ID del router donde se creará el PPPoE secret |
| `pppSecretName` | string | ✅ | Nombre de usuario PPPoE (debe ser único en el router) |
| `pppPassword` | string | ✅ | Contraseña PPPoE |
| `pppProfile` | string | ✅ | Nombre del perfil PPPoE en MikroTik |
| `pppService` | string | ❌ | Tipo de servicio (default: "pppoe") |
| `diaInicio` | int | ❌ | Día de inicio del ciclo (default: 1) |
| `diaCutoff` | int | ❌ | Día de corte (default: 25) |
| `diasGracia` | int | ❌ | Días de gracia (default: 5) |
| `esPrePago` | bool | ❌ | ¿Es pre-pago? (default: true) |
| `generarPrimeraFactura` | bool | ❌ | ¿Generar primera factura? (default: true) |
| `periodoFactura` | string | ❌ | Período de la factura (formato YYYY-MM) |
| `notasSuscripcion` | string | ❌ | Notas sobre la suscripción |
| `habilitarSecretInmediatamente` | bool | ❌ | ¿Habilitar PPPoE secret inmediatamente? (default: true) |

**Response Exitosa - Cliente Nuevo (200 OK):**
```json
{
  "status": "Success",
  "message": "Cliente creado y servicio activado exitosamente",
  "data": {
    "clienteId": 10,
    "suscripcionId": 25,
    "pppSecretId": "*2B",
    "pppSecretName": "cliente_jp_001",
    "facturaId": 100,
    "numeroFactura": "B31202604000001",
    "clienteExistia": false,
    "message": "Cliente creado y servicio activado exitosamente"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response Exitosa - Cliente Existente (200 OK):**
```json
{
  "status": "Success",
  "message": "Servicio activado exitosamente para cliente existente",
  "data": {
    "clienteId": 5,
    "suscripcionId": 26,
    "pppSecretId": "*3C",
    "pppSecretName": "cliente_jp_002",
    "facturaId": 101,
    "numeroFactura": "B31202604000002",
    "clienteExistia": true,
    "message": "Servicio activado exitosamente para cliente existente"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Response de Error - Datos Incompletos (400 Bad Request):**
```json
{
  "status": "ValidationError",
  "message": "Datos de cliente incompletos. Se requiere nombre, cédula y email para crear un nuevo cliente.",
  "errors": {
    "clienteNombre": "Requerido",
    "clienteCedula": "Requerido",
    "clienteEmail": "Requerido"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Notas Importantes:**
- Si el cliente ya existe (por cédula o email), se reutiliza
- Si el secreto PPPoE ya existe en MikroTik, fallará
- La primera factura se genera automáticamente si `generarPrimeraFactura = true`
- El secreto PPPoE se crea habilitado o deshabilitado según `habilitarSecretInmediatamente`

---

### 3.2 Cambio de Plan

#### 3.2.1 Cambiar Plan de Suscripción

**Endpoint:** `POST /api/sales/subscriptions/{subscriptionId}/change-plan`

**Descripción:** Cambia el plan de una suscripción activa. Calcula automáticamente el prorrateo si se solicita.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `subscriptionId` | int | ✅ | ID de la suscripción |

**Request Body:**
```json
{
  "nuevoPlanId": 4,
  "aplicarProrrateo": true,
  "motivoCambio": "Cliente solicita mayor velocidad",
  "fechaEfectiva": "2026-04-20T00:00:00"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `nuevoPlanId` | int | ✅ | ID del nuevo plan |
| `aplicarProrrateo` | bool | ❌ | ¿Calcular prorrateo? (default: true) |
| `motivoCambio` | string | ❌ | Motivo del cambio |
| `fechaEfectiva` | DateTime | ❌ | Fecha cuando el cambio será efectivo (default: ahora) |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Plan cambiado exitosamente",
  "data": {
    "suscripcionId": 25,
    "planAnteriorId": 3,
    "planAnteriorNombre": "Plan Premium 20MB",
    "planNuevoId": 4,
    "planNuevoNombre": "Plan Ultra 50MB",
    "montoProrrateo": 433.33,
    "fechaEfectiva": "2026-04-20T00:00:00",
    "message": "Plan cambiado exitosamente. Prorrateo: RD$ 433.33"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Cálculo de Prorrateo:**
```
Días restantes en el mes = DíasEnMes - DíaActual
Crédito = (PrecioPlanAnterior / DíasEnMes) * DíasRestantes
CargoNuevo = (PrecioNuevoPlan / DíasEnMes) * DíasRestantes
Prorrateo = CargoNuevo - Crédito
```

---

### 3.3 Suspensión de Servicio

#### 3.3.1 Suspender Servicio

**Endpoint:** `POST /api/sales/subscriptions/{subscriptionId}/suspend`

**Descripción:** Suspende un servicio activo. Actualiza el estado de la suscripción y deshabilita el secreto PPPoE en MikroTik. Opcionalmente desconecta al cliente inmediatamente.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `subscriptionId` | int | ✅ | ID de la suscripción |

**Request Body:**
```json
{
  "motivo": "Falta de pago - factura vencida hace 15 días",
  "desconectarInmediatamente": true
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `motivo` | string | ✅ | Motivo de la suspensión |
| `desconectarInmediatamente` | bool | ❌ | ¿Desconectar cliente activo ahora? (default: false) |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Servicio suspendido exitosamente",
  "data": true,
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Acciones que realiza:**
1. Actualiza estado de suscripción a `Suspendida`
2. Deshabilita el secreto PPPoE en MikroTik (si existe)
3. Si `desconectarInmediatamente = true`, elimina la conexión activa del cliente
4. Agrega registro en notas de la suscripción con fecha y motivo

---

### 3.4 Reactivación de Servicio

#### 3.4.1 Reactivar Servicio Suspendido

**Endpoint:** `POST /api/sales/subscriptions/{subscriptionId}/reactivate`

**Descripción:** Reactiva un servicio que estaba suspendido. Habilita el secreto PPPoE en MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `subscriptionId` | int | ✅ | ID de la suscripción |

**Request Body:**
```json
{
  "motivo": "Pago recibido - cliente al día",
  "habilitarSecretInmediatamente": true
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `motivo` | string | ✅ | Motivo de la reactivación |
| `habilitarSecretInmediatamente` | bool | ❌ | ¿Habilitar PPPoE secret inmediatamente? (default: true) |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Servicio reactivado exitosamente",
  "data": true,
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Acciones que realiza:**
1. Actualiza estado de suscripción a `Activa`
2. Habilita el secreto PPPoE en MikroTik (si `habilitarSecretInmediatamente = true`)
3. Agrega registro en notas de la suscripción con fecha y motivo

---

## 4. Módulo PPPoE MikroTik

**Base Path:** `/api/mikrotik`

### 4.1 Perfiles PPPoE

#### 4.1.1 Obtener Perfiles PPPoE (Paginado)

**Endpoint:** `GET /api/mikrotik/ppp/profiles/{routerId}`

**Descripción:** Obtiene todos los perfiles PPPoE configurados en un router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Query Parameters:**

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Número de página |
| `pageSize` | int | 20 | Registros por página |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Perfiles PPPoE obtenidos exitosamente",
  "data": {
    "items": [
      {
        "id": "*1A",
        "name": "Plan-Premium-20MB",
        "localAddress": "10.0.0.1",
        "remoteAddress": "pppoe-pool",
        "dnsServers": "8.8.8.8, 8.8.4.4",
        "rateLimit": "20M/20M",
        "onlyOne": "default",
        "comment": "Perfil para plan premium de 20Mbps",
        "syncState": "synced"
      }
    ],
    "totalRecords": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.1.2 Crear Perfil PPPoE

**Endpoint:** `POST /api/mikrotik/ppp/profile/{routerId}`

**Descripción:** Crea un nuevo perfil PPPoE en el router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "name": "Plan-Premium-20MB",
  "localAddress": "10.0.0.1",
  "remoteAddress": "pppoe-pool",
  "dnsServers": "8.8.8.8, 8.8.4.4",
  "rateLimit": "20M/20M",
  "onlyOne": "default",
  "comment": "Perfil para plan premium de 20Mbps"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `name` | string | ✅ | Nombre del perfil |
| `localAddress` | string | ✅ | Dirección IP local del router |
| `remoteAddress` | string | ✅ | Pool de direcciones remotas o IP |
| `dnsServers` | string | ❌ | Servidores DNS separados por coma |
| `rateLimit` | string | ❌ | Límite de velocidad (formato: "download/upload") |
| `onlyOne` | string | ❌ | Permitir una sola sesión ("yes", "no", "default") |
| `comment` | string | ❌ | Comentario/descripción |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Perfil PPPoE creado exitosamente",
  "data": {
    "id": "*1A",
    "name": "Plan-Premium-20MB",
    "localAddress": "10.0.0.1",
    "remoteAddress": "pppoe-pool",
    "dnsServers": "8.8.8.8, 8.8.4.4",
    "rateLimit": "20M/20M",
    "onlyOne": "default",
    "comment": "Perfil para plan premium de 20Mbps",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.1.3 Actualizar Perfil PPPoE

**Endpoint:** `PUT /api/mikrotik/ppp/profile/{routerId}`

**Descripción:** Actualiza un perfil PPPoE existente. Solo los campos proporcionados serán actualizados.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*1A",
  "name": "Plan-Premium-25MB",
  "rateLimit": "25M/25M"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `id` | string | ✅ | ID del perfil en MikroTik |
| `name` | string | ❌ | Nuevo nombre del perfil |
| `localAddress` | string | ❌ | Nueva dirección local |
| `remoteAddress` | string | ❌ | Nueva dirección remota |
| `dnsServers` | string | ❌ | Nuevos servidores DNS |
| `rateLimit` | string | ❌ | Nuevo límite de velocidad |
| `onlyOne` | string | ❌ | Nueva configuración de sesión única |
| `comment` | string | ❌ | Nuevo comentario |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Perfil PPPoE actualizado exitosamente",
  "data": {
    "id": "*1A",
    "name": "Plan-Premium-25MB",
    "localAddress": "10.0.0.1",
    "remoteAddress": "pppoe-pool",
    "dnsServers": "8.8.8.8, 8.8.4.4",
    "rateLimit": "25M/25M",
    "onlyOne": "default",
    "comment": "Perfil para plan premium de 25Mbps",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.1.4 Eliminar Perfil PPPoE

**Endpoint:** `DELETE /api/mikrotik/ppp/profile/{routerId}`

**Descripción:** Elimina un perfil PPPoE del router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*1A"
}
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Perfil PPPoE eliminado exitosamente",
  "data": {
    "id": "*1A",
    "name": "Plan-Premium-25MB",
    "localAddress": "10.0.0.1",
    "remoteAddress": "pppoe-pool",
    "dnsServers": "8.8.8.8, 8.8.4.4",
    "rateLimit": "25M/25M",
    "onlyOne": "default",
    "comment": "Perfil para plan premium de 25Mbps",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 4.2 Secretos PPPoE (Credenciales)

#### 4.2.1 Obtener Secretos PPPoE (Paginado)

**Endpoint:** `GET /api/mikrotik/ppp/secrets/{routerId}`

**Descripción:** Obtiene todos los secretos PPPoE (credenciales de clientes) configurados en un router.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Query Parameters:**

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Número de página |
| `pageSize` | int | 20 | Registros por página |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Secretos PPPoE obtenidos exitosamente",
  "data": {
    "items": [
      {
        "id": "*2B",
        "name": "cliente_jp_001",
        "service": "pppoe",
        "profile": "Plan-Premium-20MB",
        "disabled": false,
        "comment": "Juan Pérez - Plan Premium",
        "syncState": "synced"
      }
    ],
    "totalRecords": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.2.2 Crear Secreto PPPoE

**Endpoint:** `POST /api/mikrotik/ppp/secret/{routerId}`

**Descripción:** Crea un nuevo secreto PPPoE (credenciales de cliente) en el router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "name": "cliente_jp_001",
  "password": "SecureP@ss123!",
  "service": "pppoe",
  "profile": "Plan-Premium-20MB",
  "disabled": false,
  "comment": "Juan Pérez - Plan Premium"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `name` | string | ✅ | Nombre de usuario (credencial) |
| `password` | string | ✅ | Contraseña |
| `service` | string | ❌ | Tipo de servicio (default: "pppoe") |
| `profile` | string | ✅ | Perfil PPPoE asignado |
| `disabled` | bool | ❌ | Indica si está deshabilitado (default: false) |
| `comment` | string | ❌ | Comentario/descripción |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Secreto PPPoE creado exitosamente",
  "data": {
    "id": "*2B",
    "name": "cliente_jp_001",
    "service": "pppoe",
    "profile": "Plan-Premium-20MB",
    "disabled": false,
    "comment": "Juan Pérez - Plan Premium",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.2.3 Actualizar Secreto PPPoE

**Endpoint:** `PUT /api/mikrotik/ppp/secret/{routerId}`

**Descripción:** Actualiza un secreto PPPoE existente.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*2B",
  "password": "NewP@ss456!",
  "disabled": true,
  "comment": "Juan Pérez - Servicio suspendido"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `id` | string | ✅ | ID del secreto en MikroTik |
| `name` | string | ❌ | Nuevo nombre de usuario |
| `password` | string | ❌ | Nueva contraseña |
| `service` | string | ❌ | Nuevo tipo de servicio |
| `profile` | string | ❌ | Nuevo perfil asignado |
| `disabled` | bool | ❌ | Nuevo estado de habilitación |
| `comment` | string | ❌ | Nuevo comentario |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Secreto PPPoE actualizado exitosamente",
  "data": {
    "id": "*2B",
    "name": "cliente_jp_001",
    "service": "pppoe",
    "profile": "Plan-Premium-20MB",
    "disabled": true,
    "comment": "Juan Pérez - Servicio suspendido",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.2.4 Eliminar Secreto PPPoE

**Endpoint:** `DELETE /api/mikrotik/ppp/secret/{routerId}`

**Descripción:** Elimina un secreto PPPoE del router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*2B"
}
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Secreto PPPoE eliminado exitosamente",
  "data": {
    "id": "*2B",
    "name": "cliente_jp_001",
    "service": "pppoe",
    "profile": "Plan-Premium-20MB",
    "disabled": false,
    "comment": "Juan Pérez - Plan Premium",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 4.3 Servidores PPPoE

#### 4.3.1 Obtener Servidores PPPoE (Paginado)

**Endpoint:** `GET /api/mikrotik/ppp/servers/{routerId}`

**Descripción:** Obtiene todos los servidores PPPoE configurados en un router.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Query Parameters:**

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Número de página |
| `pageSize` | int | 20 | Registros por página |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Servidores PPPoE obtenidos exitosamente",
  "data": {
    "items": [
      {
        "id": "*3C",
        "name": "PPPoE-Server-Main",
        "interface": "ether2",
        "profile": "default",
        "maxMTU": "1480",
        "maxMRU": "1480",
        "keepAliveTimeOut": "10",
        "oneSessionPerHost": "yes",
        "disabled": false,
        "comment": "Servidor PPPoE principal",
        "syncState": "synced"
      }
    ],
    "totalRecords": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.3.2 Crear Servidor PPPoE

**Endpoint:** `POST /api/mikrotik/ppp/server/{routerId}`

**Descripción:** Crea un nuevo servidor PPPoE en el router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "name": "PPPoE-Server-Main",
  "interface": "ether2",
  "profile": "default",
  "maxMTU": "1480",
  "maxMRU": "1480",
  "keepAliveTimeOut": "10",
  "oneSessionPerHost": "yes",
  "comment": "Servidor PPPoE principal"
}
```

**Campos del Request:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `name` | string | ✅ | Nombre del servidor |
| `interface` | string | ✅ | Interfaz de red donde escuchar |
| `profile` | string | ❌ | Perfil por defecto (default: "default") |
| `maxMTU` | string | ❌ | MTU máximo (default: "1480") |
| `maxMRU` | string | ❌ | MRU máximo (default: "1480") |
| `keepAliveTimeOut` | string | ❌ | Timeout de keep-alive en segundos (default: "10") |
| `oneSessionPerHost` | string | ❌ | Una sesión por host ("yes", "no") |
| `comment` | string | ❌ | Comentario/descripción |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Servidor PPPoE creado exitosamente",
  "data": {
    "id": "*3C",
    "name": "PPPoE-Server-Main",
    "interface": "ether2",
    "profile": "default",
    "maxMTU": "1480",
    "maxMRU": "1480",
    "keepAliveTimeOut": "10",
    "oneSessionPerHost": "yes",
    "disabled": false,
    "comment": "Servidor PPPoE principal",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.3.3 Actualizar Servidor PPPoE

**Endpoint:** `PUT /api/mikrotik/ppp/server/{routerId}`

**Descripción:** Actualiza un servidor PPPoE existente.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*3C",
  "keepAliveTimeOut": "15",
  "comment": "Servidor PPPoE principal - actualizado"
}
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Servidor PPPoE actualizado exitosamente",
  "data": {
    "id": "*3C",
    "name": "PPPoE-Server-Main",
    "interface": "ether2",
    "profile": "default",
    "maxMTU": "1480",
    "maxMRU": "1480",
    "keepAliveTimeOut": "15",
    "oneSessionPerHost": "yes",
    "disabled": false,
    "comment": "Servidor PPPoE principal - actualizado",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

#### 4.3.4 Eliminar Servidor PPPoE

**Endpoint:** `DELETE /api/mikrotik/ppp/server/{routerId}`

**Descripción:** Elimina un servidor PPPoE del router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*3C"
}
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Servidor PPPoE eliminado exitosamente",
  "data": {
    "id": "*3C",
    "name": "PPPoE-Server-Main",
    "interface": "ether2",
    "profile": "default",
    "maxMTU": "1480",
    "maxMRU": "1480",
    "keepAliveTimeOut": "10",
    "oneSessionPerHost": "yes",
    "disabled": false,
    "comment": "Servidor PPPoE principal",
    "syncState": "synced"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

### 4.4 Conexiones PPPoE Activas

#### 4.4.1 Obtener Conexiones PPPoE Activas

**Endpoint:** `GET /api/mikrotik/ppp/connections/{routerId}`

**Descripción:** Obtiene todas las conexiones PPPoE activas en un router MikroTik.

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Query Parameters:**

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `pageNumber` | int | 1 | Número de página |
| `pageSize` | int | 20 | Registros por página |

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Conexiones PPPoE activas obtenidas exitosamente",
  "data": {
    "items": [
      {
        "id": "*1A2B",
        "name": "cliente_jp_001",
        "service": "pppoe",
        "callerId": "AA:BB:CC:DD:EE:FF",
        "address": "192.168.100.50",
        "uptime": "2d 15h 30m",
        "tieneClienteAsociado": true,
        "clienteId": 1,
        "clienteNombre": "Juan Pérez",
        "planNombre": "Plan Premium 20MB",
        "planVelocidadMbps": 20,
        "estadoSuscripcion": "Activa"
      }
    ],
    "totalRecords": 1,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Campos de PPPoEActiveConnectionResponse:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | string | ID de conexión en MikroTik |
| `name` | string | Nombre del secreto PPPoE |
| `service` | string | Tipo de servicio |
| `callerId` | string | MAC address del cliente |
| `address` | string | IP asignada |
| `uptime` | string | Tiempo conectado |
| `tieneClienteAsociado` | bool | ¿Tiene cliente comercial asociado? |
| `clienteId` | int? | ID del cliente comercial |
| `clienteNombre` | string? | Nombre del cliente |
| `planNombre` | string? | Nombre del plan contratado |
| `planVelocidadMbps` | int? | Velocidad del plan en Mbps |
| `estadoSuscripcion` | string? | Estado de suscripción comercial |

---

#### 4.4.2 Eliminar Conexión PPPoE Activa

**Endpoint:** `DELETE /api/mikrotik/ppp/connection/{routerId}`

**Descripción:** Desconecta forzosamente una conexión PPPoE activa (kick client).

**Path Parameters:**

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `routerId` | int | ✅ | ID del router |

**Request Body:**
```json
{
  "id": "*1A2B"
}
```

**Response Exitosa (200 OK):**
```json
{
  "status": "Success",
  "message": "Conexión PPPoE eliminada exitosamente",
  "data": {
    "id": "*1A2B",
    "name": "cliente_jp_001",
    "service": "pppoe",
    "callerId": "AA:BB:CC:DD:EE:FF",
    "address": "192.168.100.50",
    "uptime": "2d 15h 30m",
    "tieneClienteAsociado": true,
    "clienteId": 1,
    "clienteNombre": "Juan Pérez",
    "planNombre": "Plan Premium 20MB",
    "planVelocidadMbps": 20,
    "estadoSuscripcion": "Activa"
  },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

---

## 3. Módulo de Ventas (Sales) - PENDIENTE

⚠️ **Este módulo aún no ha sido implementado. Documentación de planificación.**

### 3.1 Flujo de Venta Completo (Activación de Servicio)

**Endpoint Planificado:** `POST /api/sales/activate`

**Descripción:** Flujo completo de activación de un nuevo servicio de internet para un cliente. Este endpoint orquestará múltiples operaciones:
1. Crear cliente (si no existe)
2. Crear secreto PPPoE en MikroTik
3. Crear suscripción
4. Configurar plantilla de facturación
5. Generar primera factura
6. (Opcional) Habilitar secreto PPPoE en MikroTik

### 3.2 Catálogo de Planes

**Endpoints Planificados:**
- `GET /api/sales/plans/router/{routerId}` - Obtener planes disponibles de un router
- `POST /api/sales/plans` - Crear nuevo plan comercial
- `PUT /api/sales/plans/{planId}` - Actualizar plan
- `DELETE /api/sales/plans/{planId}` - Desactivar plan

### 3.3 Paquetes Comerciales

**Endpoints Planificados:**
- `GET /api/sales/packages` - Obtener paquetes comerciales disponibles
- `POST /api/sales/packages` - Crear paquete comercial
- `PUT /api/sales/packages/{packageId}` - Actualizar paquete

### 3.4 Upgrade/Downgrade de Plan

**Endpoint Planificado:** `POST /api/sales/subscriptions/{subscriptionId}/change-plan`

**Descripción:** Cambia el plan de una suscripción activa con cálculo de prorrateo.

---

## 5. Modelos de Respuesta Estándar

Todos los endpoints devuelven respuestas en el formato `ApiResponse<T>`:

### 5.1 Estructura de ApiResponse

```json
{
  "status": "Success",
  "message": "Descripción del resultado",
  "data": { ... },
  "errors": null,
  "pagination": { ... },
  "timestamp": "2026-04-15T10:30:00Z"
}
```

**Campos:**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `status` | string | Estado de la operación (ver valores posibles abajo) |
| `message` | string | Mensaje descriptivo del resultado |
| `data` | T | Datos de la respuesta (puede ser null) |
| `errors` | object | Detalles de errores (solo en caso de error) |
| `pagination` | object | Metadata de paginación (solo en respuestas paginadas) |
| `timestamp` | DateTime | Marca de tiempo de la respuesta (UTC) |

### 5.2 Valores de Status

| Status | HTTP Code | Descripción |
|--------|-----------|-------------|
| `Success` | 200 | Operación exitosa |
| `Warning` | 200 | Operación exitosa con advertencias |
| `ValidationError` | 400 | Error de validación en los datos de entrada |
| `NotFound` | 404 | Recurso no encontrado |
| `Unauthorized` | 401 | No autenticado |
| `Forbidden` | 403 | Sin permisos para acceder |
| `Error` | 500 | Error interno del servidor |

### 5.3 PaginationMetadata

```json
{
  "currentPage": 1,
  "pageSize": 20,
  "totalRecords": 100,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true
}
```

---

## 6. Catálogo de Errores

### 6.1 Errores de Validación Comunes

| Error | Causa | Solución |
|-------|-------|----------|
| "Ya existe un cliente con la cédula especificada" | La cédula/RNC ya está registrada en la organización | Usar una cédula diferente o buscar el cliente existente |
| "Ya existe un cliente con el email especificado" | El email ya está registrado en la organización | Usar un email diferente o buscar el cliente existente |
| "No se encontró una suscripción activa para el cliente" | El cliente no tiene suscripción activa | Crear una nueva suscripción |
| "No se puede cancelar una factura que ya está completamente pagada" | La factura tiene MontoPagado >= Total | No se puede cancelar; si es necesario, registrar un ajuste |
| "El router no está disponible" | El router MikroTik no responde o está desconectado | Verificar conexión de red y credenciales del router |
| "Secreto PPPoE no encontrado en el router" | El ID de secreto no existe en MikroTik | Verificar el ID o crear un nuevo secreto |

### 6.2 Errores de Conexión MikroTik

| Error | Causa | Solución |
|-------|-------|----------|
| "Connection timeout" | El router no responde en el tiempo esperado | Verificar conectividad de red, firewall |
| "Authentication failed" | Credenciales de API incorrectas | Verificar usuario/contraseña de API en MikroTik |
| "Router not found" | El ID de router no existe en la base de datos | Verificar que el router esté registrado |

---

## 7. Guía de Integración PPPoE

### 7.1 Flujo Completo de Activación de Nuevo Cliente

```
┌─────────────────────────────────────────────────────────────┐
│                     FLUJO DE ACTIVACIÓN                      │
└─────────────────────────────────────────────────────────────┘

1. CREAR CLIENTE
   POST /api/billing/clientes
   Body: { nombre, cedula, email, telefono, direccion, organizationId }
   Response: { id: 1, ... }

2. CREAR SECRETO PPPoE EN MIKROTIK
   POST /api/mikrotik/ppp/secret/{routerId}
   Body: { name: "cliente_jp_001", password, profile, ... }
   Response: { id: "*2B", ... }

3. CREAR SUSCRIPCIÓN
   POST /api/billing/subscriptions
   Body: { clienteId: 1, planId: 3, pppSecretId: 5 }
   Response: { id: 10, ... }

4. CONFIGURAR FACTURACIÓN
   POST /api/billing/templates
   Body: { clienteId: 1, diaInicio: 1, diaCutoff: 25, ... }
   Response: true

5. GENERAR PRIMERA FACTURA
   POST /api/billing/invoices/generate
   Body: { clienteId: 1, periodo: "2026-04" }
   Response: { id: 100, numeroFactura: "B31202604000001", ... }

6. VERIFICAR CONEXIÓN ACTIVA
   GET /api/billing/connections/router/{routerId}
   Response: { items: [{ tieneClienteAsociado: true, clienteId: 1, ... }] }
```

### 7.2 Flujo de Suspensión por Falta de Pago

```
┌─────────────────────────────────────────────────────────────┐
│                  FLUJO DE SUSPENSIÓN (Manual)                │
└─────────────────────────────────────────────────────────────┘

1. DETECTAR FACTURAS VENCIDAS
   GET /api/billing/invoices/cliente/{clienteId}
   Buscar facturas con estado = 4 (Vencida) o fechaVencimiento < hoy

2. DESHABILITAR SECRETO PPPoE
   PUT /api/mikrotik/ppp/secret/{routerId}
   Body: { id: "*2B", disabled: true, comment: "Suspendido por falta de pago" }

3. (OPCIONAL) DESCONECTAR CLIENTE ACTIVO
   DELETE /api/mikrotik/ppp/connection/{routerId}
   Body: { id: "*1A2B" }

4. ACTUALIZAR ESTADO DE SUSCRIPCIÓN
   (PENDIENTE - requiere endpoint de actualización de suscripción)
```

### 7.3 Flujo de Registro de Pago

```
┌─────────────────────────────────────────────────────────────┐
│                    FLUJO DE REGISTRO DE PAGO                 │
└─────────────────────────────────────────────────────────────┘

1. REGISTRAR PAGO
   POST /api/billing/payments
   Body: { clienteId: 1, monto: 1770.00, referencia: "REF-001", ... }
   
   SISTEMA AUTOMÁTICAMENTE:
   - Distribuye pago en facturas pendientes (más antiguas primero)
   - Actualiza estado de facturas:
     * MontoPagado >= Total → "Pagada"
     * MontoPagado > 0 && < Total → "Abonada"

2. VERIFICAR ESTADO DE FACTURAS
   GET /api/billing/invoices/cliente/{clienteId}
   Response: facturas con estados actualizados

3. GENERAR RECIBO
   GET /api/billing/payments/{paymentId}/receipt
   Response: PDF del recibo de pago

4. (OPCIONAL) REHABILITAR SERVICIO SI ESTABA SUSPENDIDO
   PUT /api/mikrotik/ppp/secret/{routerId}
   Body: { id: "*2B", disabled: false }
```

### 7.4 Relación entre Entidades PPPoE y Comercial

```
MikroTik Router
    │
    ├── PPPoE Profile (perfil de velocidad/DNS)
    │       │
    │       └── PPPoE Secret (credenciales: username/password)
    │               │
    │               └── PPPoE Active Connection (conexión en vivo)
    │
    └── Base de datos MikroClean
            │
            ├── Plan (define velocidad y precio)
            │       │
            │       └── Subscription (vincula Cliente + Plan + PPPoE Secret)
            │               │
            │               └── Cliente (datos de facturación)
            │                       │
            │                       ├── Invoices (facturas)
            │                       │       │
            │                       │       └── InvoiceDetails (líneas)
            │                       │
            │                       └── Payments (pagos registrados)
            │                               │
            │                               └── PaymentInvoiceMapping (aplicación)
            │
            └── BillingTemplate (configuración de ciclo de facturación)
```

### 7.5 Ejemplo de Script de Integración (cURL)

```bash
#!/bin/bash
# Ejemplo de flujo completo de activación de cliente

BASE_URL="http://localhost:5000/api"
ORGANIZATION_ID=1
ROUTER_ID=1

# 1. Crear cliente
echo "Creando cliente..."
CLIENTE_RESPONSE=$(curl -s -X POST "$BASE_URL/billing/clientes" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Juan Pérez",
    "cedula": "001-1234567-8",
    "email": "juan.perez@email.com",
    "telefono": "809-555-1234",
    "direccion": "Calle Principal #123",
    "organizationId": 1
  }')
CLIENTE_ID=$(echo $CLIENTE_RESPONSE | jq -r '.data.id')
echo "Cliente creado con ID: $CLIENTE_ID"

# 2. Crear secreto PPPoE
echo "Creando secreto PPPoE..."
SECRET_RESPONSE=$(curl -s -X POST "$BASE_URL/mikrotik/ppp/secret/$ROUTER_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "cliente_jp_001",
    "password": "SecureP@ss123!",
    "service": "pppoe",
    "profile": "Plan-Premium-20MB"
  }')
echo "Secreto creado: $SECRET_RESPONSE"

# 3. Crear suscripción (asumiendo planId=3, pppSecretId=5)
echo "Creando suscripción..."
SUBSCRIPTION_RESPONSE=$(curl -s -X POST "$BASE_URL/billing/subscriptions" \
  -H "Content-Type: application/json" \
  -d "{
    \"clienteId\": $CLIENTE_ID,
    \"planId\": 3,
    \"pppSecretId\": 5
  }")
echo "Suscripción creada: $SUBSCRIPTION_RESPONSE"

# 4. Configurar facturación
echo "Configurando facturación..."
curl -s -X POST "$BASE_URL/billing/templates" \
  -H "Content-Type: application/json" \
  -d "{
    \"clienteId\": $CLIENTE_ID,
    \"diaInicio\": 1,
    \"diaCutoff\": 25,
    \"diasGracia\": 5,
    \"tipoCiclo\": 0,
    \"esPrePago\": true
  }"

# 5. Generar primera factura
echo "Generando primera factura..."
INVOICE_RESPONSE=$(curl -s -X POST "$BASE_URL/billing/invoices/generate" \
  -H "Content-Type: application/json" \
  -d "{
    \"clienteId\": $CLIENTE_ID,
    \"periodo\": \"2026-04\"
  }")
INVOICE_ID=$(echo $INVOICE_RESPONSE | jq -r '.data.id')
echo "Factura generada con ID: $INVOICE_ID"

# 6. Descargar PDF de factura
echo "Descargando PDF de factura..."
curl -s -o "factura-$INVOICE_ID.pdf" \
  "$BASE_URL/billing/invoices/$INVOICE_ID/pdf"
echo "PDF descargado: factura-$INVOICE_ID.pdf"

echo "¡Flujo de activación completado!"
```

### 7.6 Consideraciones de Seguridad

1. **Autenticación:** Todos los endpoints deben protegerse con JWT Bearer Token
2. **Autorización:** Los operadores solo deben acceder a routers de su organización
3. **Rate Limiting:** Implementar límites de tasa para prevenir abuso de API
4. **Auditoría:** Registrar todas las operaciones de creación/eliminación
5. **Validación:** Validar todos los inputs del usuario en el backend

### 7.7 Códigos de Velocidad PPPoE (RateLimit)

El formato de `RateLimit` en perfiles PPPoE es: `download/upload`

| Valor | Descripción |
|-------|-------------|
| `10M/10M` | 10 Mbps download / 10 Mbps upload |
| `20M/20M` | 20 Mbps download / 20 Mbps upload |
| `50M/25M` | 50 Mbps download / 25 Mbps upload |
| `100M/50M` | 100 Mbps download / 50 Mbps upload |

Puedes agregar burst permit con formato: `rate-limit@burst-time:burst-threshold:burst-limit`
Ejemplo: `20M/20M 25M@32s:64k:25M` (burst a 25Mbps por 32 segundos)

---

## Apéndice A: Configuración de Ambiente de Desarrollo

### Requisitos
- .NET 8.0 SDK
- SQL Server 2019+ (local o en contenedor Docker)
- Router MikroTik (físico o CHR en VM) accesible desde el servidor

### Variables de Entorno (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MikroCleanDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;"
  },
  "Jwt": {
    "Issuer": "MikroCleanAPI",
    "Audience": "MikroCleanClient",
    "Key": "TuClaveSecretaDeAlMenos32Caracteres!",
    "ExpirationInHours": 24
  },
  "MikroTik": {
    "PendingChangesProcessor": {
      "IntervalSeconds": 30,
      "LogLevel": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Ejecución

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar (desarrollo)
dotnet run --project MikroClean.WebAPI

# O ejecutar directamente
cd MikroClean.WebAPI
dotnet run
```

La API estará disponible en: `http://localhost:5000`

### Swagger UI

En entorno de desarrollo, Swagger está disponible en: `http://localhost:5000/swagger`

---

## Apéndice B: Migraciones de Base de Datos

Las migraciones se aplican automáticamente al iniciar la API. La migración principal de facturación es:

**Migración:** `20260328111843_AddBillingTables.cs`

**Tablas creadas:**
- `Clientes`
- `Planes`
- `Subscripciones`
- `BillingTemplates`
- `Invoices`
- `InvoiceDetails`
- `Payments`
- `PaymentInvoiceMappings`
- `Taxes`
- `FiscalVouchers`

**Datos iniciales:**
- Tax ITBIS 18% seed

---

**Documento generado:** miércoles, 15 de abril de 2026  
**Versión:** 1.0  
**Proyecto:** MikroClean - MikroTIk Router API  
**Contacto:** Equipo de desarrollo
