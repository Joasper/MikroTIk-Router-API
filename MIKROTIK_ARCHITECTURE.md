# ?? Arquitectura de Conexión MikroTik - MikroClean API

## ?? Descripción General

Este sistema maneja conexiones dinámicas y concurrentes a múltiples routers MikroTik por organización con:
- ? **Connection Pool** por organización (multi-tenant)
- ? **Retry Policies** con exponential backoff (similar a EF Core)
- ? **Operaciones tipadas** (tipo de request/response específico)
- ? **Health monitoring** de conexiones
- ? **Thread-safe** y **async/await** nativo

---

## ??? Arquitectura en Capas

```
???????????????????????????????????????????????????????????????
?                    WebAPI Layer                              ?
?  Controllers: MikroTikController                             ?
?  - Endpoints REST tipados por operación                     ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
?                 Application Layer                            ?
?  Services: MikroTikService                                   ?
?  - Orquestación de operaciones                              ?
?  - Validaciones de negocio                                   ?
?  - Manejo de ApiResponse                                     ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
?                  Domain Layer                                ?
?  Interfaces:                                                 ?
?  - IMikroTikConnectionManager                               ?
?  - IMikroTikOperation<TRequest, TResponse>                  ?
?  - IMikroTikQuery<TResponse>                                ?
?  Models:                                                     ?
?  - MikroTikResult<T>                                         ?
?  - RouterConnectionInfo                                      ?
?  - Operation Models (CreateBridge, CreateVlan, etc.)        ?
???????????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????????
?               Infrastructure Layer                           ?
?  Implementaciones:                                           ?
?  - MikroTikConnectionManager (Pool manager)                 ?
?  - RouterConnectionPool (Pool por organización)             ?
?  - MikroTikClient (Wrapper de tik4net)                      ?
?  - Polly Retry Policies                                      ?
?  - Operations: Create*, Get*, Update*, Delete*              ?
???????????????????????????????????????????????????????????????
```

---

## ?? Connection Pool Multi-Tenant

### Funcionamiento

```csharp
// Estructura del pool
ConcurrentDictionary<OrganizationId, RouterConnectionPool>
    ?? RouterConnectionPool (max 20 conexiones por organización)
        ?? ConcurrentDictionary<RouterId, IMikroTikClient>
```

### Características:
- **Thread-safe**: Usa `ConcurrentDictionary` y `SemaphoreSlim`
- **Lazy connection**: Solo crea conexión cuando se necesita
- **Auto-cleanup**: Remueve conexiones fallidas automáticamente
- **Límite por organización**: Previene saturación de recursos

---

## ?? Retry Policy (Similar a EF Core)

### Configuración:
```csharp
public class MikroTikRetryPolicy
{
    public int MaxRetryAttempts = 3;
    public TimeSpan InitialDelay = 1 segundo;
    public TimeSpan MaxDelay = 10 segundos;
    public double BackoffMultiplier = 2.0; // Exponential backoff
}
```

### Errores que reintentan:
- ? `ConnectionFailed`
- ? `Timeout`
- ? `RouterUnavailable`

### Errores que NO reintentan:
- ? `AuthenticationFailed` (credenciales incorrectas)
- ? `PermissionDenied` (sin permisos)
- ? `InvalidResponse` (error de lógica)

---

## ?? Operaciones Tipadas

### Patrón de Diseño:
Cada operación implementa `IMikroTikOperation<TRequest, TResponse>`:

```csharp
public interface IMikroTikOperation<TRequest, TResponse>
{
    string Command { get; }
    Dictionary<string, string> BuildParameters(TRequest request);
    TResponse ParseResponse(ITikSentence response);
}
```

### Ejemplo: Crear Bridge

```csharp
// 1. Request tipado
var request = new CreateBridgeRequest
{
    Name = "bridge-lan",
    AgingTime = 300,
    Comment = "Bridge principal"
};

// 2. Ejecutar operación
var response = await _mikroTikService.CreateBridgeAsync(routerId, request);

// 3. Response tipado
if (response.Status == ResponseStatus.Success)
{
    var bridge = response.Data; // BridgeResponse
    Console.WriteLine($"Bridge creado: {bridge.Name} - ID: {bridge.Id}");
}
```

---

## ?? Operaciones Disponibles

### Interfaces
| Operación | Request | Response | Endpoint |
|-----------|---------|----------|----------|
| Crear Bridge | `CreateBridgeRequest` | `BridgeResponse` | `POST /api/mikrotik/routers/{id}/interfaces/bridge` |
| Crear VLAN | `CreateVlanRequest` | `VlanResponse` | `POST /api/mikrotik/routers/{id}/interfaces/vlan` |
| Listar Interfaces | - | `List<InterfaceResponse>` | `GET /api/mikrotik/routers/{id}/interfaces` |

### IP Address
| Operación | Request | Response | Endpoint |
|-----------|---------|----------|----------|
| Agregar IP | `CreateIpAddressRequest` | `IpAddressResponse` | `POST /api/mikrotik/routers/{id}/ip/address` |

### Firewall
| Operación | Request | Response | Endpoint |
|-----------|---------|----------|----------|
| Crear Regla | `CreateFirewallRuleRequest` | `FirewallRuleResponse` | `POST /api/mikrotik/routers/{id}/firewall/rules` |

### System
| Operación | Request | Response | Endpoint |
|-----------|---------|----------|----------|
| Recursos Sistema | - | `SystemResourceResponse` | `GET /api/mikrotik/routers/{id}/system/resources` |

---

## ?? Cómo Agregar Nuevas Operaciones

### Paso 1: Crear modelos en Domain Layer
```csharp
// MikroClean.Domain/MikroTik/Operations/OperationModels.cs

public class CreateDhcpServerRequest
{
    public string Name { get; set; }
    public string Interface { get; set; }
    public string AddressPool { get; set; }
}

public class DhcpServerResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    // ... más propiedades
}
```

### Paso 2: Implementar operación en Infrastructure
```csharp
// MikroClean.Infrastructure/MikroTik/Operations/DhcpOperations.cs

public class CreateDhcpServerOperation : IMikroTikOperation<CreateDhcpServerRequest, DhcpServerResponse>
{
    public string Command => "/ip/dhcp-server/add";

    public Dictionary<string, string> BuildParameters(CreateDhcpServerRequest request)
    {
        return new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["interface"] = request.Interface,
            ["address-pool"] = request.AddressPool
        };
    }

    public DhcpServerResponse ParseResponse(ITikSentence response)
    {
        return new DhcpServerResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name")
        };
    }
}
```

### Paso 3: Agregar método en IMikroTikService
```csharp
// MikroClean.Application/Interfaces/IMikroTikService.cs

Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
    int routerId, 
    CreateDhcpServerRequest request);
```

### Paso 4: Implementar en MikroTikService
```csharp
// MikroClean.Application/Services/MikroTikService.cs

public async Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
    int routerId, 
    CreateDhcpServerRequest request)
{
    try
    {
        var operation = new CreateDhcpServerOperation();
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

        if (!result.IsSuccess)
        {
            return ApiResponse<DhcpServerResponse>.Error(
                $"Error creando servidor DHCP: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<DhcpServerResponse>.Success(result.Data!, "Servidor DHCP creado");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en router {RouterId}", routerId);
        return ApiResponse<DhcpServerResponse>.Error($"Error: {ex.Message}");
    }
}
```

### Paso 5: Agregar endpoint en Controller
```csharp
// MikroClean.WebAPI/Controllers/MikroTikController.cs

[HttpPost("routers/{routerId}/dhcp/servers")]
public async Task<IActionResult> CreateDhcpServer(
    int routerId, 
    [FromBody] CreateDhcpServerRequest request)
{
    var response = await _mikroTikService.CreateDhcpServerAsync(routerId, request);
    return HandleResponse(response);
}
```

---

## ?? Seguridad y Mejores Prácticas

### 1. Encriptación de Contraseñas
**TODO**: Implementar servicio de encriptación en `MikroTikConnectionManager`:
```csharp
// Agregar IEncryptionService
Password = await _encryptionService.DecryptAsync(router.EncryptedPassword)
```

### 2. Autenticación/Autorización
- Agregar `[Authorize]` en los endpoints
- Validar que el usuario tiene acceso al router de su organización
- Usar `UserRouterAccess` para permisos granulares

### 3. Rate Limiting
- Implementar rate limiting por organización
- Prevenir saturación del router con demasiadas peticiones

### 4. Audit Logging
- Registrar todas las operaciones en `AuditLog`
- Incluir: Usuario, Router, Operación, Timestamp, Resultado

---

## ?? Monitoreo y Health Checks

### Health Check de Routers
```csharp
// Endpoint para verificar salud de routers
GET /api/mikrotik/organizations/{orgId}/health
```

### Métricas Recomendadas:
- Número de conexiones activas por organización
- Tasa de éxito/fallo de operaciones
- Latencia promedio por router
- Cantidad de reintentos promedio

---

## ?? Ejemplo de Uso Completo

### Escenario: Configurar red para nueva sucursal

```csharp
// 1. Crear bridge
var bridgeRequest = new CreateBridgeRequest
{
    Name = "bridge-sucursal-01",
    Comment = "Bridge para sucursal 01"
};
var bridgeResult = await mikroTikService.CreateBridgeAsync(routerId, bridgeRequest);

// 2. Crear VLAN
var vlanRequest = new CreateVlanRequest
{
    Name = "vlan-office",
    VlanId = 100,
    Interface = "bridge-sucursal-01"
};
var vlanResult = await mikroTikService.CreateVlanAsync(routerId, vlanRequest);

// 3. Agregar IP al bridge
var ipRequest = new CreateIpAddressRequest
{
    Address = "192.168.100.1/24",
    Interface = "bridge-sucursal-01"
};
var ipResult = await mikroTikService.CreateIpAddressAsync(routerId, ipRequest);

// 4. Crear regla de firewall
var firewallRequest = new CreateFirewallRuleRequest
{
    Chain = "forward",
    Action = "accept",
    InInterface = "vlan-office",
    Comment = "Permitir tráfico VLAN office"
};
var fwResult = await mikroTikService.CreateFirewallRuleAsync(routerId, firewallRequest);
```

---

## ?? Paquetes NuGet Requeridos

### MikroClean.Infrastructure.csproj
```xml
<PackageReference Include="tik4net" Version="3.7.0" />
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="8.0.0" />
```

### Instalación:
```bash
cd MikroClean.Infrastructure
dotnet add package tik4net
dotnet add package Polly
dotnet add package Microsoft.Extensions.Caching.Memory
```

---

## ?? Ventajas de esta Arquitectura

1. **Type-Safe**: Cada operación tiene request/response específicos
2. **Escalable**: Pool por organización previene conflictos
3. **Resiliente**: Retry automático con exponential backoff
4. **Mantenible**: Agregar operaciones es simple y predecible
5. **Performance**: Cache y reutilización de conexiones
6. **Multi-tenant**: Aislamiento entre organizaciones
7. **Testeable**: Interfaces permiten mocking fácil

---

## ?? Próximos Pasos

### Fase 1: Core Completado ?
- [x] Connection Manager
- [x] Pool de conexiones
- [x] Retry policies
- [x] Operaciones básicas

### Fase 2: Seguridad (TODO)
- [ ] Servicio de encriptación de contraseñas
- [ ] Autorización por router
- [ ] Rate limiting

### Fase 3: Operaciones Adicionales (TODO)
- [ ] DHCP Server
- [ ] DNS
- [ ] Routing
- [ ] NAT
- [ ] Wireless
- [ ] Queue (QoS)

### Fase 4: Monitoring (TODO)
- [ ] Health checks background service
- [ ] Métricas en tiempo real
- [ ] Dashboard de conexiones

---

## ?? Troubleshooting

### Error: "Invalid object name 'Organizations'"
**Causa**: La tabla no existe en la base de datos  
**Solución**: Ejecutar migraciones
```bash
dotnet ef database update --project MikroClean.Infrastructure
```

### Error: "Connection timeout"
**Causa**: Router no alcanzable o firewall bloqueando puerto 8728  
**Solución**: 
- Verificar conectividad de red
- Asegurar puerto 8728 abierto
- Aumentar `ConnectionTimeout` en configuración

### Error: "Authentication failed"
**Causa**: Credenciales incorrectas o password no descifrado  
**Solución**: 
- Implementar `IEncryptionService`
- Verificar credenciales en DB

---

## ?? Soporte

Para más información consultar:
- [Documentación tik4net](https://github.com/danikf/tik4net)
- [MikroTik API](https://wiki.mikrotik.com/wiki/Manual:API)
- [Polly Documentation](https://github.com/App-vNext/Polly)
