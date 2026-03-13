# Guía Paso a Paso: Cómo Agregar una Nueva Funcionalidad MikroTik

> **Ejemplo práctico:** Implementación completa de **IP Pools** (GET, CREATE, UPDATE, DELETE)

---

## Tabla de Contenidos

1. [Arquitectura del Proyecto](#1-arquitectura-del-proyecto)
2. [Resumen del Flujo](#2-resumen-del-flujo)
3. [Paso 1: Crear los Modelos (Request/Response)](#paso-1-crear-los-modelos-requestresponse)
4. [Paso 2: Crear las Operaciones (Operations)](#paso-2-crear-las-operaciones-operations)
5. [Paso 3: Agregar Métodos al Servicio (Interface + Implementación)](#paso-3-agregar-métodos-al-servicio)
6. [Paso 4: Agregar Endpoints al Controller](#paso-4-agregar-endpoints-al-controller)
7. [Paso 5: Registrar Dependencias (si aplica)](#paso-5-registrar-dependencias)
8. [Checklist Final](#checklist-final)
9. [Referencia de la API MikroTik para IP Pools](#referencia-api-mikrotik)

---

## 1. Arquitectura del Proyecto

El proyecto sigue **Clean Architecture** con 5 capas:

```
MikroClean.sln
├── MikroClean.Domain/              ← Entidades, Interfaces, Modelos MikroTik
│   ├── Entities/                   ← Entidades de base de datos
│   ├── Interfaces/                 ← Contratos de repositorios y seguridad
│   └── MikroTik/                   ← Abstracciones del protocolo MikroTik
│       ├── IMikroTikOperation.cs   ← Interface para operaciones (CRUD)
│       ├── IMikroTikConnectionManager.cs
│       ├── MikroTikResult.cs       ← Resultado de operaciones MikroTik
│       ├── RouterConnectionModels.cs
│       └── Operations/
│           └── OperationModels.cs  ← 📍 MODELOS Request/Response
│
├── MikroClean.Application/         ← Lógica de negocio, DTOs, Servicios
│   ├── Interfaces/
│   │   └── IMikroTikService.cs     ← 📍 INTERFACE del servicio
│   ├── Services/
│   │   └── MikroTikService.cs      ← 📍 IMPLEMENTACIÓN del servicio
│   ├── MikroTik/Operations/
│   │   └── StandardOperations.cs   ← 📍 OPERACIONES (BuildParams + ParseResponse)
│   ├── Models/
│   │   └── ApiResponse.cs          ← Wrapper de respuestas API estándar
│   └── Dtos/                       ← DTOs para otras funcionalidades (Auth, Routers, etc.)
│
├── MikroClean.Infrastructure/      ← Implementaciones concretas
│   ├── MikroTik/
│   │   ├── MikroTikClient.cs       ← Cliente TCP al router
│   │   ├── MikroTikConnectionManager.cs ← Pool de conexiones + ejecución
│   │   └── RouterConnectionPool.cs
│   ├── Repositories/               ← Repositorios EF Core
│   ├── Context/                    ← DbContext
│   └── Configurations/             ← Configuraciones de EF Core (Fluent API)
│
├── MikroClean.InversionOfControl/  ← Registro de dependencias (DI)
│   └── DependencyContainer.cs      ← 📍 REGISTRO DE SERVICIOS
│
└── MikroClean.WebAPI/              ← Capa de presentación
    ├── Controllers/
    │   ├── Base/
    │   │   └── BaseApiController.cs ← HandleResponse() para respuestas HTTP
    │   └── MikroTikController.cs   ← 📍 ENDPOINTS DE LA API
    └── Program.cs
```

---

## 2. Resumen del Flujo

Cuando un cliente hace un request HTTP, el flujo es:

```
Cliente HTTP
    │
    ▼
┌──────────────────────────────────────────────────────────────────┐
│  MikroTikController (WebAPI)                                     │
│  ► Recibe el request HTTP                                        │
│  ► Delega al servicio                                            │
│  ► Retorna HandleResponse(response)                              │
└──────┬───────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  MikroTikService (Application)                                   │
│  ► Valida datos                                                  │
│  ► Crea la operación (new CreatePoolOperation())                 │
│  ► Llama a _connectionManager.ExecuteOperationAsync()            │
│  ► Envuelve resultado en ApiResponse<T>                          │
└──────┬───────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  MikroTikConnectionManager (Infrastructure)                      │
│  ► Obtiene conexión del pool                                     │
│  ► Ejecuta el Command de la operación                            │
│  ► Llama a operation.BuildParameters() para armar los params     │
│  ► Llama a operation.ParseResponse() con la respuesta del router │
│  ► Retorna MikroTikResult<T>                                     │
└──────┬───────────────────────────────────────────────────────────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────────┐
│  Router MikroTik (Hardware)                                      │
│  ► Recibe el comando via API TCP (puerto 8728)                   │
│  ► Ejecuta la acción (crear pool, listar, etc.)                  │
│  ► Devuelve la respuesta con los campos del recurso              │
└──────────────────────────────────────────────────────────────────┘
```

### Interfaces Clave

| Interface | Archivo | Propósito |
|---|---|---|
| `IMikroTikOperation<TRequest, TResponse>` | `Domain/MikroTik/IMikroTikOperation.cs` | Define una operación de escritura (add, set, remove) |
| `IMikroTikQuery<TResponse>` | `Domain/MikroTik/IMikroTikOperation.cs` | Define una operación de lectura (print) |
| `IMikroTikConnectionManager` | `Domain/MikroTik/IMikroTikConnectionManager.cs` | Ejecuta operaciones contra el router |
| `IMikroTikService` | `Application/Interfaces/IMikroTikService.cs` | Contrato del servicio de aplicación |

### Diferencia entre Operation y Query

| | `IMikroTikOperation<TReq, TRes>` | `IMikroTikQuery<TRes>` |
|---|---|---|
| **Uso** | Crear, Actualizar, Eliminar | Listar, Obtener |
| **Tiene Request?** | ✅ Sí (`TRequest`) | ❌ No (solo envía el comando) |
| **ParseResponse recibe** | Un `ITikSentence` (una respuesta) | `IEnumerable<ITikSentence>` (lista) |
| **Se ejecuta con** | `ExecuteOperationAsync()` | `ExecuteQueryAsync()` |

---

## Paso 1: Crear los Modelos (Request/Response)

### 📁 Archivo: `MikroClean.Domain/MikroTik/Operations/OperationModels.cs`

Agrega al final del archivo (antes del último `}`) los modelos del recurso que quieres manejar. Cada funcionalidad necesita:

- **Request**: los datos que el usuario envía para crear/actualizar
- **Response**: los datos que el router devuelve

### Qué devuelve MikroTik para IP Pools (`/ip/pool/print`)

El router MikroTik devuelve estos campos para un IP Pool:

| Campo MikroTik | Tipo | Descripción |
|---|---|---|
| `.id` | string | ID interno del recurso (ej: `*1`) |
| `name` | string | Nombre del pool |
| `ranges` | string | Rangos de IP (ej: `192.168.1.100-192.168.1.200`) |
| `next-pool` | string | Pool siguiente (encadenamiento) |
| `comment` | string | Comentario opcional |

### Código a agregar:

```csharp
// ============= IP POOLS =============

/// <summary>
/// Request para crear un IP Pool
/// Comando MikroTik: /ip/pool/add
/// </summary>
public class CreateIpPoolRequest
{
    /// <summary>
    /// Nombre del pool (obligatorio)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Rangos de IP. Formato: "192.168.1.100-192.168.1.200"
    /// Se pueden especificar múltiples rangos separados por coma
    /// </summary>
    public string Ranges { get; set; } = string.Empty;

    /// <summary>
    /// Pool siguiente en la cadena (opcional).
    /// Cuando este pool se agota, el DHCP usará el next-pool.
    /// </summary>
    public string? NextPool { get; set; }

    /// <summary>
    /// Comentario descriptivo (opcional)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Request para actualizar un IP Pool existente
/// Comando MikroTik: /ip/pool/set
/// </summary>
public class UpdateIpPoolRequest
{
    /// <summary>
    /// ID del pool a actualizar (obligatorio - formato MikroTik: *1, *2, etc.)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo nombre del pool (null = no cambiar)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Nuevos rangos de IP (null = no cambiar)
    /// </summary>
    public string? Ranges { get; set; }

    /// <summary>
    /// Nuevo pool siguiente (null = no cambiar, "" = eliminar)
    /// </summary>
    public string? NextPool { get; set; }

    /// <summary>
    /// Nuevo comentario (null = no cambiar)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Request para eliminar un IP Pool
/// Comando MikroTik: /ip/pool/remove
/// </summary>
public class DeleteIpPoolRequest
{
    /// <summary>
    /// ID del pool a eliminar (formato MikroTik: *1, *2, etc.)
    /// </summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Response que representa un IP Pool del router
/// </summary>
public class IpPoolResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Ranges { get; set; } = string.Empty;
    public string? NextPool { get; set; }
    public string? Comment { get; set; }
}
```

### 🔑 Reglas importantes para los modelos:

1. **Siempre incluir `= string.Empty`** en strings obligatorios para evitar null references
2. **Usar `string?`** para campos opcionales
3. **El campo `Id` en los Response** siempre mapea a `.id` del router (formato `*1`, `*2`)
4. **El campo `Id` en Update/Delete Request** es obligatorio para identificar el recurso

---

## Paso 2: Crear las Operaciones (Operations)

### 📁 Archivo: `MikroClean.Application/MikroTik/Operations/StandardOperations.cs`

Las operaciones son clases que implementan `IMikroTikOperation<TRequest, TResponse>` o `IMikroTikQuery<TResponse>` y definen:

1. **`Command`**: El comando de la API MikroTik (ej: `/ip/pool/add`)
2. **`BuildParameters()`**: Convierte tu Request C# a un `Dictionary<string, string>` para el router
3. **`ParseResponse()`**: Convierte la respuesta del router a tu modelo Response de C#

### Referencia de comandos MikroTik para IP Pools:

| Acción | Comando API MikroTik |
|---|---|
| Listar todos | `/ip/pool/print` |
| Crear | `/ip/pool/add` |
| Actualizar | `/ip/pool/set` |
| Eliminar | `/ip/pool/remove` |

### Código a agregar:

```csharp
// ============= IP POOL OPERATIONS =============

/// <summary>
/// Query: Obtener todos los IP Pools del router
/// GET /ip/pool/print
/// </summary>
public class GetAllIpPoolsQuery : IMikroTikQuery<List<IpPoolResponse>>
{
    public string Command => "/ip/pool/print";

    public List<IpPoolResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var pools = new List<IpPoolResponse>();

        foreach (var sentence in responses)
        {
            pools.Add(new IpPoolResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                Ranges = sentence.GetResponseField("ranges") ?? string.Empty,
                NextPool = sentence.GetResponseField("next-pool"),
                Comment = sentence.GetResponseField("comment")
            });
        }

        return pools;
    }
}

/// <summary>
/// Operación: Crear un nuevo IP Pool
/// POST /ip/pool/add
/// </summary>
public class CreateIpPoolOperation : IMikroTikOperation<CreateIpPoolRequest, IpPoolResponse>
{
    public string Command => "/ip/pool/add";

    public Dictionary<string, string> BuildParameters(CreateIpPoolRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["ranges"] = request.Ranges
        };

        if (!string.IsNullOrEmpty(request.NextPool))
            parameters["next-pool"] = request.NextPool;

        if (!string.IsNullOrEmpty(request.Comment))
            parameters["comment"] = request.Comment;

        return parameters;
    }

    public IpPoolResponse ParseResponse(ITikSentence response)
    {
        return new IpPoolResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            Ranges = response.GetResponseField("ranges") ?? string.Empty,
            NextPool = response.GetResponseField("next-pool"),
            Comment = response.GetResponseField("comment")
        };
    }
}

/// <summary>
/// Operación: Actualizar un IP Pool existente
/// PUT /ip/pool/set
/// </summary>
public class UpdateIpPoolOperation : IMikroTikOperation<UpdateIpPoolRequest, IpPoolResponse>
{
    public string Command => "/ip/pool/set";

    public Dictionary<string, string> BuildParameters(UpdateIpPoolRequest request)
    {
        // Para "set" SIEMPRE se necesita el .id del recurso
        var parameters = new Dictionary<string, string>
        {
            [".id"] = request.Id
        };

        // Solo enviar los campos que el usuario quiere cambiar
        if (!string.IsNullOrEmpty(request.Name))
            parameters["name"] = request.Name;

        if (!string.IsNullOrEmpty(request.Ranges))
            parameters["ranges"] = request.Ranges;

        if (request.NextPool != null) // string.Empty = eliminar, null = no cambiar
            parameters["next-pool"] = request.NextPool;

        if (!string.IsNullOrEmpty(request.Comment))
            parameters["comment"] = request.Comment;

        return parameters;
    }

    public IpPoolResponse ParseResponse(ITikSentence response)
    {
        return new IpPoolResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            Ranges = response.GetResponseField("ranges") ?? string.Empty,
            NextPool = response.GetResponseField("next-pool"),
            Comment = response.GetResponseField("comment")
        };
    }
}

/// <summary>
/// Operación: Eliminar un IP Pool
/// DELETE /ip/pool/remove
/// </summary>
public class DeleteIpPoolOperation : IMikroTikOperation<DeleteIpPoolRequest, IpPoolResponse>
{
    public string Command => "/ip/pool/remove";

    public Dictionary<string, string> BuildParameters(DeleteIpPoolRequest request)
    {
        return new Dictionary<string, string>
        {
            [".id"] = request.Id
        };
    }

    public IpPoolResponse ParseResponse(ITikSentence response)
    {
        // El remove no devuelve datos, pero debemos retornar algo
        return new IpPoolResponse
        {
            Id = response.GetResponseField(".id") ?? string.Empty
        };
    }
}
```

### 🔑 Reglas importantes para las operaciones:

1. **El `Command`** debe coincidir exactamente con la ruta de la API MikroTik
2. **`BuildParameters()`** mapea propiedades C# (PascalCase) → parámetros MikroTik (kebab-case)
3. **Para operaciones `set`** (update), SIEMPRE incluir `[".id"] = request.Id`
4. **Para operaciones `remove`** (delete), solo enviar `[".id"] = request.Id`
5. **`ParseResponse()`** usa `sentence.GetResponseField("nombre-campo")` para leer datos
6. **Campos numéricos** se parsean con `long.TryParse()` o `int.Parse()`
7. **Campos booleanos** se comparan con `== "true"`

---

## Paso 3: Agregar Métodos al Servicio

### 📁 Archivo 1: `MikroClean.Application/Interfaces/IMikroTikService.cs`

Agrega las firmas de los nuevos métodos en la interface:

```csharp
// ============= IP POOLS =============

/// <summary>
/// Obtiene todos los IP pools de un router
/// </summary>
Task<ApiResponse<List<IpPoolResponse>>> GetAllIpPoolsAsync(int routerId);

/// <summary>
/// Crea un nuevo IP pool en un router
/// </summary>
Task<ApiResponse<IpPoolResponse>> CreateIpPoolAsync(int routerId, CreateIpPoolRequest request);

/// <summary>
/// Actualiza un IP pool existente en un router
/// </summary>
Task<ApiResponse<IpPoolResponse>> UpdateIpPoolAsync(int routerId, UpdateIpPoolRequest request);

/// <summary>
/// Elimina un IP pool de un router
/// </summary>
Task<ApiResponse<IpPoolResponse>> DeleteIpPoolAsync(int routerId, DeleteIpPoolRequest request);
```

### 📁 Archivo 2: `MikroClean.Application/Services/MikroTikService.cs`

Implementa cada método siguiendo el patrón existente:

```csharp
// ============= IP POOLS =============

public async Task<ApiResponse<List<IpPoolResponse>>> GetAllIpPoolsAsync(int routerId)
{
    try
    {
        // 1. Crear la query (lectura, no tiene Request)
        var query = new GetAllIpPoolsQuery();

        // 2. Ejecutar la query a través del ConnectionManager
        var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

        // 3. Verificar si la operación fue exitosa
        if (!result.IsSuccess)
        {
            return ApiResponse<List<IpPoolResponse>>.Error(
                $"Error obteniendo IP pools: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        // 4. Retornar resultado exitoso
        return ApiResponse<List<IpPoolResponse>>.Success(
            result.Data!,
            $"Se encontraron {result.Data!.Count} IP pools"
        );
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error obteniendo IP pools de router {RouterId}", routerId);
        return ApiResponse<List<IpPoolResponse>>.Error($"Error inesperado: {ex.Message}");
    }
}

public async Task<ApiResponse<IpPoolResponse>> CreateIpPoolAsync(int routerId, CreateIpPoolRequest request)
{
    try
    {
        // 1. Crear la operación (escritura)
        var operation = new CreateIpPoolOperation();

        // 2. Ejecutar la operación. El ConnectionManager:
        //    - Llama a operation.BuildParameters(request) → genera el diccionario
        //    - Envía el Command + parámetros al router
        //    - Llama a operation.ParseResponse(sentence) → genera el Response
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

        // 3. Verificar resultado
        if (!result.IsSuccess)
        {
            return ApiResponse<IpPoolResponse>.Error(
                $"Error creando IP pool: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        // 4. Retornar éxito
        return ApiResponse<IpPoolResponse>.Success(result.Data!, "IP Pool creado exitosamente");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creando IP pool en router {RouterId}", routerId);
        return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
    }
}

public async Task<ApiResponse<IpPoolResponse>> UpdateIpPoolAsync(int routerId, UpdateIpPoolRequest request)
{
    try
    {
        var operation = new UpdateIpPoolOperation();
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

        if (!result.IsSuccess)
        {
            return ApiResponse<IpPoolResponse>.Error(
                $"Error actualizando IP pool: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<IpPoolResponse>.Success(result.Data!, "IP Pool actualizado exitosamente");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error actualizando IP pool en router {RouterId}", routerId);
        return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
    }
}

public async Task<ApiResponse<IpPoolResponse>> DeleteIpPoolAsync(int routerId, DeleteIpPoolRequest request)
{
    try
    {
        var operation = new DeleteIpPoolOperation();
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);

        if (!result.IsSuccess)
        {
            return ApiResponse<IpPoolResponse>.Error(
                $"Error eliminando IP pool: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        return ApiResponse<IpPoolResponse>.Success(result.Data!, "IP Pool eliminado exitosamente");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error eliminando IP pool en router {RouterId}", routerId);
        return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {ex.Message}");
    }
}
```

### 🧩 Patrón del Servicio (plantilla para cualquier funcionalidad):

```csharp
public async Task<ApiResponse<TResponse>> NombreMetodoAsync(int routerId, TRequest request)
{
    try
    {
        // 1. Crear operación o query
        var operation = new MiOperacion();

        // 2. Ejecutar (Operation para escritura, Query para lectura)
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, request);
        // ó: var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

        // 3. Verificar resultado
        if (!result.IsSuccess)
        {
            return ApiResponse<TResponse>.Error(
                $"Error: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        // 4. Retornar éxito
        return ApiResponse<TResponse>.Success(result.Data!, "Mensaje de éxito");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en operación para router {RouterId}", routerId);
        return ApiResponse<TResponse>.Error($"Error inesperado: {ex.Message}");
    }
}
```

---

## Paso 4: Agregar Endpoints al Controller

### 📁 Archivo: `MikroClean.WebAPI/Controllers/MikroTikController.cs`

Agrega los endpoints HTTP. Cada endpoint:
1. Recibe el `routerId` de la URL
2. Recibe el body (si aplica) con `[FromBody]`
3. Delega al servicio
4. Retorna `HandleResponse(response)`

```csharp
// ============= IP POOLS =============

/// <summary>
/// Obtiene todos los IP pools de un router
/// GET: api/mikrotik/routers/{routerId}/ip/pools
/// </summary>
[HttpGet("routers/{routerId}/ip/pools")]
public async Task<IActionResult> GetAllIpPools(int routerId)
{
    var response = await _mikroTikService.GetAllIpPoolsAsync(routerId);
    return HandleResponse(response);
}

/// <summary>
/// Crea un nuevo IP pool en un router
/// POST: api/mikrotik/routers/{routerId}/ip/pools
/// </summary>
[HttpPost("routers/{routerId}/ip/pools")]
public async Task<IActionResult> CreateIpPool(int routerId, [FromBody] CreateIpPoolRequest request)
{
    var response = await _mikroTikService.CreateIpPoolAsync(routerId, request);
    return HandleResponse(response);
}

/// <summary>
/// Actualiza un IP pool existente en un router
/// PUT: api/mikrotik/routers/{routerId}/ip/pools
/// </summary>
[HttpPut("routers/{routerId}/ip/pools")]
public async Task<IActionResult> UpdateIpPool(int routerId, [FromBody] UpdateIpPoolRequest request)
{
    var response = await _mikroTikService.UpdateIpPoolAsync(routerId, request);
    return HandleResponse(response);
}

/// <summary>
/// Elimina un IP pool de un router
/// DELETE: api/mikrotik/routers/{routerId}/ip/pools
/// </summary>
[HttpDelete("routers/{routerId}/ip/pools")]
public async Task<IActionResult> DeleteIpPool(int routerId, [FromBody] DeleteIpPoolRequest request)
{
    var response = await _mikroTikService.DeleteIpPoolAsync(routerId, request);
    return HandleResponse(response);
}
```

### Convención de rutas:

```
api/mikrotik/routers/{routerId}/{seccion}/{subseccion}
```

| Recurso MikroTik | Ruta API Sugerida |
|---|---|
| `/ip/pool` | `routers/{routerId}/ip/pools` |
| `/ip/address` | `routers/{routerId}/ip/address` |
| `/ip/firewall/filter` | `routers/{routerId}/firewall/rules` |
| `/ip/dhcp-server` | `routers/{routerId}/dhcp/servers` |
| `/ip/dns` | `routers/{routerId}/ip/dns` |
| `/interface/bridge` | `routers/{routerId}/interfaces/bridge` |
| `/queue/simple` | `routers/{routerId}/queues/simple` |
| `/system/scheduler` | `routers/{routerId}/system/schedulers` |

---

## Paso 5: Registrar Dependencias

### 📁 Archivo: `MikroClean.InversionOfControl/DependencyContainer.cs`

> **⚠️ NOTA**: Para funcionalidades MikroTik que solo agregan métodos al `MikroTikService` existente y no crean un nuevo servicio, **NO necesitas modificar este archivo**. Las operaciones y queries se instancian directamente dentro del servicio con `new`.

Solo necesitas modificar `DependencyContainer.cs` si:
- Creas un **nuevo servicio** separado (ej: `IIpPoolService` / `IpPoolService`)
- Creas un **nuevo repositorio**
- Agregas algún **servicio singleton** nuevo

Si decides crear un servicio separado, agrega:

```csharp
// En el método AddDependencies()
services.AddScoped<IIpPoolService, IpPoolService>();
```

---

## Checklist Final

Antes de probar tu nueva funcionalidad, verifica:

### Archivos Modificados

| # | Archivo | Acción | ¿Qué agregar? |
|---|---|---|---|
| 1 | `Domain/MikroTik/Operations/OperationModels.cs` | Modificar | Clases `Request` y `Response` |
| 2 | `Application/MikroTik/Operations/StandardOperations.cs` | Modificar | Clases `Operation` y `Query` |
| 3 | `Application/Interfaces/IMikroTikService.cs` | Modificar | Firmas de métodos |
| 4 | `Application/Services/MikroTikService.cs` | Modificar | Implementación de métodos |
| 5 | `WebAPI/Controllers/MikroTikController.cs` | Modificar | Endpoints HTTP |
| 6 | `InversionOfControl/DependencyContainer.cs` | ¿Modificar? | Solo si creas un servicio nuevo |

### Validaciones

- [ ] Los nombres de los `Command` coinciden con la API MikroTik (ej: `/ip/pool/add`)
- [ ] Los nombres de los campos en `BuildParameters` usan kebab-case (ej: `next-pool`, no `NextPool`)
- [ ] Los nombres de los campos en `ParseResponse` usan kebab-case (ej: `sentence.GetResponseField("next-pool")`)
- [ ] Los endpoints HTTP usan los verbos correctos (GET para listar, POST para crear, PUT para actualizar, DELETE para eliminar)
- [ ] La interface `IMikroTikService` y la implementación `MikroTikService` tienen los mismos métodos
- [ ] Cada método del servicio está envuelto en `try/catch` con logging
- [ ] El proyecto compila sin errores (`dotnet build`)

---

## Referencia API MikroTik

### Cómo encontrar los campos de un recurso MikroTik

1. **Desde la terminal del router**: Ejecutar el print y ver qué campos devuelve
   ```
   /ip/pool/print detail
   ```

2. **Desde la documentación oficial**:
   [https://help.mikrotik.com/docs/spaces/ROS/pages/328078/IP+Pool](https://help.mikrotik.com/docs/spaces/ROS/pages/328078/IP+Pool)

3. **Recursos comunes de MikroTik** que puedes agregar siguiendo esta misma guía:

| Recurso | Comando Base | Descripción |
|---|---|---|
| `/ip/pool` | `/ip/pool/` | Pools de direcciones IP |
| `/ip/dhcp-server` | `/ip/dhcp-server/` | Servidores DHCP |
| `/ip/dhcp-server/network` | `/ip/dhcp-server/network/` | Redes DHCP |
| `/ip/route` | `/ip/route/` | Rutas estáticas |
| `/ip/dns` | `/ip/dns/` | Configuración DNS |
| `/ip/dns/static` | `/ip/dns/static/` | Entradas DNS estáticas |
| `/queue/simple` | `/queue/simple/` | Colas simples (bandwidth) |
| `/ip/hotspot` | `/ip/hotspot/` | Hotspot |
| `/ip/hotspot/user` | `/ip/hotspot/user/` | Usuarios de Hotspot |
| `/ppp/secret` | `/ppp/secret/` | Secretos PPP (PPPoE users) |
| `/system/scheduler` | `/system/scheduler/` | Tareas programadas |
| `/system/script` | `/system/script/` | Scripts del sistema |
| `/ip/firewall/nat` | `/ip/firewall/nat/` | Reglas NAT |
| `/ip/firewall/mangle` | `/ip/firewall/mangle/` | Reglas Mangle |
| `/interface/wireless/security-profiles` | `/interface/wireless/security-profiles/` | Perfiles de seguridad WiFi |

### Patrón de comandos MikroTik

Cada recurso soporta estos subcomandos:

| Subcomando | Acción | Necesita `.id`? |
|---|---|---|
| `print` | Listar todos | ❌ |
| `add` | Crear nuevo | ❌ |
| `set` | Actualizar existente | ✅ |
| `remove` | Eliminar | ✅ |
| `get` | Obtener uno por ID | ✅ |
| `enable` | Habilitar | ✅ |
| `disable` | Deshabilitar | ✅ |

### Ejemplo de respuesta real del router

Cuando ejecutas `/ip/pool/print`, el router devuelve algo como:

```
!re
=.id=*1
=name=dhcp_pool
=ranges=192.168.1.100-192.168.1.200
=comment=Pool para clientes

!re
=.id=*2
=name=hotspot_pool
=ranges=10.0.0.10-10.0.0.254
=next-pool=overflow_pool
```

Cada bloque `!re` es un `ITikSentence`, y `=campo=valor` son los campos que lees con `GetResponseField("campo")`.

---

## Ejemplo Completo: Requests y Responses HTTP

### Listar IP Pools

```http
GET /api/mikrotik/routers/1/ip/pools
```

**Response (200 OK):**
```json
{
    "status": "success",
    "message": "Se encontraron 2 IP pools",
    "data": [
        {
            "id": "*1",
            "name": "dhcp_pool",
            "ranges": "192.168.1.100-192.168.1.200",
            "nextPool": null,
            "comment": "Pool para clientes"
        },
        {
            "id": "*2",
            "name": "hotspot_pool",
            "ranges": "10.0.0.10-10.0.0.254",
            "nextPool": "overflow_pool",
            "comment": null
        }
    ],
    "errors": null,
    "pagination": null,
    "timestamp": "2026-03-13T14:00:00Z"
}
```

### Crear IP Pool

```http
POST /api/mikrotik/routers/1/ip/pools
Content-Type: application/json

{
    "name": "nuevo_pool",
    "ranges": "172.16.0.100-172.16.0.200",
    "nextPool": "dhcp_pool",
    "comment": "Pool de respaldo"
}
```

**Response (200 OK):**
```json
{
    "status": "success",
    "message": "IP Pool creado exitosamente",
    "data": {
        "id": "*3",
        "name": "nuevo_pool",
        "ranges": "172.16.0.100-172.16.0.200",
        "nextPool": "dhcp_pool",
        "comment": "Pool de respaldo"
    },
    "errors": null,
    "timestamp": "2026-03-13T14:05:00Z"
}
```

### Actualizar IP Pool

```http
PUT /api/mikrotik/routers/1/ip/pools
Content-Type: application/json

{
    "id": "*3",
    "ranges": "172.16.0.50-172.16.0.250",
    "comment": "Pool de respaldo ampliado"
}
```

### Eliminar IP Pool

```http
DELETE /api/mikrotik/routers/1/ip/pools
Content-Type: application/json

{
    "id": "*3"
}
```

### Response de Error

```json
{
    "status": "error",
    "message": "Error creando IP pool: failure: already have such name",
    "data": null,
    "errors": {
        "errorType": "CommandFailed"
    },
    "timestamp": "2026-03-13T14:10:00Z"
}
```

---

## Resumen Visual Rápido

```
¿Qué quieres agregar?
         │
         ▼
    Crear Modelos           → OperationModels.cs     (Request + Response)
         │
         ▼
    Crear Operaciones       → StandardOperations.cs  (Command + Build + Parse)
         │
         ▼
    Agregar al Servicio     → IMikroTikService.cs    (firmas)
                            → MikroTikService.cs     (implementación)
         │
         ▼
    Agregar Endpoints       → MikroTikController.cs  (rutas HTTP)
         │
         ▼
    ¿Nuevo Servicio?
    ├── NO → ¡Listo! 🎉
    └── SÍ → DependencyContainer.cs (registrar en DI)
```
