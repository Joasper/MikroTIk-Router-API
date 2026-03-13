# ?? Tutorial: Cómo Agregar Nuevas Funcionalidades MikroTik

## ?? Índice
1. [Entendiendo el Flujo Completo](#entendiendo-el-flujo-completo)
2. [Paso a Paso: Agregar Nueva Operación](#paso-a-paso-agregar-nueva-operación)
3. [Ejemplo Completo: Implementar DHCP Server](#ejemplo-completo-implementar-dhcp-server)
4. [Cómo Funciona la Conexión](#cómo-funciona-la-conexión)
5. [Debugging y Testing](#debugging-y-testing)

---

## ?? Entendiendo el Flujo Completo

### ¿Cómo funciona todo el sistema?

```
???????????????????????????????????????????????????????????????????
?  USUARIO/FRONTEND                                                ?
?  Envía: POST /api/mikrotik/routers/1/dhcp/servers              ?
?  Body: { "name": "dhcp1", "interface": "bridge-lan" }          ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  1. WEBAPI CONTROLLER (Capa de Presentación)                    ?
?  MikroTikController.cs                                           ?
?                                                                  ?
?  [HttpPost("routers/{routerId}/dhcp/servers")]                  ?
?  public async Task<IActionResult> CreateDhcpServer(             ?
?      int routerId,                                               ?
?      CreateDhcpServerRequest request)                            ?
?  {                                                               ?
?      var response = await _mikroTikService                       ?
?          .CreateDhcpServerAsync(routerId, request);              ?
?      return HandleResponse(response);                            ?
?  }                                                               ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  2. APPLICATION SERVICE (Lógica de Negocio)                     ?
?  MikroTikService.cs                                              ?
?                                                                  ?
?  public async Task<ApiResponse<DhcpServerResponse>>             ?
?      CreateDhcpServerAsync(int routerId, CreateDhcpServerRequest)?
?  {                                                               ?
?      var operation = new CreateDhcpServerOperation();           ?
?      var result = await _connectionManager                       ?
?          .ExecuteOperationAsync(routerId, operation, request);   ?
?      return ApiResponse.Success(result.Data);                    ?
?  }                                                               ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  3. OPERATION CLASS (Convierte Request ? Comando MikroTik)      ?
?  CreateDhcpServerOperation.cs                                    ?
?                                                                  ?
?  public Dictionary<string, string> BuildParameters(request)     ?
?  {                                                               ?
?      return new Dictionary<string, string>                       ?
?      {                                                           ?
?          ["name"] = request.Name,                                ?
?          ["interface"] = request.Interface,                      ?
?          ["address-pool"] = request.AddressPool                  ?
?      };                                                          ?
?  }                                                               ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  4. CONNECTION MANAGER (Gestiona Pool y Reintentos)             ?
?  MikroTikConnectionManager.cs                                    ?
?                                                                  ?
?  • Obtiene router de DB                                          ?
?  • Desencripta password                                          ?
?  • Busca/crea conexión en el pool                               ?
?  • Ejecuta con retry policy                                      ?
?  • Maneja errores y reconexiones                                 ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  5. CONNECTION POOL (Pool por Organización)                     ?
?  RouterConnectionPool.cs                                         ?
?                                                                  ?
?  Organización 1:                                                 ?
?    ?? Router 1: [Conexión Activa]                               ?
?    ?? Router 2: [Conexión Activa]                               ?
?    ?? Router 3: [Conexión Inactiva]                             ?
?                                                                  ?
?  Organización 2:                                                 ?
?    ?? Router 4: [Conexión Activa]                               ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  6. MIKROTIK CLIENT (Wrapper de tik4net)                        ?
?  MikroTikClient.cs                                               ?
?                                                                  ?
?  • connection.Open(ip, port, user, password)                     ?
?  • command = connection.CreateCommand("/ip/dhcp-server/add")     ?
?  • command.AddParameter("name", "dhcp1")                         ?
?  • result = command.ExecuteScalar()                              ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  7. ROUTER MIKROTIK (Dispositivo Físico)                        ?
?  192.168.1.1:8728                                                ?
?                                                                  ?
?  Recibe comando API:                                             ?
?  /ip/dhcp-server/add                                             ?
?  =name=dhcp1                                                     ?
?  =interface=bridge-lan                                           ?
?                                                                  ?
?  Ejecuta y responde:                                             ?
?  !done                                                           ?
?  =ret=*1                                                         ?
???????????????????????????????????????????????????????????????????
                         ?
???????????????????????????????????????????????????????????????????
?  8. RESPONSE (De vuelta al usuario)                             ?
?                                                                  ?
?  {                                                               ?
?    "status": "success",                                          ?
?    "message": "Servidor DHCP creado exitosamente",              ?
?    "data": {                                                     ?
?      "id": "*1",                                                 ?
?      "name": "dhcp1",                                            ?
?      "interface": "bridge-lan"                                   ?
?    }                                                             ?
?  }                                                               ?
???????????????????????????????????????????????????????????????????
```

---

## ?? Paso a Paso: Agregar Nueva Operación

### Ejemplo: Vamos a implementar **DHCP Server**

---

### **PASO 1: Crear Modelos (Domain Layer)** ??

**Archivo**: `MikroClean.Domain/MikroTik/Operations/OperationModels.cs`

```csharp
// Agregar al archivo existente:

// ============= DHCP SERVER =============

public class CreateDhcpServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public string AddressPool { get; set; } = string.Empty;
    public int LeaseTime { get; set; } = 600; // segundos
    public bool Disabled { get; set; } = false;
    public string? Comment { get; set; }
}

public class DhcpServerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public string AddressPool { get; set; } = string.Empty;
    public int LeaseTime { get; set; }
    public bool Disabled { get; set; }
    public bool Invalid { get; set; }
    public string? Comment { get; set; }
}

public class DhcpPoolRequest
{
    public string Name { get; set; } = string.Empty;
    public string Ranges { get; set; } = string.Empty; // ej: "192.168.1.100-192.168.1.200"
    public string? Comment { get; set; }
}

public class DhcpPoolResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Ranges { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
```

> ?? **¿Por qué aquí?**: Los modelos son contratos del dominio, no dependen de implementación

---

### **PASO 2: Implementar la Operación (Application Layer)** ??

**Archivo**: `MikroClean.Application/MikroTik/Operations/DhcpOperations.cs` (NUEVO)

```csharp
using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Application.MikroTik.Operations
{
    /// <summary>
    /// Operación para crear un servidor DHCP en MikroTik
    /// </summary>
    public class CreateDhcpServerOperation : IMikroTikOperation<CreateDhcpServerRequest, DhcpServerResponse>
    {
        // 1. Define el comando de la API de MikroTik
        public string Command => "/ip/dhcp-server/add";

        // 2. Convierte tu Request en parámetros de MikroTik API
        public Dictionary<string, string> BuildParameters(CreateDhcpServerRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["name"] = request.Name,
                ["interface"] = request.Interface,
                ["address-pool"] = request.AddressPool,
                ["lease-time"] = request.LeaseTime.ToString(),
                ["disabled"] = request.Disabled ? "yes" : "no"
            };

            if (!string.IsNullOrEmpty(request.Comment))
            {
                parameters["comment"] = request.Comment;
            }

            return parameters;
        }

        // 3. Convierte la respuesta del router a tu Response object
        public DhcpServerResponse ParseResponse(ITikSentence response)
        {
            return new DhcpServerResponse
            {
                Id = response.GetResponseField(".id"),
                Name = response.GetResponseField("name"),
                Interface = response.GetResponseField("interface"),
                AddressPool = response.GetResponseField("address-pool"),
                LeaseTime = int.TryParse(response.GetResponseField("lease-time"), out var lt) ? lt : 0,
                Disabled = response.GetResponseField("disabled") == "true",
                Invalid = response.GetResponseField("invalid") == "true",
                Comment = response.GetResponseField("comment")
            };
        }
    }

    /// <summary>
    /// Operación para crear un pool de direcciones DHCP
    /// </summary>
    public class CreateDhcpPoolOperation : IMikroTikOperation<DhcpPoolRequest, DhcpPoolResponse>
    {
        public string Command => "/ip/pool/add";

        public Dictionary<string, string> BuildParameters(DhcpPoolRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["name"] = request.Name,
                ["ranges"] = request.Ranges
            };

            if (!string.IsNullOrEmpty(request.Comment))
            {
                parameters["comment"] = request.Comment;
            }

            return parameters;
        }

        public DhcpPoolResponse ParseResponse(ITikSentence response)
        {
            return new DhcpPoolResponse
            {
                Id = response.GetResponseField(".id"),
                Name = response.GetResponseField("name"),
                Ranges = response.GetResponseField("ranges"),
                Comment = response.GetResponseField("comment")
            };
        }
    }

    /// <summary>
    /// Query para obtener todos los servidores DHCP
    /// </summary>
    public class GetAllDhcpServersQuery : IMikroTikQuery<List<DhcpServerResponse>>
    {
        public string Command => "/ip/dhcp-server/print";

        public List<DhcpServerResponse> ParseResponse(IEnumerable<ITikSentence> responses)
        {
            var servers = new List<DhcpServerResponse>();

            foreach (var sentence in responses)
            {
                servers.Add(new DhcpServerResponse
                {
                    Id = sentence.GetResponseField(".id"),
                    Name = sentence.GetResponseField("name"),
                    Interface = sentence.GetResponseField("interface"),
                    AddressPool = sentence.GetResponseField("address-pool"),
                    LeaseTime = int.TryParse(sentence.GetResponseField("lease-time"), out var lt) ? lt : 0,
                    Disabled = sentence.GetResponseField("disabled") == "true",
                    Invalid = sentence.GetResponseField("invalid") == "true",
                    Comment = sentence.GetResponseField("comment")
                });
            }

            return servers;
        }
    }
}
```

> ?? **¿Por qué aquí?**: Las operaciones están en Application porque usan lógica específica de tu negocio

---

### **PASO 3: Agregar Métodos en el Servicio (Application Layer)** ??

**Archivo**: `MikroClean.Application/Interfaces/IMikroTikService.cs`

```csharp
// Agregar al final de la interfaz:

// ============= DHCP SERVER =============

/// <summary>
/// Crea un pool de direcciones IP
/// </summary>
Task<ApiResponse<DhcpPoolResponse>> CreateDhcpPoolAsync(
    int routerId, 
    DhcpPoolRequest request);

/// <summary>
/// Crea un servidor DHCP
/// </summary>
Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
    int routerId, 
    CreateDhcpServerRequest request);

/// <summary>
/// Obtiene todos los servidores DHCP del router
/// </summary>
Task<ApiResponse<List<DhcpServerResponse>>> GetAllDhcpServersAsync(int routerId);
```

**Archivo**: `MikroClean.Application/Services/MikroTikService.cs`

```csharp
// Agregar al final de la clase, antes del último }

// ============= DHCP SERVER =============

public async Task<ApiResponse<DhcpPoolResponse>> CreateDhcpPoolAsync(
    int routerId, 
    DhcpPoolRequest request)
{
    try
    {
        var operation = new CreateDhcpPoolOperation();
        var result = await _connectionManager.ExecuteOperationAsync(
            routerId, 
            operation, 
            request
        );

        if (!result.IsSuccess)
        {
            return ApiResponse<DhcpPoolResponse>.Error(
                $"Error creando pool DHCP: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<DhcpPoolResponse>.Success(
            result.Data!, 
            "Pool DHCP creado exitosamente"
        );
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creando pool DHCP en router {RouterId}", routerId);
        return ApiResponse<DhcpPoolResponse>.Error($"Error inesperado: {ex.Message}");
    }
}

public async Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
    int routerId, 
    CreateDhcpServerRequest request)
{
    try
    {
        var operation = new CreateDhcpServerOperation();
        var result = await _connectionManager.ExecuteOperationAsync(
            routerId, 
            operation, 
            request
        );

        if (!result.IsSuccess)
        {
            return ApiResponse<DhcpServerResponse>.Error(
                $"Error creando servidor DHCP: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<DhcpServerResponse>.Success(
            result.Data!, 
            "Servidor DHCP creado exitosamente"
        );
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creando servidor DHCP en router {RouterId}", routerId);
        return ApiResponse<DhcpServerResponse>.Error($"Error inesperado: {ex.Message}");
    }
}

public async Task<ApiResponse<List<DhcpServerResponse>>> GetAllDhcpServersAsync(int routerId)
{
    try
    {
        var query = new GetAllDhcpServersQuery();
        var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

        if (!result.IsSuccess)
        {
            return ApiResponse<List<DhcpServerResponse>>.Error(
                $"Error obteniendo servidores DHCP: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<List<DhcpServerResponse>>.Success(
            result.Data!,
            $"Se encontraron {result.Data!.Count} servidores DHCP"
        );
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error obteniendo servidores DHCP de router {RouterId}", routerId);
        return ApiResponse<List<DhcpServerResponse>>.Error($"Error inesperado: {ex.Message}");
    }
}
```

> ?? **Patrón**: Siempre es igual:
> 1. Crear instancia de la operación
> 2. Ejecutar via ConnectionManager
> 3. Verificar si fue exitoso
> 4. Retornar ApiResponse

---

### **PASO 4: Crear Endpoints REST (WebAPI Layer)** ??

**Archivo**: `MikroClean.WebAPI/Controllers/MikroTikController.cs`

```csharp
// Agregar al final de la clase:

// ============= DHCP SERVER =============

/// <summary>
/// Crea un pool de direcciones IP para DHCP
/// POST: api/mikrotik/routers/{routerId}/dhcp/pools
/// </summary>
[HttpPost("routers/{routerId}/dhcp/pools")]
public async Task<IActionResult> CreateDhcpPool(
    int routerId, 
    [FromBody] DhcpPoolRequest request)
{
    var response = await _mikroTikService.CreateDhcpPoolAsync(routerId, request);
    return HandleResponse(response);
}

/// <summary>
/// Crea un servidor DHCP
/// POST: api/mikrotik/routers/{routerId}/dhcp/servers
/// </summary>
[HttpPost("routers/{routerId}/dhcp/servers")]
public async Task<IActionResult> CreateDhcpServer(
    int routerId, 
    [FromBody] CreateDhcpServerRequest request)
{
    var response = await _mikroTikService.CreateDhcpServerAsync(routerId, request);
    return HandleResponse(response);
}

/// <summary>
/// Obtiene todos los servidores DHCP del router
/// GET: api/mikrotik/routers/{routerId}/dhcp/servers
/// </summary>
[HttpGet("routers/{routerId}/dhcp/servers")]
public async Task<IActionResult> GetAllDhcpServers(int routerId)
{
    var response = await _mikroTikService.GetAllDhcpServersAsync(routerId);
    return HandleResponse(response);
}
```

---

## ? ¡Listo! Ya puedes usar tu nueva funcionalidad:

```bash
# 1. Crear pool de IPs
curl -X POST https://localhost:7000/api/mikrotik/routers/1/dhcp/pools \
  -H "Content-Type: application/json" \
  -d '{
    "name": "dhcp-pool-lan",
    "ranges": "192.168.1.100-192.168.1.200",
    "comment": "Pool para LAN"
  }'

# 2. Crear servidor DHCP
curl -X POST https://localhost:7000/api/mikrotik/routers/1/dhcp/servers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "dhcp-lan",
    "interface": "bridge-lan",
    "addressPool": "dhcp-pool-lan",
    "leaseTime": 600,
    "comment": "Servidor DHCP para LAN"
  }'

# 3. Listar servidores DHCP
curl https://localhost:7000/api/mikrotik/routers/1/dhcp/servers
```

---

## ?? Cómo Funciona la Conexión Internamente

### 1?? Primera Conexión (Cache Miss)

```csharp
// Usuario ejecuta:
POST /api/mikrotik/routers/1/interfaces/bridge

// Internamente:
???????????????????????????????????????????????????????????
? MikroTikConnectionManager.ExecuteOperationAsync()       ?
?   ?                                                      ?
? GetOrCreateClientAsync(routerId: 1)                     ?
?   ?                                                      ?
? Cache Miss ? No existe en pool                          ?
?   ?                                                      ?
? RouterRepository.GetByIdAsync(1)                        ?
?   ?                                                      ?
? Obtiene Router de DB:                                   ?
?   {                                                      ?
?     Id: 1,                                               ?
?     Ip: "192.168.1.1",                                   ?
?     User: "admin",                                       ?
?     EncryptedPassword: "kJ9mP2vX8qR5..." ? ENCRIPTADO   ?
?   }                                                      ?
?   ?                                                      ?
? EncryptionService.Decrypt("kJ9mP2vX8qR5...")           ?
?   ?                                                      ?
? Password desencriptado: "MiPassword123!"                ?
?   ?                                                      ?
? MikroTikClient.ConnectAsync(                            ?
?     ip: "192.168.1.1",                                   ?
?     port: 8728,                                          ?
?     user: "admin",                                       ?
?     password: "MiPassword123!" ? EN TEXTO PLANO         ?
? )                                                        ?
?   ?                                                      ?
? tik4net: connection.Open(...)                           ?
?   ?                                                      ?
? ? CONECTADO                                            ?
?   ?                                                      ?
? Se guarda en Pool de Organización                       ?
? Se cachea por 5 minutos                                  ?
???????????????????????????????????????????????????????????
```

### 2?? Segunda Conexión (Cache Hit)

```csharp
// 30 segundos después, usuario ejecuta otra operación:
POST /api/mikrotik/routers/1/interfaces/vlan

// Internamente:
???????????????????????????????????????????????????????????
? MikroTikConnectionManager.ExecuteOperationAsync()       ?
?   ?                                                      ?
? GetOrCreateClientAsync(routerId: 1)                     ?
?   ?                                                      ?
? ? Cache Hit ? Encuentra conexión existente             ?
?   ?                                                      ?
? Verifica: client.IsConnected = true                     ?
?   ?                                                      ?
? Reutiliza la conexión existente ? MÁS RÁPIDO           ?
?   ?                                                      ?
? Ejecuta comando inmediatamente                           ?
???????????????????????????????????????????????????????????
```

> ? **Performance**: La primera operación tarda ~500ms, las siguientes ~50ms

---

### 3?? Con Reintentos (Retry Policy)

```csharp
// Usuario ejecuta operación, pero router está temporalmente caído:
POST /api/mikrotik/routers/1/interfaces/bridge

// Internamente:
???????????????????????????????????????????????????????????
? Intento 1:                                               ?
?   ?                                                      ?
? Intentando conectar a 192.168.1.1...                    ?
? ? Timeout (10 segundos)                                ?
?   ?                                                      ?
? ShouldRetry(TimeoutException) = true                    ?
? Esperar: 1 segundo                                       ?
?                                                          ?
? Intento 2:                                               ?
?   ?                                                      ?
? Intentando conectar a 192.168.1.1...                    ?
? ? Connection failed                                    ?
?   ?                                                      ?
? ShouldRetry(ConnectionFailed) = true                    ?
? Esperar: 2 segundos (exponential backoff)               ?
?                                                          ?
? Intento 3:                                               ?
?   ?                                                      ?
? Intentando conectar a 192.168.1.1...                    ?
? ? CONECTADO (router volvió a la vida)                 ?
?   ?                                                      ?
? Ejecuta comando: /interface/bridge/add                  ?
? ? Bridge creado                                        ?
?   ?                                                      ?
? Log: "Operación exitosa. Intentos: 3"                   ?
???????????????????????????????????????????????????????????
```

**Logs que verías:**
```
[10:00:00] Info: Ejecutando operación /interface/bridge/add en router 1
[10:00:10] Warning: Reintentando operación MikroTik. Intento 1/3. Error: Connection timeout
[10:00:13] Warning: Reintentando operación MikroTik. Intento 2/3. Error: Connection failed
[10:00:16] Info: Operación exitosa en router 1. Comando: /interface/bridge/add. Intentos: 3
```

---

## ?? Ejemplo Completo: DHCP Server

### Flujo Completo desde Cero:

```csharp
// ====================
// 1. USUARIO: Registra router en el sistema
// ====================

POST /api/routers
{
  "name": "Router Sucursal A",
  "ip": "192.168.1.1",
  "user": "admin",
  "password": "MikroTikPass123!",  // ? TEXTO PLANO
  "organizationId": 1
}

// SISTEMA HACE:
// - EncryptionService.Encrypt("MikroTikPass123!")
// - Guarda en DB: "kJ9mP2vX8qR5tY3wN6bV1cZ4sA7dF0gH=="


// ====================
// 2. USUARIO: Crea pool de IPs
// ====================

POST /api/mikrotik/routers/1/dhcp/pools
{
  "name": "dhcp-pool-office",
  "ranges": "192.168.1.100-192.168.1.200",
  "comment": "Pool para oficinas"
}

// SISTEMA HACE:
// ???????????????????????????????????????
// ? 1. MikroTikController recibe request?
// ? 2. Llama a MikroTikService          ?
// ? 3. MikroTikService crea Operation   ?
// ? 4. ConnectionManager ejecuta:       ?
// ?    a) Busca router en pool ? NO     ?
// ?    b) Lee router de DB              ?
// ?    c) Desencripta password          ?
// ?    d) Crea MikroTikClient           ?
// ?    e) client.Connect(ip, user, pwd) ?
// ?    f) Guarda en pool                ?
// ? 5. Ejecuta comando en router:       ?
// ?    /ip/pool/add                     ?
// ?    =name=dhcp-pool-office           ?
// ?    =ranges=192.168.1.100-...        ?
// ? 6. Router responde: !done =ret=*1   ?
// ? 7. ParseResponse convierte a objeto ?
// ? 8. Retorna ApiResponse              ?
// ???????????????????????????????????????

// RESPUESTA:
{
  "status": "success",
  "message": "Pool DHCP creado exitosamente",
  "data": {
    "id": "*1",
    "name": "dhcp-pool-office",
    "ranges": "192.168.1.100-192.168.1.200"
  }
}


// ====================
// 3. USUARIO: Crea servidor DHCP
// ====================

POST /api/mikrotik/routers/1/dhcp/servers
{
  "name": "dhcp-office",
  "interface": "bridge-lan",
  "addressPool": "dhcp-pool-office",
  "leaseTime": 600
}

// SISTEMA HACE:
// ???????????????????????????????????????
// ? 1-3. Mismo flujo que antes          ?
// ? 4. ConnectionManager ejecuta:       ?
// ?    a) Busca router en pool ? ? SÍ  ?
// ?    b) Reutiliza conexión existente  ?  ? MÁS RÁPIDO!
// ?    c) NO desencripta ni reconecta   ?
// ? 5. Ejecuta comando en router:       ?
// ?    /ip/dhcp-server/add              ?
// ?    =name=dhcp-office                ?
// ?    =interface=bridge-lan            ?
// ?    =address-pool=dhcp-pool-office   ?
// ?    =lease-time=600                  ?
// ? 6. Router responde: !done =ret=*2   ?
// ? 7-8. Retorna resultado              ?
// ???????????????????????????????????????

// RESPUESTA:
{
  "status": "success",
  "message": "Servidor DHCP creado exitosamente",
  "data": {
    "id": "*2",
    "name": "dhcp-office",
    "interface": "bridge-lan",
    "addressPool": "dhcp-pool-office",
    "leaseTime": 600,
    "disabled": false
  }
}


// ====================
// 4. USUARIO: Lista servidores DHCP
// ====================

GET /api/mikrotik/routers/1/dhcp/servers

// SISTEMA HACE:
// - Reutiliza conexión del pool
// - Ejecuta: /ip/dhcp-server/print
// - Convierte todas las respuestas a List<DhcpServerResponse>

// RESPUESTA:
{
  "status": "success",
  "message": "Se encontraron 1 servidores DHCP",
  "data": [
    {
      "id": "*2",
      "name": "dhcp-office",
      "interface": "bridge-lan",
      "addressPool": "dhcp-pool-office",
      "leaseTime": 600,
      "disabled": false
    }
  ]
}
```

---

## ?? Resumen de los 4 Pasos para Agregar Funcionalidad

| Paso | Archivo | Qué Hacer |
|------|---------|-----------|
| 1?? | `Domain/.../OperationModels.cs` | Crear `Request` y `Response` classes |
| 2?? | `Application/.../Operations/` | Crear `Operation` class con `Command`, `BuildParameters`, `ParseResponse` |
| 3?? | `Application/.../MikroTikService.cs` | Agregar método que usa la Operation |
| 4?? | `WebAPI/.../MikroTikController.cs` | Crear endpoint HTTP que llama al servicio |

---

## ?? ¿Cómo sé qué comando usar?

### Consulta la documentación de MikroTik API:

**URL**: https://wiki.mikrotik.com/wiki/Manual:API

### Ejemplos de comandos comunes:

| Funcionalidad | Comando API | Documentación |
|---------------|-------------|---------------|
| Crear Bridge | `/interface/bridge/add` | [Bridge](https://wiki.mikrotik.com/wiki/Manual:Interface/Bridge) |
| Crear VLAN | `/interface/vlan/add` | [VLAN](https://wiki.mikrotik.com/wiki/Manual:Interface/VLAN) |
| Agregar IP | `/ip/address/add` | [IP Address](https://wiki.mikrotik.com/wiki/Manual:IP/Address) |
| Crear Pool | `/ip/pool/add` | [IP Pool](https://wiki.mikrotik.com/wiki/Manual:IP/Pools) |
| DHCP Server | `/ip/dhcp-server/add` | [DHCP](https://wiki.mikrotik.com/wiki/Manual:IP/DHCP_Server) |
| Firewall Rule | `/ip/firewall/filter/add` | [Firewall](https://wiki.mikrotik.com/wiki/Manual:IP/Firewall/Filter) |
| NAT | `/ip/firewall/nat/add` | [NAT](https://wiki.mikrotik.com/wiki/Manual:IP/Firewall/NAT) |
| Route | `/ip/route/add` | [Routing](https://wiki.mikrotik.com/wiki/Manual:IP/Route) |
| DNS | `/ip/dns/static/add` | [DNS](https://wiki.mikrotik.com/wiki/Manual:IP/DNS) |
| Wireless | `/interface/wireless/add` | [Wireless](https://wiki.mikrotik.com/wiki/Manual:Interface/Wireless) |

### ?? Probar comandos directamente en MikroTik:

```routeros
# Conectarse al router via WinBox o SSH

# Ver ayuda del comando:
/ip/dhcp-server/add ?

# Ejemplo:
/ip/dhcp-server/add name=dhcp1 interface=bridge-lan address-pool=pool1

# Ver resultado:
/ip/dhcp-server/print detail
```

---

## ?? Template Rápido para Copiar/Pegar

### Crear nueva operación en 5 minutos:

```csharp
// ========================================
// 1. MODELS (Domain)
// ========================================
public class Create[NOMBRE]Request
{
    public string Name { get; set; } = string.Empty;
    // ... tus propiedades
}

public class [NOMBRE]Response
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    // ... tus propiedades
}

// ========================================
// 2. OPERATION (Application)
// ========================================
public class Create[NOMBRE]Operation : IMikroTikOperation<Create[NOMBRE]Request, [NOMBRE]Response>
{
    public string Command => "/tu/comando/mikrotik/add";

    public Dictionary<string, string> BuildParameters(Create[NOMBRE]Request request)
    {
        return new Dictionary<string, string>
        {
            ["name"] = request.Name,
            // ... tus parámetros
        };
    }

    public [NOMBRE]Response ParseResponse(ITikSentence response)
    {
        return new [NOMBRE]Response
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            // ... tus campos
        };
    }
}

// ========================================
// 3. SERVICE METHOD (Application)
// ========================================
public async Task<ApiResponse<[NOMBRE]Response>> Create[NOMBRE]Async(
    int routerId, 
    Create[NOMBRE]Request request)
{
    try
    {
        var operation = new Create[NOMBRE]Operation();
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

        if (!result.IsSuccess)
        {
            return ApiResponse<[NOMBRE]Response>.Error(
                $"Error creando [nombre]: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<[NOMBRE]Response>.Success(result.Data!, "[Nombre] creado exitosamente");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creando [nombre] en router {RouterId}", routerId);
        return ApiResponse<[NOMBRE]Response>.Error($"Error inesperado: {ex.Message}");
    }
}

// ========================================
// 4. CONTROLLER ENDPOINT (WebAPI)
// ========================================
[HttpPost("routers/{routerId}/[ruta]")]
public async Task<IActionResult> Create[NOMBRE](
    int routerId, 
    [FromBody] Create[NOMBRE]Request request)
{
    var response = await _mikroTikService.Create[NOMBRE]Async(routerId, request);
    return HandleResponse(response);
}
```

> ?? Reemplaza `[NOMBRE]` con tu funcionalidad: DhcpServer, NatRule, DnsRecord, etc.

---

## ?? Debugging y Testing

### Ver qué está pasando internamente:

```csharp
// Agregar breakpoints en:

1. MikroTikController.Create[Operacion]()
   ? Ver el request que llega

2. MikroTikService.Create[Operacion]Async()
   ? Ver que se crea la operation correctamente

3. CreateOperationClass.BuildParameters()
   ? Ver los parámetros que se envían al router

4. MikroTikConnectionManager.ExecuteOperationAsync()
   ? Ver el proceso de conexión y retry

5. MikroTikClient.ExecuteCommandAsync()
   ? Ver la ejecución real del comando

6. CreateOperationClass.ParseResponse()
   ? Ver la respuesta del router y cómo se parsea
```

### Ejemplo de debugging real:

```csharp
// En MikroTikService.CreateDhcpServerAsync()
public async Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(...)
{
    // BREAKPOINT AQUÍ ?
    var operation = new CreateDhcpServerOperation();
    
    // Inspeccionar en debugger:
    // - routerId: 1
    // - request.Name: "dhcp-office"
    // - request.Interface: "bridge-lan"
    
    // BREAKPOINT AQUÍ ?
    var result = await _connectionManager.ExecuteOperationAsync(...);
    
    // Inspeccionar:
    // - result.IsSuccess: true/false
    // - result.Data: DhcpServerResponse { Id = "*1", Name = "dhcp-office" }
    // - result.ErrorMessage: null (si exitoso)
    
    // BREAKPOINT AQUÍ ?
    return ApiResponse<DhcpServerResponse>.Success(result.Data!, "...");
}
```

---

## ?? Entendiendo los Componentes

### ?? Request/Response Models (Domain)
**Propósito**: Definir el contrato de entrada/salida  
**Ejemplo**: `CreateBridgeRequest` ? lo que el usuario envía  
**Ubicación**: `MikroClean.Domain/MikroTik/Operations/OperationModels.cs`

```csharp
// Lo que TÚ defines basándote en lo que necesitas
public class CreateBridgeRequest
{
    public string Name { get; set; }     // Usuario: "bridge-lan"
    public bool AdminMac { get; set; }   // Usuario: false
    public int AgingTime { get; set; }   // Usuario: 300
}
```

---

### ?? Operation Class (Application)
**Propósito**: Traducir tu Request a comandos MikroTik API  
**Ejemplo**: `CreateBridgeOperation`  
**Ubicación**: `MikroClean.Application/MikroTik/Operations/`

```csharp
public class CreateBridgeOperation : IMikroTikOperation<CreateBridgeRequest, BridgeResponse>
{
    // 1. El comando de MikroTik API
    public string Command => "/interface/bridge/add";

    // 2. Traducir tu objeto a parámetros MikroTik
    public Dictionary<string, string> BuildParameters(CreateBridgeRequest request)
    {
        // Tu Request:          Parámetros MikroTik:
        // Name: "bridge-lan" ? ["name"] = "bridge-lan"
        // AdminMac: false    ? ["admin-mac"] = "no"
        // AgingTime: 300     ? ["ageing-time"] = "300"
        
        return new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["admin-mac"] = request.AdminMac ? "yes" : "no",
            ["ageing-time"] = request.AgingTime.ToString()
        };
    }

    // 3. Traducir respuesta MikroTik a tu Response object
    public BridgeResponse ParseResponse(ITikSentence response)
    {
        // Respuesta MikroTik:    Tu Response object:
        // .id=*1               ? Id = "*1"
        // name=bridge-lan      ? Name = "bridge-lan"
        // running=true         ? Running = true
        
        return new BridgeResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            Running = response.GetResponseField("running") == "true"
        };
    }
}
```

---

### ?? Service Method (Application)
**Propósito**: Orquestar la operación y manejar errores  
**Ejemplo**: Método en `MikroTikService`  

```csharp
public async Task<ApiResponse<BridgeResponse>> CreateBridgeAsync(
    int routerId, 
    CreateBridgeRequest request)
{
    try
    {
        // 1. Crear instancia de la operación
        var operation = new CreateBridgeOperation();
        
        // 2. Ejecutar via ConnectionManager (maneja todo automáticamente)
        var result = await _connectionManager.ExecuteOperationAsync(
            routerId,    // ? A qué router conectarse
            operation,   // ? Qué operación ejecutar
            request      // ? Con qué datos
        );

        // 3. Verificar resultado
        if (!result.IsSuccess)
        {
            // Hubo error ? retornar ApiResponse.Error
            return ApiResponse<BridgeResponse>.Error(
                $"Error: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        // 4. Éxito ? retornar ApiResponse.Success
        return ApiResponse<BridgeResponse>.Success(
            result.Data!, 
            "Bridge creado exitosamente"
        );
    }
    catch (Exception ex)
    {
        // Error inesperado
        _logger.LogError(ex, "Error en router {RouterId}", routerId);
        return ApiResponse<BridgeResponse>.Error($"Error: {ex.Message}");
    }
}
```

---

### ?? Controller Endpoint (WebAPI)
**Propósito**: Exponer como REST API  
**Ejemplo**: Endpoint en `MikroTikController`

```csharp
/// <summary>
/// Crea una interfaz bridge en un router
/// POST: api/mikrotik/routers/{routerId}/interfaces/bridge
/// </summary>
[HttpPost("routers/{routerId}/interfaces/bridge")]
public async Task<IActionResult> CreateBridge(
    int routerId,                        // ? De la URL
    [FromBody] CreateBridgeRequest request)  // ? Del body JSON
{
    // Simplemente llama al servicio y retorna la respuesta
    var response = await _mikroTikService.CreateBridgeAsync(routerId, request);
    return HandleResponse(response);  // ? Convierte a HTTP status code
}
```

---

## ?? Comparación: OPERACIÓN vs QUERY

### ?? OPERACIÓN (Modifica el router)

**Ejemplos**: Crear, Actualizar, Eliminar

```csharp
// Implementa: IMikroTikOperation<TRequest, TResponse>
public class CreateBridgeOperation : IMikroTikOperation<CreateBridgeRequest, BridgeResponse>
{
    public string Command => "/interface/bridge/add";  // ? add, set, remove
    public Dictionary<string, string> BuildParameters(request) { ... }
    public BridgeResponse ParseResponse(ITikSentence response) { ... }
}

// Uso:
var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);
```

### ?? QUERY (Solo lee información)

**Ejemplos**: Listar, Obtener detalles, Ver estado

```csharp
// Implementa: IMikroTikQuery<TResponse>
public class GetAllBridgesQuery : IMikroTikQuery<List<BridgeResponse>>
{
    public string Command => "/interface/bridge/print";  // ? print, get
    public List<BridgeResponse> ParseResponse(IEnumerable<ITikSentence> responses) { ... }
}

// Uso:
var result = await _connectionManager.ExecuteQueryAsync(routerId, query);
```

**Diferencia clave:**
- **Operation**: Recibe UNA respuesta (el item creado/modificado)
- **Query**: Recibe MÚLTIPLES respuestas (lista de items)

---

## ?? Quick Start: Agregar Tu Primera Funcionalidad

### Vamos a agregar: **NAT (Masquerade)**

#### 1. Investiga el comando MikroTik:

```routeros
# En tu router MikroTik:
/ip/firewall/nat/add ?

# Respuesta:
action: masquerade | dst-nat | src-nat
chain: srcnat | dstnat
out-interface: <interface-name>
```

#### 2. Crea los modelos:

```csharp
// MikroClean.Domain/MikroTik/Operations/OperationModels.cs

public class CreateNatRuleRequest
{
    public string Chain { get; set; } = "srcnat";  // srcnat o dstnat
    public string Action { get; set; } = "masquerade";
    public string? OutInterface { get; set; }
    public string? SrcAddress { get; set; }
    public string? Comment { get; set; }
    public bool Disabled { get; set; } = false;
}

public class NatRuleResponse
{
    public string Id { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OutInterface { get; set; }
    public bool Disabled { get; set; }
    public string? Comment { get; set; }
}
```

#### 3. Implementa la operación:

```csharp
// MikroClean.Application/MikroTik/Operations/NatOperations.cs (NUEVO ARCHIVO)

using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Application.MikroTik.Operations
{
    public class CreateNatRuleOperation : IMikroTikOperation<CreateNatRuleRequest, NatRuleResponse>
    {
        public string Command => "/ip/firewall/nat/add";

        public Dictionary<string, string> BuildParameters(CreateNatRuleRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["chain"] = request.Chain,
                ["action"] = request.Action,
                ["disabled"] = request.Disabled ? "yes" : "no"
            };

            if (!string.IsNullOrEmpty(request.OutInterface))
                parameters["out-interface"] = request.OutInterface;
            
            if (!string.IsNullOrEmpty(request.SrcAddress))
                parameters["src-address"] = request.SrcAddress;
            
            if (!string.IsNullOrEmpty(request.Comment))
                parameters["comment"] = request.Comment;

            return parameters;
        }

        public NatRuleResponse ParseResponse(ITikSentence response)
        {
            return new NatRuleResponse
            {
                Id = response.GetResponseField(".id"),
                Chain = response.GetResponseField("chain"),
                Action = response.GetResponseField("action"),
                OutInterface = response.GetResponseField("out-interface"),
                Disabled = response.GetResponseField("disabled") == "true",
                Comment = response.GetResponseField("comment")
            };
        }
    }
}
```

#### 4. Agrega al servicio:

```csharp
// MikroClean.Application/Interfaces/IMikroTikService.cs
Task<ApiResponse<NatRuleResponse>> CreateNatRuleAsync(int routerId, CreateNatRuleRequest request);

// MikroClean.Application/Services/MikroTikService.cs
public async Task<ApiResponse<NatRuleResponse>> CreateNatRuleAsync(
    int routerId, 
    CreateNatRuleRequest request)
{
    try
    {
        var operation = new CreateNatRuleOperation();
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

        if (!result.IsSuccess)
        {
            return ApiResponse<NatRuleResponse>.Error(
                $"Error creando regla NAT: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<NatRuleResponse>.Success(result.Data!, "Regla NAT creada exitosamente");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creando NAT en router {RouterId}", routerId);
        return ApiResponse<NatRuleResponse>.Error($"Error inesperado: {ex.Message}");
    }
}
```

#### 5. Crea el endpoint:

```csharp
// MikroClean.WebAPI/Controllers/MikroTikController.cs

/// <summary>
/// Crea una regla NAT (masquerade)
/// POST: api/mikrotik/routers/{routerId}/firewall/nat
/// </summary>
[HttpPost("routers/{routerId}/firewall/nat")]
public async Task<IActionResult> CreateNatRule(
    int routerId, 
    [FromBody] CreateNatRuleRequest request)
{
    var response = await _mikroTikService.CreateNatRuleAsync(routerId, request);
    return HandleResponse(response);
}
```

#### 6. ¡Úsalo!

```bash
curl -X POST https://localhost:7000/api/mikrotik/routers/1/firewall/nat \
  -H "Content-Type: application/json" \
  -d '{
    "chain": "srcnat",
    "action": "masquerade",
    "outInterface": "ether1",
    "comment": "NAT para salida a internet"
  }'
```

---

## ?? Comandos MikroTik Más Comunes

### Formato del Protocolo API:

```
Comando:    /path/to/command
Parámetros: =key=value
Flags:      ?flag

Ejemplo real:
/ip/dhcp-server/add =name=dhcp1 =interface=bridge-lan =address-pool=pool1
```

### Conversión a nuestro sistema:

| MikroTik API | Nuestro Sistema |
|--------------|-----------------|
| `/ip/dhcp-server/add` | `Command = "/ip/dhcp-server/add"` |
| `=name=dhcp1` | `parameters["name"] = "dhcp1"` |
| `=interface=bridge-lan` | `parameters["interface"] = "bridge-lan"` |

---

## ?? Cómo Descubrir Comandos y Parámetros

### Método 1: Terminal MikroTik

```routeros
# Conectarse al router
ssh admin@192.168.1.1

# Ver ayuda de cualquier comando con "?"
/ip/dhcp-server/add ?

# Resultado:
address-pool        -- Name of address pool for lease allocation
interface           -- Interface on which server will be running
name                -- Server name
lease-time          -- Time that DHCP client can use assigned address
...
```

### Método 2: WinBox

1. Abrir WinBox
2. Ir al menú deseado (ej: IP ? DHCP Server)
3. Click en "Add" o "Edit"
4. Ver los campos disponibles
5. Traducir a tu Request class

### Método 3: Documentación Oficial

https://wiki.mikrotik.com/wiki/Manual:TOC

Busca tu funcionalidad y verás ejemplos de comandos.

---

## ?? Tips y Trucos

### ? Nombres de Campos

MikroTik usa kebab-case:
```csharp
// ? MAL
parameters["addressPool"] = "pool1";

// ? BIEN
parameters["address-pool"] = "pool1";
```

### ? Valores Booleanos

MikroTik usa "yes"/"no":
```csharp
// ? MAL
parameters["disabled"] = request.Disabled.ToString(); // "True"/"False"

// ? BIEN
parameters["disabled"] = request.Disabled ? "yes" : "no";
```

### ? Valores Numéricos

Convertir a string:
```csharp
parameters["lease-time"] = request.LeaseTime.ToString();
parameters["vlan-id"] = request.VlanId.ToString();
```

### ? Campos Opcionales

Solo agregar si tienen valor:
```csharp
if (!string.IsNullOrEmpty(request.Comment))
{
    parameters["comment"] = request.Comment;
}
```

---

## ?? Cheat Sheet de Operaciones

### Crear (add)
```csharp
Command: "/path/to/add"
Método: ExecuteOperationAsync()
Response: Un objeto (el item creado)
```

### Listar (print)
```csharp
Command: "/path/to/print"
Método: ExecuteQueryAsync()
Response: Lista de objetos
```

### Modificar (set)
```csharp
Command: "/path/to/set"
Parámetros: Incluye ".id" del item + campos a modificar
```

### Eliminar (remove)
```csharp
Command: "/path/to/remove"
Parámetros: Solo ".id" del item
```

### Habilitar/Deshabilitar (enable/disable)
```csharp
Command: "/path/to/enable" o "/path/to/disable"
Parámetros: ".id" del item
```

---

## ?? Video Tutorial (Conceptual)

### Tiempo estimado: 15 minutos por funcionalidad nueva

```
00:00 - Investigar comando MikroTik (wiki.mikrotik.com)
03:00 - Crear Request/Response models (Domain)
06:00 - Implementar Operation class (Application)
09:00 - Agregar método en Service (Application)
12:00 - Crear endpoint en Controller (WebAPI)
15:00 - Probar en Swagger
```

---

## ? Checklist para Nueva Funcionalidad

```
? 1. Investigué el comando MikroTik en la wiki
? 2. Probé el comando manualmente en el router
? 3. Creé Request class con todos los parámetros necesarios
? 4. Creé Response class con todos los campos de respuesta
? 5. Implementé Operation class con Command, BuildParameters, ParseResponse
? 6. Agregué método en IMikroTikService interface
? 7. Implementé método en MikroTikService class
? 8. Creé endpoint HTTP en MikroTikController
? 9. Compilé sin errores (dotnet build)
? 10. Probé en Swagger
? 11. Verifiqué el resultado en el router MikroTik
```

---

## ?? ¡Ya Entiendes el Sistema!

Ahora sabes:
- ? Cómo fluye una petición de principio a fin
- ? Cómo se conecta al router (con pool y cache)
- ? Cómo se encriptan/desencriptan passwords
- ? Cómo funcionan los reintentos automáticos
- ? Cómo agregar cualquier funcionalidad nueva
- ? Dónde poner cada pieza de código

---

## ?? Siguiente Lectura

- **[COMPLETE_USAGE_GUIDE.md](./COMPLETE_USAGE_GUIDE.md)** - Más ejemplos de uso
- **[MIKROTIK_ARCHITECTURE.md](./MIKROTIK_ARCHITECTURE.md)** - Arquitectura detallada
- **[ENCRYPTION_GUIDE.md](./ENCRYPTION_GUIDE.md)** - Sistema de encriptación

---

## ?? ¿Necesitas Ayuda?

### Pregunta común: "¿Cómo implemento [X]?"

1. Busca en wiki.mikrotik.com el comando para [X]
2. Copia el template de este documento
3. Reemplaza [NOMBRE] con tu funcionalidad
4. Ajusta los parámetros según la documentación
5. ¡Listo!

### Ejemplo de pregunta bien formulada:

> "Quiero implementar crear un pool de IP addresses. El comando de MikroTik es `/ip/pool/add` y los parámetros son `name` y `ranges`. ¿Cómo lo hago?"

**Respuesta**: Sigue el template del PASO 2 con:
- `Command = "/ip/pool/add"`
- `parameters["name"] = request.Name`
- `parameters["ranges"] = request.Ranges`

---

?? **¡Éxito! Ahora eres experto en agregar funcionalidades MikroTik a tu API.**
