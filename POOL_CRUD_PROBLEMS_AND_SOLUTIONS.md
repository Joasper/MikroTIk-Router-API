# 🔧 Pool CRUD — Problemas y Soluciones

## El Problema: `/ip/pool/add` retorna solo el ID

### ¿Qué pasa exactamente?

Cuando MikroTik ejecuta el comando `/ip/pool/add`, el router responde con **únicamente el ID del nuevo recurso**, por ejemplo:

```
*6
```

No devuelve el objeto completo (nombre, ranges, etc.).

Sin embargo, tu `ParseResponse` en `CreateIpPoolOperation` intenta leer campos como `name`, `ranges`, etc., que **no existen** en esa respuesta → **Exception**.

```
❌ response.GetResponseField("name")   → campo no encontrado → lanza excepción
❌ response.GetResponseField("ranges") → campo no encontrado → lanza excepción
```

Lo mismo aplica para:
- `/ip/pool/set` (Update) → retorna respuesta **vacía**.
- `/ip/pool/remove` (Delete) → retorna respuesta **vacía**.

---

## ✅ Solución: Two-Step (Crear + GetById)

La solución estándar con la API de MikroTik es:

1. Ejecutar el comando (`add` / `set` / `remove`)
2. Extraer el ID de la respuesta del `add`, o usar el ID que ya tenías para `set`
3. Hacer una segunda consulta `/ip/pool/print .id=*6` para obtener el objeto completo

---

## Paso 1 — Añadir `GetIpPoolByIdQuery` en `StandardOperations.cs`

Dentro de la clase `CreateFirewallRuleOperation` (o donde tengas los pools), añade:

```csharp
// ========== GET BY ID ==========
public class GetIpPoolByIdQuery : IMikroTikQuery<IpPoolResponse?>
{
    private readonly string _id;

    public GetIpPoolByIdQuery(string id)
    {
        _id = id;
    }

    // MikroTik acepta filtros en el command como ?=.id=*6
    public string Command => $"/ip/pool/print ?.id={_id}";

    public IpPoolResponse? ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var sentence = responses.FirstOrDefault();
        if (sentence == null) return null;

        return new IpPoolResponse
        {
            Id    = sentence.GetResponseField(".id"),
            Name  = sentence.GetResponseField("name"),
            Ranges = sentence.GetResponseField("ranges"),
            NextPool = sentence.GetOptionalField("next-pool"),
            Comment  = sentence.GetOptionalField("comment")
        };
    }
}
```

> **Nota:** Algunos clientes de MikroTik (`tik4net`) usan el formato de filtro diferente. Si `Command => $"/ip/pool/print ?.id={_id}"` no funciona, ver sección "Alternativo" abajo.

---

## Paso 2 — Modificar `CreateIpPoolOperation.ParseResponse`

Actualmente la implementación intenta leer todos los campos. Cámbiala para que **solo extraiga el ID**:

```csharp
public class CreateIpPoolOperation : IMikroTikOperation<CreateIpPoolRequest, string>
//                                            cambia TResponse a →  ↑↑↑↑↑↑ string
{
    public string Command => "/ip/pool/add";

    public Dictionary<string, string> BuildParameters(CreateIpPoolRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"]   = request.Name,
            ["ranges"] = request.Ranges,
        };

        if (!string.IsNullOrEmpty(request.NextPool))
            parameters["next-pool"] = request.NextPool;

        if (!string.IsNullOrEmpty(request.Comment))
            parameters["comment"] = request.Comment;

        return parameters;
    }

    // El router responde: "*6" — solo guardamos ese string
    public string ParseResponse(ITikSentence response)
    {
        // tik4net expone el ID creado en la sentence de respuesta
        // dependiendo del cliente puede estar en el campo ".id" o como valor directo
        return response.GetResponseField(".ret");
        // Si el campo no es ".ret", prueba:  response.ToString() o response.Words.FirstOrDefault()
    }
}
```

> ⚠️ El campo exacto depende del cliente C# de MikroTik que uses (`tik4net` usa `.ret` para los IDs de retorno en comandos `add`).

---

## Paso 3 — Modificar `CreateIpPoolAsync` en `MikroTikService.cs`

```csharp
public async Task<ApiResponse<IpPoolResponse>> CreateIpPoolAsync(int routerId, CreateIpPoolRequest createPoolRequest)
{
    try
    {
        // 1. Ejecutar el add — TResponse ahora es string (el ID)
        var operation = new CreateIpPoolOperation();
        var result = await _connectionManager.ExecuteOperationAsync(routerId, operation, createPoolRequest);

        if (!result.IsSuccess)
        {
            return ApiResponse<IpPoolResponse>.Error(
                $"Error creando IP pool: {result.ErrorMessage}",
                new { ErrorType = result.ErrorType.ToString() }
            );
        }

        // 2. Usar el ID retornado para buscar el objeto completo
        var createdId = result.Data!;  // e.g. "*6"
        var getByIdQuery = new GetIpPoolByIdQuery(createdId);
        var getResult = await _connectionManager.ExecuteQueryAsync(routerId, getByIdQuery);

        if (!getResult.IsSuccess || getResult.Data == null)
        {
            return ApiResponse<IpPoolResponse>.Error("Pool creado pero no se pudo recuperar el objeto");
        }

        return ApiResponse<IpPoolResponse>.Success(getResult.Data, "IP Pool creado exitosamente");
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Error creando IP pool en router {RouterId}", routerId);
        return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {e.Message}");
    }
}
```

---

## Paso 4 — GetById (endpoint dedicado)

Para un endpoint `GET /api/mikrotik/{routerId}/pools/{id}`:

### En `IMikroTikService.cs`:
```csharp
Task<ApiResponse<IpPoolResponse>> GetIpPoolByIdAsync(int routerId, string poolId);
```

### En `MikroTikService.cs`:
```csharp
public async Task<ApiResponse<IpPoolResponse>> GetIpPoolByIdAsync(int routerId, string poolId)
{
    try
    {
        var query = new GetIpPoolByIdQuery(poolId);
        var result = await _connectionManager.ExecuteQueryAsync(routerId, query);

        if (!result.IsSuccess)
            return ApiResponse<IpPoolResponse>.Error($"Error obteniendo pool: {result.ErrorMessage}");

        if (result.Data == null)
            return ApiResponse<IpPoolResponse>.NotFound($"Pool con id '{poolId}' no encontrado");

        return ApiResponse<IpPoolResponse>.Success(result.Data, "Pool encontrado");
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Error obteniendo IP pool {PoolId} en router {RouterId}", poolId, routerId);
        return ApiResponse<IpPoolResponse>.Error($"Error inesperado: {e.Message}");
    }
}
```

### En `MikroTikController.cs`:
```csharp
[HttpGet("{routerId}/pools/{poolId}")]
public async Task<IActionResult> GetIpPoolByIdAsync(int routerId, string poolId)
{
    var response = await _mikroTikService.GetIpPoolByIdAsync(routerId, poolId);
    return HandleResponse(response);
}
```

---

## Paso 5 — Fix Update (`UpdateIpPoolAsync`)

El mismo problema aplica para Update. `/ip/pool/set` retorna vacío.

```csharp
public class UpdateIpPoolOperation : IMikroTikOperation<UpdateIpPoolRequest, string>
//                                                                            ↑ string, no IpPoolResponse
{
    public string Command => "/ip/pool/set";

    public Dictionary<string, string> BuildParameters(UpdateIpPoolRequest request) { /* igual que antes */ }

    // set retorna vacío, no hay nada útil — devolvemos el ID que ya teníamos
    public string ParseResponse(ITikSentence response) => request.Id;
    // ⚠️ No puedes acceder a `request` aquí directamente. Ver alternativa abajo.
}
```

**Alternativa más limpia para Update** — Pasar el ID directamente al servicio:

```csharp
// En UpdateIpPoolAsync, después de ejecutar el set exitosamente:
var getByIdQuery = new GetIpPoolByIdQuery(updateIpPoolRequest.Id);
var getResult = await _connectionManager.ExecuteQueryAsync(routerId, getByIdQuery);
return ApiResponse<IpPoolResponse>.Success(getResult.Data!, "Pool actualizado");
```

---

## Paso 6 — Fix Delete (`DeleteIpPoolAsync`)

Para Delete no tiene sentido retornar el objeto (ya no existe). Cambia el tipo de retorno:

```csharp
// Opción A: Retornar solo un mensaje de éxito con el ID eliminado
public async Task<ApiResponse<object>> DeleteIpPoolAsync(int routerId, DeleteIpPoolRequest request)

// Opción B: Mantener IpPoolResponse pero con solo el Id poblado
return ApiResponse<IpPoolResponse>.Success(
    new IpPoolResponse { Id = request.Id },
    "IP Pool eliminado exitosamente"
);
```

La **Opción B** es más sencilla y no rompe la firma de los métodos existentes.

---

## Resumen del Flujo

```
POST /pools        → /ip/pool/add  → retorna "*6" → GetById("*6") → IpPoolResponse ✅
GET  /pools        → /ip/pool/print             → lista completa              ✅
GET  /pools/{id}   → /ip/pool/print ?.id={id}   → objeto único               ✅ (NUEVO)
PUT  /pools/{id}   → /ip/pool/set   → retorna vacío → GetById(id) → IpPoolResponse ✅
DELETE /pools/{id} → /ip/pool/remove → retorna vacío → { Id = id }            ✅
```

---

## Alternativo: Filtrar por ID en tik4net

Si el cliente `tik4net` no soporta filtros en el `Command` string directamente, puedes usar `GetAllIpPools` y filtrar en C#:

```csharp
public async Task<ApiResponse<IpPoolResponse>> GetIpPoolByIdAsync(int routerId, string poolId)
{
    var allPools = await GetAllIpPoolsAsync(routerId);
    if (!allPools.IsSuccess) return ApiResponse<IpPoolResponse>.Error(allPools.Message);

    var pool = allPools.Data?.FirstOrDefault(p => p.Id == poolId);
    if (pool == null) return ApiResponse<IpPoolResponse>.NotFound($"Pool '{poolId}' no encontrado");

    return ApiResponse<IpPoolResponse>.Success(pool, "Pool encontrado");
}
```

> Este enfoque es menos eficiente (trae todos los pools) pero es 100% compatible con cualquier versión del cliente MikroTik.
