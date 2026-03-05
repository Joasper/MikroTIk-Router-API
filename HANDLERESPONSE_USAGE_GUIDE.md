# Guía de Uso: HandleResponse

## ?? Concepto

El `HandleResponse<T>` proporciona una estructura estandarizada para las respuestas de la API, permitiendo que **siempre se retorne HTTP 200 OK** pero con un status interno que indica el resultado real de la operación.

---

## ??? Estructura de ApiResponse

```json
{
  "status": "success | error | validation_error | not_found | unauthorized | forbidden",
  "message": "Mensaje descriptivo",
  "data": { }, // El objeto o lista de datos (puede ser null)
  "errors": { }, // Detalles de errores (opcional)
  "pagination": { // Opcional, solo para listas paginadas
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalRecords": 50,
    "hasPrevious": false,
    "hasNext": true
  },
  "timestamp": "2024-02-15T10:30:00Z"
}
```

---

## ?? Cómo Usar en Servicios

### 1. Inyectar HandleResponse en el Constructor

```csharp
public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HandleResponse<OrganizationDTO> _handleResponse;
    private readonly HandleResponse<IEnumerable<OrganizationDTO>> _handleResponseList;
    private readonly HandleResponse<bool> _handleResponseBool;

    public OrganizationService(
        IOrganizationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        
        // Inicializar handlers según el tipo de retorno
        _handleResponse = new HandleResponse<OrganizationDTO>();
        _handleResponseList = new HandleResponse<IEnumerable<OrganizationDTO>>();
        _handleResponseBool = new HandleResponse<bool>();
    }
}
```

### 2. Usar en Métodos del Servicio

#### ? Operación Exitosa (GET)
```csharp
public async Task<ApiResponse<OrganizationDTO>> GetOrganizationByIdAsync(int id)
{
    try
    {
        var organization = await _repository.GetByIdAsync(id);
        
        if (organization == null || organization.DeletedAt != null)
        {
            return _handleResponse.NotFound("Organización no encontrada");
        }

        var dto = MapToDto(organization);
        return _handleResponse.Success(dto, "Organización obtenida exitosamente");
    }
    catch (Exception ex)
    {
        return _handleResponse.Error($"Error al obtener la organización: {ex.Message}");
    }
}
```

#### ? Operación Exitosa con Lista
```csharp
public async Task<ApiResponse<IEnumerable<OrganizationDTO>>> GetAllOrganizationsAsync()
{
    try
    {
        var organizations = await _repository.GetAllAsync();
        var dtos = organizations.Select(MapToDto).ToList();
        
        return _handleResponseList.Success(
            dtos, 
            $"Se encontraron {dtos.Count} organizaciones"
        );
    }
    catch (Exception ex)
    {
        return _handleResponseList.Error($"Error al obtener organizaciones: {ex.Message}");
    }
}
```

#### ? Operación Exitosa con Paginación
```csharp
public async Task<ApiResponse<IEnumerable<OrganizationDTO>>> GetOrganizationsPaginatedAsync(
    int page, int pageSize)
{
    try
    {
        var totalRecords = await _repository.CountAsync();
        var organizations = await _repository.GetPaginatedAsync(page, pageSize);
        var dtos = organizations.Select(MapToDto).ToList();
        
        var pagination = new PaginationMetadata(page, pageSize, totalRecords);
        
        return _handleResponseList.Success(
            dtos,
            $"Página {page} de {pagination.TotalPages}",
            pagination
        );
    }
    catch (Exception ex)
    {
        return _handleResponseList.Error($"Error al obtener organizaciones: {ex.Message}");
    }
}
```

#### ? Operación Exitosa (CREATE/UPDATE)
```csharp
public async Task<ApiResponse<OrganizationDTO>> CreateOrganizationAsync(CreateOrganizationDTO dto)
{
    try
    {
        // Validaciones de negocio
        var exists = await _repository.GetByExpressionAsync(o => o.Name == dto.Name);
        if (exists != null)
        {
            return _handleResponse.ValidationError(
                "Ya existe una organización con ese nombre",
                new { Name = "El nombre ya está en uso" }
            );
        }

        var organization = new Organizations
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone
        };

        _repository.Add(organization);
        await _unitOfWork.SaveChangesAsync();

        var resultDto = MapToDto(organization);
        return _handleResponse.Success(resultDto, "Organización creada exitosamente");
    }
    catch (Exception ex)
    {
        return _handleResponse.Error($"Error al crear organización: {ex.Message}");
    }
}
```

#### ? Operación Exitosa (DELETE)
```csharp
public async Task<ApiResponse<bool>> DeleteOrganizationAsync(int id)
{
    try
    {
        var organization = await _repository.GetByIdAsync(id);
        if (organization == null)
        {
            return _handleResponseBool.NotFound("Organización no encontrada");
        }

        // Soft delete
        organization.DeletedAt = DateTime.UtcNow;
        _repository.UpdateAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        return _handleResponseBool.Success(true, "Organización eliminada exitosamente");
    }
    catch (Exception ex)
    {
        return _handleResponseBool.Error($"Error al eliminar: {ex.Message}");
    }
}
```

#### ? Error de Validación
```csharp
return _handleResponse.ValidationError(
    "Los datos proporcionados no son válidos",
    new 
    { 
        Name = "El nombre es requerido",
        Email = "El formato de email es inválido"
    }
);
```

#### ? Recurso No Encontrado
```csharp
return _handleResponse.NotFound("Organización no encontrada");
```

#### ? No Autorizado
```csharp
return _handleResponse.Unauthorized("Debe iniciar sesión para realizar esta acción");
```

#### ? Acceso Prohibido
```csharp
return _handleResponse.Forbidden("No tiene permisos para realizar esta acción");
```

#### ? Error General
```csharp
return _handleResponse.Error("Error al procesar la solicitud", new { Detail = ex.Message });
```

---

## ?? Cómo Usar en Controladores

### Opción 1: Siempre Retornar 200 OK (Recomendado para SPA)

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(IOrganizationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _service.GetAllOrganizationsAsync();
        // Siempre retorna 200 OK, el frontend decide según el "status"
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationDTO dto)
    {
        if (!ModelState.IsValid)
            return Ok(BuildValidationError(ModelState));

        var response = await _service.CreateOrganizationAsync(dto);
        return Ok(response);
    }
}
```

### Opción 2: Retornar Códigos HTTP Apropiados (REST Estricto)

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(IOrganizationService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _service.GetOrganizationByIdAsync(id);
        
        return response.Status switch
        {
            "success" => Ok(response),
            "not_found" => NotFound(response),
            "error" => StatusCode(500, response),
            _ => StatusCode(500, response)
        };
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError(ModelState));

        var response = await _service.CreateOrganizationAsync(dto);
        
        return response.Status switch
        {
            "success" => CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response),
            "validation_error" => BadRequest(response),
            "error" => StatusCode(500, response),
            _ => StatusCode(500, response)
        };
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError(ModelState));

        var response = await _service.UpdateOrganizationAsync(id, dto);
        
        return response.Status switch
        {
            "success" => Ok(response),
            "not_found" => NotFound(response),
            "validation_error" => BadRequest(response),
            "error" => StatusCode(500, response),
            _ => StatusCode(500, response)
        };
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _service.DeleteOrganizationAsync(id);
        
        return response.Status switch
        {
            "success" => Ok(response),
            "not_found" => NotFound(response),
            "error" => StatusCode(500, response),
            _ => StatusCode(500, response)
        };
    }

    private static object BuildValidationError(ModelStateDictionary modelState)
    {
        return new
        {
            Status = "validation_error",
            Message = "Datos de entrada inválidos",
            Errors = modelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage)),
            Timestamp = DateTime.UtcNow
        };
    }
}
```

---

## ?? Ejemplo de Respuesta del Controlador

### GET /api/organizations (Éxito)
```json
{
  "status": "success",
  "message": "Se encontraron 3 organizaciones",
  "data": [
    {
      "id": 1,
      "name": "Empresa A",
      "email": "contacto@empresaa.com",
      "phone": "123456789",
      "createdAt": "2024-02-15T10:00:00Z",
      "license": {
        "id": 1,
        "key": "ABC-123-XYZ",
        "type": "Enterprise",
        "endDate": "2025-12-31T23:59:59Z",
        "isActive": true,
        "isExpired": false,
        "daysRemaining": 320
      }
    }
  ],
  "errors": null,
  "pagination": null,
  "timestamp": "2024-02-15T15:30:00Z"
}
```

### GET /api/organizations/999 (No Encontrado)
```json
{
  "status": "not_found",
  "message": "Organización no encontrada",
  "data": null,
  "errors": null,
  "pagination": null,
  "timestamp": "2024-02-15T15:30:00Z"
}
```

### POST /api/organizations (Error de Validación)
```json
{
  "status": "validation_error",
  "message": "Ya existe una organización con ese nombre",
  "data": null,
  "errors": {
    "name": "El nombre ya está en uso"
  },
  "pagination": null,
  "timestamp": "2024-02-15T15:30:00Z"
}
```

### POST /api/organizations (Error del Servidor)
```json
{
  "status": "error",
  "message": "Error al crear la organización: Connection timeout",
  "data": null,
  "errors": null,
  "pagination": null,
  "timestamp": "2024-02-15T15:30:00Z"
}
```

---

## ?? Ventajas de Este Enfoque

### ? Para el Frontend
1. **Manejo simplificado**: Siempre recibe 200 OK
2. **Consistencia**: Todas las respuestas tienen la misma estructura
3. **Información rica**: Status + message + data + errors
4. **Timestamp**: Para debugging y sincronización

### ? Para el Backend
1. **Código limpio**: Sin múltiples return statements con diferentes códigos HTTP
2. **Reutilizable**: Mismo handler para todos los servicios
3. **Extensible**: Fácil agregar nuevos tipos de status
4. **Type-safe**: Genérico con tipado fuerte

---

## ?? Comparación: Con vs Sin HandleResponse

### ? Antes (Sin HandleResponse)
```csharp
public async Task<OrganizationDTO> GetOrganizationByIdAsync(int id)
{
    var org = await _repository.GetByIdAsync(id);
    if (org == null)
        throw new NotFoundException("No encontrado"); // ? Excepción
    
    return MapToDto(org);
}

// En el controlador:
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    try
    {
        var result = await _service.GetOrganizationByIdAsync(id);
        return Ok(result); // ? Sin estructura estándar
    }
    catch (NotFoundException ex)
    {
        return NotFound(ex.Message); // ? Código HTTP diferente
    }
    catch (Exception ex)
    {
        return StatusCode(500, ex.Message); // ? Código HTTP diferente
    }
}
```

### ? Después (Con HandleResponse)
```csharp
public async Task<ApiResponse<OrganizationDTO>> GetOrganizationByIdAsync(int id)
{
    try
    {
        var org = await _repository.GetByIdAsync(id);
        if (org == null)
            return _handleResponse.NotFound("Organización no encontrada");

        var dto = MapToDto(org);
        return _handleResponse.Success(dto, "Organización obtenida exitosamente");
    }
    catch (Exception ex)
    {
        return _handleResponse.Error($"Error: {ex.Message}");
    }
}

// En el controlador (Opción simple):
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var response = await _service.GetOrganizationByIdAsync(id);
    return Ok(response); // ? Siempre 200 OK
}

// O con códigos HTTP apropiados:
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var response = await _service.GetOrganizationByIdAsync(id);
    
    return response.Status switch
    {
        "success" => Ok(response),
        "not_found" => NotFound(response),
        _ => StatusCode(500, response)
    };
}
```

---

## ?? Patrones de Uso Comunes

### Pattern 1: CRUD Básico
```csharp
// CREATE
var response = _handleResponse.Success(dto, "Creado exitosamente");

// READ
var response = _handleResponse.Success(dto, "Obtenido exitosamente");

// UPDATE
var response = _handleResponse.Success(dto, "Actualizado exitosamente");

// DELETE
var response = _handleResponseBool.Success(true, "Eliminado exitosamente");
```

### Pattern 2: Validaciones
```csharp
// Validación simple
if (string.IsNullOrEmpty(dto.Name))
    return _handleResponse.ValidationError("El nombre es requerido", null);

// Validación múltiple
if (!IsValid(dto))
{
    return _handleResponse.ValidationError(
        "Datos inválidos",
        new 
        { 
            Name = "Requerido",
            Email = "Formato inválido"
        }
    );
}
```

### Pattern 3: Autenticación/Autorización
```csharp
// Usuario no autenticado
if (!User.Identity.IsAuthenticated)
    return _handleResponse.Unauthorized("Debe iniciar sesión");

// Sin permisos
if (!HasPermission(user, "MANAGE_ROUTERS"))
    return _handleResponse.Forbidden("No tiene permisos para esta acción");
```

### Pattern 4: Búsquedas
```csharp
// Búsqueda sin resultados
var results = await _repository.SearchAsync(criteria);
if (!results.Any())
{
    return _handleResponseList.Success(
        new List<OrganizationDTO>(),
        "No se encontraron resultados"
    );
}

return _handleResponseList.Success(results, $"Se encontraron {results.Count()} resultados");
```

---

## ?? Mejores Prácticas

### ? DO's
- ? Siempre usar try-catch en los servicios
- ? Retornar mensajes descriptivos
- ? Incluir detalles en los errores de validación
- ? Usar el status apropiado (success, error, not_found, etc.)
- ? Agregar timestamp automáticamente
- ? Usar PaginationMetadata para listas grandes

### ? DON'Ts
- ? No lanzar excepciones desde el servicio (usar HandleResponse)
- ? No retornar null (siempre retornar ApiResponse)
- ? No usar strings genéricos como "Error" (ser descriptivo)
- ? No exponer detalles técnicos en mensajes al usuario
- ? No mezclar códigos HTTP con status interno inconsistentemente

---

## ?? Testing de Servicios

```csharp
[Fact]
public async Task CreateOrganization_WithValidData_ReturnsSuccess()
{
    // Arrange
    var dto = new CreateOrganizationDTO 
    { 
        Name = "Test Org", 
        Email = "test@org.com",
        Phone = "123456789"
    };

    // Act
    var response = await _service.CreateOrganizationAsync(dto);

    // Assert
    Assert.Equal("success", response.Status);
    Assert.NotNull(response.Data);
    Assert.Equal("Test Org", response.Data.Name);
}

[Fact]
public async Task CreateOrganization_WithDuplicateName_ReturnsValidationError()
{
    // Arrange
    var dto = new CreateOrganizationDTO { Name = "Duplicate", Email = "test@org.com" };

    // Act
    var response = await _service.CreateOrganizationAsync(dto);

    // Assert
    Assert.Equal("validation_error", response.Status);
    Assert.NotNull(response.Errors);
}
```

---

## ?? Ejemplo de Consumo en Frontend

### JavaScript/TypeScript
```typescript
async function getOrganization(id: number) {
  const response = await fetch(`/api/organizations/${id}`);
  const result = await response.json();
  
  if (result.status === 'success') {
    console.log('Organización:', result.data);
  } else if (result.status === 'not_found') {
    showNotification(result.message, 'warning');
  } else if (result.status === 'error') {
    showNotification(result.message, 'error');
  }
}

async function createOrganization(data: CreateOrganizationDTO) {
  const response = await fetch('/api/organizations', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  
  const result = await response.json();
  
  switch (result.status) {
    case 'success':
      showNotification(result.message, 'success');
      return result.data;
    case 'validation_error':
      showValidationErrors(result.errors);
      break;
    case 'error':
      showNotification(result.message, 'error');
      break;
  }
  
  return null;
}
```

---

## ?? Resumen

### En el Servicio:
```csharp
// Inicializar
private readonly HandleResponse<OrganizationDTO> _handleResponse = new();

// Usar
return _handleResponse.Success(data, "Mensaje");
return _handleResponse.Error("Mensaje de error");
return _handleResponse.NotFound("No encontrado");
return _handleResponse.ValidationError("Mensaje", errorsObject);
```

### En el Controlador (Opción Simple):
```csharp
[HttpGet]
public async Task<IActionResult> Get()
{
    var response = await _service.GetAllAsync();
    return Ok(response); // Siempre 200 OK
}
```

### En el Controlador (Opción con Códigos HTTP):
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var response = await _service.GetByIdAsync(id);
    
    return response.Status switch
    {
        "success" => Ok(response),
        "not_found" => NotFound(response),
        "validation_error" => BadRequest(response),
        "unauthorized" => Unauthorized(response),
        "forbidden" => StatusCode(403, response),
        _ => StatusCode(500, response)
    };
}
```

**Recomendación**: Usa la **Opción 1** (siempre 200 OK) si trabajas con aplicaciones SPA modernas. Usa la **Opción 2** si necesitas cumplir estrictamente con REST.
