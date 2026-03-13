# 🎯 Guía Completa de Uso - Sistema MikroTik MikroClean

## 📚 Tabla de Contenidos

1. [Configuración Inicial](#configuración-inicial)
2. [Gestión de Routers](#gestión-de-routers)
3. [Operaciones MikroTik](#operaciones-mikrotik)
4. [Escenarios Comunes](#escenarios-comunes)
5. [Operaciones Batch](#operaciones-batch)
6. [Troubleshooting](#troubleshooting)

---

## 🔧 Configuración Inicial

### 1. Configurar appsettings.Development.json

```json
{
  "Encryption": {
    "SecretKey": "MikroClean-Dev-Secret-2024-min32chars"
  },
  "MikroTik": {
    "ConnectionTimeout": 10,
    "CommandTimeout": 30,
    "MaxConnectionsPerOrganization": 20,
    "RetryPolicy": {
      "MaxRetryAttempts": 3,
      "InitialDelaySeconds": 1,
      "MaxDelaySeconds": 10,
      "BackoffMultiplier": 2.0
    }
  }
}
```

### 2. Ejecutar Migraciones

```bash
cd MikroClean.Infrastructure
dotnet ef database update
```

### 3. Instalar Paquetes NuGet

```bash
cd MikroClean.Infrastructure
dotnet add package tik4net --version 3.5.0
dotnet add package Polly --version 8.2.0
dotnet add package Microsoft.Extensions.Caching.Memory --version 8.0.1

cd ../MikroClean.Application
dotnet add package Microsoft.Extensions.Logging.Abstractions --version 8.0.0

dotnet restore
dotnet build
```

---

## 🖥️ Gestión de Routers

### 1. Crear Router (Password se encripta automáticamente)

```http
POST /api/routers
Content-Type: application/json

{
  "name": "Router Sucursal Norte",
  "ip": "192.168.10.1",
  "user": "admin",
  "password": "MikroTikAdmin2024!",
  "model": "RB750Gr3",
  "location": "Sucursal Norte - Rack A1",
  "organizationId": 1
}
```

**Respuesta Exitosa:**
```json
{
  "status": "success",
  "message": "Router creado exitosamente",
  "data": {
    "id": 1,
    "name": "Router Sucursal Norte",
    "ip": "192.168.10.1",
    "user": "admin",
    "isActive": true,
    "model": "RB750Gr3",
    "location": "Sucursal Norte - Rack A1",
    "organizationId": 1,
    "organizationName": "Mi Empresa S.A.",
    "createdAt": "2024-01-15T10:00:00Z"
  },
  "timestamp": "2024-01-15T10:00:05Z"
}
```

> 🔐 El password `"MikroTikAdmin2024!"` se guarda encriptado en DB

---

### 2. Listar Routers de una Organización

```http
GET /api/routers/organization/1
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Se encontraron 5 routers",
  "data": [
    {
      "id": 1,
      "name": "Router Sucursal Norte",
      "ip": "192.168.10.1",
      "user": "admin",
      "isActive": true,
      "lastSeen": "2024-01-15T09:45:00Z",
      "organizationId": 1
    },
    {
      "id": 2,
      "name": "Router Sucursal Sur",
      "ip": "192.168.20.1",
      "user": "admin",
      "isActive": true,
      "lastSeen": "2024-01-15T09:50:00Z",
      "organizationId": 1
    }
  ]
}
```

---

### 3. Actualizar Router (Cambiar Password)

```http
PUT /api/routers/1
Content-Type: application/json

{
  "name": "Router Sucursal Norte",
  "ip": "192.168.10.1",
  "user": "admin",
  "password": "NuevoPasswordSeguro2024!",
  "model": "RB750Gr3",
  "location": "Sucursal Norte - Rack A1",
  "isActive": true
}
```

> 🔄 Al cambiar password:
> 1. Se encripta el nuevo password
> 2. Se cierra la conexión activa automáticamente
> 3. La siguiente operación usará el nuevo password

---

### 4. Probar Conexión

```http
POST /api/routers/1/test
```

**Respuesta Exitosa:**
```json
{
  "status": "success",
  "message": "Router conectado exitosamente",
  "data": true
}
```

**Respuesta con Error:**
```json
{
  "status": "error",
  "message": "Error al probar conexión: Authentication failed",
  "data": false
}
```

---

### 5. Eliminar Router (Soft Delete)

```http
DELETE /api/routers/1
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Router eliminado exitosamente",
  "data": true
}
```

---

## 🌐 Operaciones MikroTik

### 1. Crear Interfaz Bridge

```http
POST /api/mikrotik/routers/1/interfaces/bridge
Content-Type: application/json

{
  "name": "bridge-lan",
  "adminMac": false,
  "agingTime": 300,
  "comment": "Bridge principal para LAN",
  "disabled": false
}
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Bridge creado exitosamente",
  "data": {
    "id": "*1",
    "name": "bridge-lan",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "running": true,
    "disabled": false,
    "comment": "Bridge principal para LAN"
  }
}
```

---

### 2. Crear VLAN

```http
POST /api/mikrotik/routers/1/interfaces/vlan
Content-Type: application/json

{
  "name": "vlan-office",
  "vlanId": 100,
  "interface": "bridge-lan",
  "comment": "VLAN para área de oficinas",
  "disabled": false
}
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "VLAN creada exitosamente",
  "data": {
    "id": "*2",
    "name": "vlan-office",
    "vlanId": 100,
    "interface": "bridge-lan",
    "running": true,
    "disabled": false,
    "comment": "VLAN para área de oficinas"
  }
}
```

---

### 3. Agregar Dirección IP

```http
POST /api/mikrotik/routers/1/ip/address
Content-Type: application/json

{
  "address": "192.168.100.1/24",
  "interface": "bridge-lan",
  "comment": "Gateway principal",
  "disabled": false
}
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Dirección IP agregada exitosamente",
  "data": {
    "id": "*3",
    "address": "192.168.100.1/24",
    "network": "192.168.100.0",
    "interface": "bridge-lan",
    "invalid": false,
    "disabled": false,
    "dynamic": false
  }
}
```

---

### 4. Crear Regla de Firewall

```http
POST /api/mikrotik/routers/1/firewall/rules
Content-Type: application/json

{
  "chain": "forward",
  "action": "accept",
  "srcAddress": "192.168.100.0/24",
  "dstAddress": "0.0.0.0/0",
  "protocol": "tcp",
  "dstPort": "80,443",
  "comment": "Permitir HTTP/HTTPS desde LAN",
  "disabled": false
}
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Regla de firewall creada exitosamente",
  "data": {
    "id": "*4",
    "chain": "forward",
    "action": "accept",
    "srcAddress": "192.168.100.0/24",
    "protocol": "tcp",
    "dstPort": "80,443",
    "bytes": 0,
    "packets": 0,
    "disabled": false,
    "comment": "Permitir HTTP/HTTPS desde LAN"
  }
}
```

---

### 5. Obtener Todas las Interfaces

```http
GET /api/mikrotik/routers/1/interfaces
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Se encontraron 12 interfaces",
  "data": [
    {
      "id": "*0",
      "name": "ether1",
      "type": "ether",
      "macAddress": "AA:BB:CC:DD:EE:01",
      "running": true,
      "disabled": false,
      "rxBytes": 1234567890,
      "txBytes": 9876543210,
      "comment": "WAN"
    },
    {
      "id": "*1",
      "name": "bridge-lan",
      "type": "bridge",
      "macAddress": "AA:BB:CC:DD:EE:FF",
      "running": true,
      "disabled": false,
      "rxBytes": 555555555,
      "txBytes": 666666666,
      "comment": "Bridge principal para LAN"
    }
  ]
}
```

---

### 6. Obtener Recursos del Sistema

```http
GET /api/mikrotik/routers/1/system/resources
```

**Respuesta:**
```json
{
  "status": "success",
  "message": "Información del sistema obtenida exitosamente",
  "data": {
    "version": "7.12.1 (stable)",
    "boardName": "RB750Gr3",
    "architecture": "arm",
    "totalMemory": 268435456,
    "freeMemory": 134217728,
    "cpuLoad": 15.5,
    "totalHddSpace": 16777216,
    "freeHddSpace": 8388608,
    "uptime": "7.12:30:45"
  }
}
```

---

## 🎬 Escenarios Comunes

### Escenario 1: Configuración Inicial de Red

```javascript
// 1. Crear router
const router = await createRouter({
  name: "Router Principal",
  ip: "192.168.1.1",
  user: "admin",
  password: "MikroTikPass123!",
  organizationId: 1
});

// 2. Probar conexión
await testRouter(router.id);

// 3. Crear bridge
const bridge = await createBridge(router.id, {
  name: "bridge-lan",
  comment: "Bridge principal"
});

// 4. Agregar IP al bridge
await createIpAddress(router.id, {
  address: "192.168.1.1/24",
  interface: "bridge-lan"
});

// 5. Crear VLAN
await createVlan(router.id, {
  name: "vlan-office",
  vlanId: 100,
  interface: "bridge-lan"
});

// 6. Agregar IP a la VLAN
await createIpAddress(router.id, {
  address: "192.168.100.1/24",
  interface: "vlan-office"
});

// 7. Configurar firewall
await createFirewallRule(router.id, {
  chain: "forward",
  action: "accept",
  inInterface: "vlan-office",
  comment: "Permitir tráfico desde oficina"
});
```

---

### Escenario 2: Monitoreo de Múltiples Routers

```javascript
// 1. Pre-calentar conexiones
const warmup = await warmUpOrganization(organizationId);
// Resultado: { "1": true, "2": true, "3": true }

// 2. Obtener recursos de todos los routers
const routers = await getRoutersByOrganization(organizationId);

for (const router of routers) {
  const resources = await getSystemResources(router.id);
  
  console.log(`Router: ${router.name}`);
  console.log(`  CPU: ${resources.cpuLoad}%`);
  console.log(`  Memoria: ${resources.freeMemory / resources.totalMemory * 100}%`);
  console.log(`  Uptime: ${resources.uptime}`);
}
```

---

### Escenario 3: Configurar Red para Nueva Sucursal

```csharp
public class SucursalSetupService
{
    private readonly IRouterService _routerService;
    private readonly IMikroTikService _mikroTikService;

    public async Task<bool> ConfigurarNuevaSucursalAsync(
        string sucursalName,
        string routerIp,
        string routerPassword,
        int organizationId)
    {
        // 1. Registrar router en el sistema
        var createRouterDto = new CreateRouterDTO
        {
            Name = $"Router {sucursalName}",
            Ip = routerIp,
            User = "admin",
            Password = routerPassword, // Se encripta automáticamente
            Location = sucursalName,
            OrganizationId = organizationId
        };

        var routerResponse = await _routerService.CreateRouterAsync(createRouterDto);
        if (routerResponse.Status != ResponseStatus.Success)
            return false;

        var routerId = routerResponse.Data!.Id;

        // 2. Probar conexión
        var testResponse = await _routerService.TestAndUpdateRouterStatusAsync(routerId);
        if (!testResponse.Data)
            return false;

        // 3. Crear bridge principal
        var bridgeResponse = await _mikroTikService.CreateBridgeAsync(
            routerId,
            new CreateBridgeRequest
            {
                Name = $"bridge-{sucursalName.ToLower()}",
                Comment = $"Bridge para {sucursalName}"
            }
        );

        if (bridgeResponse.Status != ResponseStatus.Success)
            return false;

        var bridgeName = bridgeResponse.Data!.Name;

        // 4. Agregar IP al bridge
        await _mikroTikService.CreateIpAddressAsync(
            routerId,
            new CreateIpAddressRequest
            {
                Address = "10.10.10.1/24",
                Interface = bridgeName,
                Comment = "Gateway principal"
            }
        );

        // 5. Crear VLANs
        var vlans = new[]
        {
            (VlanId: 10, Name: "vlan-admin", Comment: "VLAN Administración"),
            (VlanId: 20, Name: "vlan-users", Comment: "VLAN Usuarios"),
            (VlanId: 30, Name: "vlan-guest", Comment: "VLAN Invitados")
        };

        foreach (var vlan in vlans)
        {
            await _mikroTikService.CreateVlanAsync(
                routerId,
                new CreateVlanRequest
                {
                    Name = vlan.Name,
                    VlanId = vlan.VlanId,
                    Interface = bridgeName,
                    Comment = vlan.Comment
                }
            );
        }

        // 6. Configurar firewall básico
        await ConfigurarFirewallBasicoAsync(routerId);

        return true;
    }

    private async Task ConfigurarFirewallBasicoAsync(int routerId)
    {
        // Bloquear acceso no autorizado
        await _mikroTikService.CreateFirewallRuleAsync(
            routerId,
            new CreateFirewallRuleRequest
            {
                Chain = "input",
                Action = "accept",
                SrcAddress = "192.168.0.0/16",
                Protocol = "tcp",
                DstPort = "8728", // API port
                Comment = "Permitir API desde LAN"
            }
        );

        await _mikroTikService.CreateFirewallRuleAsync(
            routerId,
            new CreateFirewallRuleRequest
            {
                Chain = "input",
                Action = "drop",
                Protocol = "tcp",
                DstPort = "8728",
                Comment = "Bloquear API desde internet"
            }
        );
    }
}
```

---

### Escenario 4: Dashboard de Monitoreo

```csharp
public class MonitoringService
{
    private readonly IRouterService _routerService;
    private readonly IMikroTikService _mikroTikService;

    public async Task<RouterDashboardDTO> GetDashboardDataAsync(int organizationId)
    {
        // 1. Obtener todos los routers
        var routersResponse = await _routerService.GetRoutersByOrganizationAsync(organizationId);
        var routers = routersResponse.Data!;

        var dashboard = new RouterDashboardDTO
        {
            TotalRouters = routers.Count(),
            ActiveRouters = routers.Count(r => r.IsActive),
            ConnectedRouters = 0,
            RoutersDetails = new List<RouterDetailDTO>()
        };

        // 2. Obtener estado y recursos de cada router
        foreach (var router in routers.Where(r => r.IsActive))
        {
            try
            {
                // Probar conexión
                var statusResponse = await _mikroTikService.GetRouterStatusAsync(router.Id);
                var isConnected = statusResponse.Data?.IsConnected ?? false;

                if (isConnected)
                {
                    dashboard.ConnectedRouters++;

                    // Obtener recursos del sistema
                    var resourcesResponse = await _mikroTikService.GetSystemResourcesAsync(router.Id);
                    
                    if (resourcesResponse.Status == ResponseStatus.Success)
                    {
                        var resources = resourcesResponse.Data!;
                        
                        dashboard.RoutersDetails.Add(new RouterDetailDTO
                        {
                            RouterId = router.Id,
                            RouterName = router.Name,
                            Ip = router.Ip,
                            IsConnected = true,
                            CpuLoad = resources.CpuLoad,
                            MemoryUsagePercent = (1 - (double)resources.FreeMemory / resources.TotalMemory) * 100,
                            Uptime = resources.Uptime,
                            Version = resources.Version,
                            LastSeen = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    dashboard.RoutersDetails.Add(new RouterDetailDTO
                    {
                        RouterId = router.Id,
                        RouterName = router.Name,
                        Ip = router.Ip,
                        IsConnected = false,
                        LastSeen = router.LastSeen
                    });
                }
            }
            catch (Exception ex)
            {
                // Router no alcanzable
                dashboard.RoutersDetails.Add(new RouterDetailDTO
                {
                    RouterId = router.Id,
                    RouterName = router.Name,
                    Ip = router.Ip,
                    IsConnected = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        return dashboard;
    }
}

public class RouterDashboardDTO
{
    public int TotalRouters { get; set; }
    public int ActiveRouters { get; set; }
    public int ConnectedRouters { get; set; }
    public List<RouterDetailDTO> RoutersDetails { get; set; } = new();
}

public class RouterDetailDTO
{
    public int RouterId { get; set; }
    public string RouterName { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public double? CpuLoad { get; set; }
    public double? MemoryUsagePercent { get; set; }
    public TimeSpan? Uptime { get; set; }
    public string? Version { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

## 🔄 Operaciones con Reintentos Automáticos

### El Sistema Reintenta Automáticamente:

```http
POST /api/mikrotik/routers/1/interfaces/bridge
```

**Logs del servidor:**
```
[10:00:00] Info: Ejecutando operación: /interface/bridge/add en router 1
[10:00:05] Warning: Reintentando operación MikroTik. Intento 1/3. Error: Connection timeout
[10:00:07] Warning: Reintentando operación MikroTik. Intento 2/3. Error: Connection timeout
[10:00:11] Info: Operación exitosa en router 1. Comando: /interface/bridge/add. Intentos: 3
```

**Tiempo de espera entre reintentos (Exponential Backoff):**
- Intento 1: Falla inmediatamente
- Espera: 1 segundo
- Intento 2: Falla
- Espera: 2 segundos (1 * 2^1)
- Intento 3: Éxito ✅

---

## 📊 Gestión de Conexiones

### 1. Pre-calentar Conexiones de Organización

```http
POST /api/mikrotik/organizations/1/warm-up
```

**Uso**: Ejecutar al inicio del día o cuando un usuario se loguea

**Respuesta:**
```json
{
  "status": "success",
  "message": "Conexiones pre-calentadas: 5/5 exitosas",
  "data": {
    "1": true,
    "2": true,
    "3": true,
    "4": false,
    "5": true
  }
}
```

> 💡 **Ventaja**: Las primeras operaciones serán instantáneas porque la conexión ya está establecida

---

### 2. Obtener Estado de Conexión

```http
GET /api/mikrotik/routers/1/status
```

**Respuesta:**
```json
{
  "status": "success",
  "data": {
    "routerId": 1,
    "isConnected": true,
    "lastConnected": "2024-01-15T10:00:00Z",
    "lastDisconnected": null,
    "failedAttempts": 0,
    "lastError": null,
    "latency": "00:00:00.156"
  }
}
```

---

### 3. Probar Conexión Individual

```http
GET /api/mikrotik/routers/1/test-connection
```

---

## 🔁 Operaciones en Batch (Múltiples Routers)

### Ejemplo: Configurar VLAN en todos los routers de una organización

> ⚠️ Actualmente no expuesto en API REST (solo disponible en código C#)

```csharp
public async Task ConfigurarVlanEnTodosLosRoutersAsync(int organizationId)
{
    var operation = new CreateVlanOperation();
    var request = new CreateVlanRequest
    {
        Name = "vlan-global",
        VlanId = 999,
        Interface = "bridge-lan",
        Comment = "VLAN configurada globalmente"
    };

    var response = await _mikroTikService.ExecuteOnMultipleRoutersAsync(
        organizationId,
        operation,
        request
    );

    // Resultado por router
    foreach (var (routerId, result) in response.Data!)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"Router {routerId}: VLAN creada - ID {result.Data!.Id}");
        }
        else
        {
            Console.WriteLine($"Router {routerId}: Error - {result.ErrorMessage}");
        }
    }
}
```

---

## 🛠️ Troubleshooting

### Error: "Invalid object name 'Organizations'"

**Solución**:
```bash
cd MikroClean.Infrastructure
dotnet ef database update
```

---

### Error: "Encryption:SecretKey no configurada"

**Solución**: Agregar en `appsettings.json`:
```json
{
  "Encryption": {
    "SecretKey": "clave-de-minimo-32-caracteres-aleatorios"
  }
}
```

---

### Error: "Authentication failed"

**Posibles causas**:
1. Password incorrecto al crear el router
2. Usuario MikroTik no tiene permisos API
3. API del router deshabilitada

**Solución**:
```routeros
# En MikroTik (vía WinBox o SSH)

# 1. Verificar que API está enabled
/ip service print
# Si "api" está disabled:
/ip service enable api

# 2. Verificar permisos del usuario
/user print detail
# El usuario debe tener policy: api,read,write

# 3. Crear usuario específico para API
/user add name=mikroclean-api password=TuPassword group=full policy=api,read,write,policy
```

---

### Error: "Connection timeout"

**Posibles causas**:
1. Router no alcanzable por red
2. Firewall bloqueando puerto 8728
3. IP incorrecta

**Solución**:
```bash
# 1. Verificar conectividad
ping 192.168.1.1

# 2. Verificar puerto (desde servidor donde corre la API)
Test-NetConnection -ComputerName 192.168.1.1 -Port 8728

# 3. En MikroTik, permitir API desde la IP del servidor
/ip firewall filter add chain=input src-address=<IP-DEL-SERVIDOR> protocol=tcp dst-port=8728 action=accept comment="API MikroClean"
```

---

### Error: "No se pudo conectar al router"

**Verificar**:
1. Router está activo: `isActive = true`
2. Router no eliminado: `deletedAt = null`
3. IP correcta y alcanzable
4. Credenciales correctas

**Debug**:
```http
# 1. Verificar router existe
GET /api/routers/1

# 2. Probar conexión
POST /api/routers/1/test

# 3. Ver estado de conexión
GET /api/mikrotik/routers/1/status
```

---

## 📈 Métricas y Monitoreo

### Endpoints de Salud:

```http
# Estado general de routers de organización
GET /api/routers/organization/1

# Estado de conexión específico
GET /api/mikrotik/routers/1/status

# Recursos del sistema (CPU, memoria, uptime)
GET /api/mikrotik/routers/1/system/resources
```

### Logs Importantes:

```csharp
// En producción, busca estos logs:

[Info] Nueva conexión establecida con router {RouterId} de organización {OrganizationId}
[Info] Operación exitosa en router {RouterId}. Comando: {Command}. Intentos: {Attempts}
[Warning] Reintentando operación MikroTik. Intento {Attempt}/{MaxAttempts}
[Error] Error ejecutando operación en router {RouterId} después de {Attempts} intentos
[Warning] Router {RouterId} desconectado
[Info] Cerrando todas las conexiones de la organización {OrganizationId}
```

---

## 🎯 Próximos Pasos

### Operaciones Adicionales a Implementar:

```csharp
// DHCP Server
public class CreateDhcpServerRequest
{
    public string Name { get; set; }
    public string Interface { get; set; }
    public string AddressPool { get; set; }
    public int LeaseTime { get; set; }
}

// DNS
public class AddDnsRecordRequest
{
    public string Name { get; set; }
    public string Type { get; set; } // A, AAAA, CNAME
    public string Address { get; set; }
}

// NAT (Masquerade)
public class CreateNatRuleRequest
{
    public string Chain { get; set; } // srcnat, dstnat
    public string Action { get; set; } // masquerade, dst-nat
    public string? OutInterface { get; set; }
}

// Wireless
public class CreateWirelessNetworkRequest
{
    public string Ssid { get; set; }
    public string Mode { get; set; } // ap-bridge, station
    public string Security { get; set; } // wpa2, wpa3
    public string Password { get; set; }
}

// Queue (QoS)
public class CreateQueueRequest
{
    public string Name { get; set; }
    public string Target { get; set; }
    public string MaxLimit { get; set; }
}

// Routes
public class AddRouteRequest
{
    public string DstAddress { get; set; }
    public string Gateway { get; set; }
    public int Distance { get; set; }
}
```

---

## 📞 Soporte y Recursos

### Documentación Relacionada:
- [MIKROTIK_ARCHITECTURE.md](./MIKROTIK_ARCHITECTURE.md) - Arquitectura del sistema
- [ENCRYPTION_GUIDE.md](./ENCRYPTION_GUIDE.md) - Guía de encriptación
- [MIKROTIK_USAGE_GUIDE.md](./MIKROTIK_USAGE_GUIDE.md) - Ejemplos adicionales

### MikroTik API:
- [MikroTik API Documentation](https://wiki.mikrotik.com/wiki/Manual:API)
- [tik4net GitHub](https://github.com/danikf/tik4net)
- [RouterOS Commands](https://wiki.mikrotik.com/wiki/Manual:TOC)

### Stack:
- .NET 8
- Entity Framework Core 8.0.24
- tik4net 3.5.0
- Polly 8.2.0 (Retry policies)
- SQL Server

---

## ✅ Checklist de Implementación

### Fase 1: Setup Inicial ✅
- [x] Arquitectura de capas
- [x] Connection Manager con pooling
- [x] Retry policies con Polly
- [x] Sistema de encriptación AES-256
- [x] Operaciones básicas tipadas
- [x] Repositorio de routers
- [x] CRUD de routers con encriptación automática

### Fase 2: Operaciones MikroTik ✅
- [x] Crear Bridge
- [x] Crear VLAN
- [x] Agregar IP Address
- [x] Crear Firewall Rules
- [x] Obtener System Resources
- [x] Listar Interfaces

### Fase 3: Gestión de Conexiones ✅
- [x] Test de conexión
- [x] Estado de conexión
- [x] Warm-up de conexiones
- [x] Desconexión automática

### Fase 4: Seguridad ✅
- [x] Encriptación de passwords
- [x] Configuración de SecretKey
- [x] Cache seguro de conexiones

### Fase 5: Por Implementar 📋
- [ ] DHCP Server operations
- [ ] DNS operations
- [ ] NAT operations
- [ ] Wireless operations
- [ ] Queue (QoS) operations
- [ ] Routing operations
- [ ] Background health check service
- [ ] Autorización por usuario/router
- [ ] Rate limiting
- [ ] Audit logging de operaciones
- [ ] Notificaciones de routers offline

---

## 🎓 Best Practices

### ✅ DO:
- Siempre pre-calentar conexiones al inicio de sesión
- Validar conectividad antes de operaciones masivas
- Usar try-catch y manejar errores apropiadamente
- Registrar operaciones críticas en AuditLog
- Cerrar conexiones al desloguear usuario
- Usar la capa de servicios, no llamar directo al ConnectionManager
- Validar permisos de usuario sobre router antes de operaciones

### ❌ DON'T:
- No hardcodear passwords de routers
- No mantener conexiones abiertas indefinidamente
- No ejecutar operaciones destructivas sin confirmación
- No ignorar errores de autenticación
- No exponer passwords en APIs o logs
- No usar la misma SecretKey en dev y producción

---

## 🚀 Ejemplo Completo de Uso (Frontend)

```typescript
// service/mikroTikService.ts

class MikroTikService {
  private baseUrl = 'https://api.mikroclean.com/api';

  // 1. Crear router
  async createRouter(data: CreateRouterDTO): Promise<RouterDTO> {
    const response = await fetch(`${this.baseUrl}/routers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    
    const result = await response.json();
    if (result.status === 'success') {
      return result.data;
    }
    throw new Error(result.message);
  }

  // 2. Configurar red completa
  async setupNetwork(routerId: number) {
    // Pre-calentar conexión
    await this.testConnection(routerId);

    // Crear bridge
    const bridge = await this.createBridge(routerId, {
      name: 'bridge-lan',
      comment: 'Bridge principal'
    });

    // Agregar IP
    await this.createIpAddress(routerId, {
      address: '192.168.1.1/24',
      interface: bridge.name
    });

    // Crear VLANs
    const vlans = [
      { name: 'vlan-admin', vlanId: 10, interface: bridge.name },
      { name: 'vlan-users', vlanId: 20, interface: bridge.name }
    ];

    for (const vlan of vlans) {
      await this.createVlan(routerId, vlan);
    }

    // Configurar firewall
    await this.createFirewallRule(routerId, {
      chain: 'forward',
      action: 'accept',
      srcAddress: '192.168.1.0/24',
      comment: 'Permitir tráfico LAN'
    });
  }

  // 3. Monitoreo en tiempo real
  async monitorRouter(routerId: number): Promise<void> {
    setInterval(async () => {
      const resources = await this.getSystemResources(routerId);
      
      console.log(`CPU: ${resources.cpuLoad}%`);
      console.log(`Memoria: ${resources.freeMemory / resources.totalMemory * 100}%`);
      console.log(`Uptime: ${resources.uptime}`);
      
      // Actualizar UI
      this.updateDashboard(resources);
    }, 30000); // Cada 30 segundos
  }

  private async testConnection(routerId: number): Promise<boolean> {
    const response = await fetch(`${this.baseUrl}/routers/${routerId}/test`, {
      method: 'POST'
    });
    const result = await response.json();
    return result.data;
  }

  private async createBridge(routerId: number, data: CreateBridgeRequest) {
    const response = await fetch(
      `${this.baseUrl}/mikrotik/routers/${routerId}/interfaces/bridge`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      }
    );
    const result = await response.json();
    return result.data;
  }

  // ... más métodos
}
```

---

## 📱 Ejemplo de UI (React)

```tsx
// components/RouterDashboard.tsx

import { useEffect, useState } from 'react';

export function RouterDashboard({ organizationId }: { organizationId: number }) {
  const [routers, setRouters] = useState<RouterDTO[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadRouters();
    
    // Actualizar cada minuto
    const interval = setInterval(loadRouters, 60000);
    return () => clearInterval(interval);
  }, [organizationId]);

  const loadRouters = async () => {
    try {
      const response = await fetch(`/api/routers/organization/${organizationId}`);
      const result = await response.json();
      
      if (result.status === 'success') {
        setRouters(result.data);
        
        // Pre-calentar conexiones
        await fetch(`/api/mikrotik/organizations/${organizationId}/warm-up`, {
          method: 'POST'
        });
      }
    } catch (error) {
      console.error('Error cargando routers:', error);
    } finally {
      setLoading(false);
    }
  };

  const createBridge = async (routerId: number) => {
    const bridge = {
      name: prompt('Nombre del bridge:'),
      comment: 'Creado desde dashboard'
    };

    const response = await fetch(
      `/api/mikrotik/routers/${routerId}/interfaces/bridge`,
      {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(bridge)
      }
    );

    const result = await response.json();
    
    if (result.status === 'success') {
      alert(`Bridge creado: ${result.data.name}`);
    } else {
      alert(`Error: ${result.message}`);
    }
  };

  return (
    <div>
      <h2>Routers de la Organización</h2>
      
      {loading ? (
        <p>Cargando...</p>
      ) : (
        <div>
          {routers.map(router => (
            <div key={router.id} className="router-card">
              <h3>{router.name}</h3>
              <p>IP: {router.ip}</p>
              <p>Estado: {router.isActive ? '🟢 Activo' : '🔴 Inactivo'}</p>
              <p>Última conexión: {router.lastSeen}</p>
              
              <button onClick={() => createBridge(router.id)}>
                Crear Bridge
              </button>
              <button onClick={() => getInterfaces(router.id)}>
                Ver Interfaces
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

---

## 🎉 ¡Sistema Listo para Usar!

Tu API está configurada con:
- ✅ Encriptación automática de passwords
- ✅ Connection pooling multi-tenant
- ✅ Retry policies inteligentes
- ✅ Operaciones tipadas y seguras
- ✅ Monitoreo de conexiones
- ✅ Arquitectura escalable

### Comandos para Iniciar:

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Aplicar migraciones
cd MikroClean.Infrastructure
dotnet ef database update

# 3. Ejecutar API
cd ../MikroClean.WebAPI
dotnet run

# API corriendo en: https://localhost:7000
# Swagger: https://localhost:7000/swagger
```

### Primer Test:

```bash
# 1. Crear organización
curl -X POST https://localhost:7000/api/organizations \
  -H "Content-Type: application/json" \
  -d '{"name":"Mi Empresa","email":"info@miempresa.com","phone":"555-1234"}'

# 2. Crear router
curl -X POST https://localhost:7000/api/routers \
  -H "Content-Type: application/json" \
  -d '{"name":"Router Test","ip":"192.168.1.1","user":"admin","password":"admin","organizationId":1}'

# 3. Probar conexión
curl -X POST https://localhost:7000/api/routers/1/test

# 4. Crear bridge
curl -X POST https://localhost:7000/api/mikrotik/routers/1/interfaces/bridge \
  -H "Content-Type: application/json" \
  -d '{"name":"bridge-test","comment":"Test bridge"}'
```

🎊 **¡Listo! Tu sistema MikroTik está funcionando.**
