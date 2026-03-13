# ?? Sistema de Encriptación de Passwords - MikroClean

## ?? Descripción

Las contraseñas de los routers MikroTik se almacenan **encriptadas** en la base de datos usando **AES-256** con una clave secreta configurada en `appsettings.json`.

---

## ?? Configuración

### 1. appsettings.Development.json

```json
{
  "Encryption": {
    "SecretKey": "MikroClean-Dev-Secret-Key-2024-CHANGE-THIS-IN-PRODUCTION-xyz123"
  }
}
```

### 2. appsettings.json (Producción)

```json
{
  "Encryption": {
    "SecretKey": "USE-A-VERY-STRONG-SECRET-KEY-HERE-MIN-32-CHARACTERS-RANDOM!"
  }
}
```

> ?? **IMPORTANTE**: 
> - La `SecretKey` debe tener **mínimo 32 caracteres**
> - Debe ser **única y aleatoria** en producción
> - **NO** la versiones en Git (usar Variables de Entorno o Azure Key Vault)
> - Si cambias la clave, **NO podrás desencriptar passwords existentes**

---

## ?? Generación de SecretKey Segura

### PowerShell (Windows)
```powershell
# Generar clave aleatoria de 64 caracteres
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

### C# (Consola)
```csharp
var key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
Console.WriteLine(key);
```

### Online (Uso temporal)
```
https://www.grc.com/passwords.htm (Ultra High Security Passwords)
```

---

## ?? Uso del Sistema

### 1. Crear Router (Password se encripta automáticamente)

```http
POST /api/routers
Content-Type: application/json

{
  "name": "Router Sucursal Principal",
  "ip": "192.168.1.1",
  "user": "admin",
  "password": "MiPasswordDelRouter123!",
  "model": "RB750Gr3",
  "location": "Sucursal Principal - Piso 2",
  "organizationId": 1
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Router creado exitosamente",
  "data": {
    "id": 1,
    "name": "Router Sucursal Principal",
    "ip": "192.168.1.1",
    "user": "admin",
    "isActive": true,
    "model": "RB750Gr3",
    "location": "Sucursal Principal - Piso 2",
    "organizationId": 1,
    "organizationName": "Mi Empresa S.A."
  }
}
```

> ?? **Nota**: El password `"MiPasswordDelRouter123!"` se almacena en DB como:
> ```
> EncryptedPassword: "kJ9mP2vX8qR5tY3wN6bV1cZ4sA7dF0gH=="
> ```

---

### 2. Actualizar Router (Cambiar Password)

```http
PUT /api/routers/1
Content-Type: application/json

{
  "name": "Router Sucursal Principal",
  "ip": "192.168.1.1",
  "user": "admin",
  "password": "NuevoPasswordSeguro456!",
  "model": "RB750Gr3",
  "location": "Sucursal Principal - Piso 2",
  "isActive": true
}
```

> ?? **Automático**: Al cambiar el password, el sistema:
> 1. Encripta el nuevo password
> 2. Cierra la conexión activa al router
> 3. La próxima operación usará el nuevo password

---

### 3. Probar Conexión al Router

```http
POST /api/routers/1/test
```

**Response (Exitoso):**
```json
{
  "status": "success",
  "message": "Router conectado exitosamente",
  "data": true
}
```

**Response (Fallo - Password incorrecto):**
```json
{
  "status": "error",
  "message": "Error al probar conexión: Authentication failed",
  "data": false
}
```

---

### 4. Obtener Estado de Conexión

```http
GET /api/mikrotik/routers/1/status
```

**Response:**
```json
{
  "status": "success",
  "data": {
    "routerId": 1,
    "isConnected": true,
    "lastConnected": "2024-01-15T10:30:00Z",
    "lastDisconnected": null,
    "failedAttempts": 0,
    "lastError": null,
    "latency": "00:00:00.123"
  }
}
```

---

## ?? Flujo de Encriptación/Desencriptación

### Al Crear Router:
```
Usuario envía ? Password en texto plano ? RouterService
                                           ?
                                   AesEncryptionService.Encrypt()
                                           ?
                                   Base64 encriptado
                                           ?
                                   Se guarda en DB
```

### Al Conectar a Router:
```
MikroTikConnectionManager solicita router
                ?
        RouterRepository.GetByIdAsync()
                ?
        Router con EncryptedPassword
                ?
        AesEncryptionService.Decrypt()
                ?
        Password en texto plano (en memoria)
                ?
        Se conecta al router MikroTik
```

---

## ??? Seguridad

### ? Características de Seguridad:

1. **AES-256-CBC**: Encriptación simétrica de grado militar
2. **Key Derivation**: La SecretKey se deriva usando SHA256
3. **IV Único**: Vector de inicialización derivado de la clave
4. **Base64 Storage**: Formato seguro para almacenar en DB varchar
5. **Memory Cache**: Password desencriptado solo vive en cache por 5 minutos
6. **No Logs**: Passwords nunca se loguean (ni encriptados ni en texto plano)

### ? Nunca Hagas:

```csharp
// ? MAL - Exponer password en API
public class RouterDTO
{
    public string Password { get; set; } // NUNCA!
}

// ? MAL - Loguear passwords
_logger.LogInformation("Password: {Password}", router.EncryptedPassword);

// ? MAL - Guardar password sin encriptar
router.EncryptedPassword = createDto.Password; // NUNCA!
```

### ? Siempre Haz:

```csharp
// ? BIEN - Encriptar antes de guardar
router.EncryptedPassword = _encryptionService.Encrypt(createDto.Password);

// ? BIEN - Desencriptar solo cuando se necesita
var decryptedPassword = _encryptionService.Decrypt(router.EncryptedPassword);

// ? BIEN - No exponer password en DTOs públicos
public class RouterDTO
{
    public string User { get; set; } // ? OK
    // Password no se incluye ?
}
```

---

## ?? Migración de Passwords Existentes

Si ya tienes routers con passwords en texto plano, ejecuta este script:

### Script de Migración:

```csharp
// MikroClean.Infrastructure/Scripts/EncryptExistingPasswords.cs

using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.Interfaces.UOW;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class PasswordMigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PasswordMigrationService> _logger;

    public PasswordMigrationService(
        IServiceProvider serviceProvider,
        ILogger<PasswordMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var routerRepo = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var routers = await routerRepo.GetAllAsync();
        var migrated = 0;

        foreach (var router in routers)
        {
            // Verificar si ya está encriptado
            if (!encryptionService.IsValidEncryptedText(router.EncryptedPassword))
            {
                // Está en texto plano, encriptar
                var plainPassword = router.EncryptedPassword;
                router.EncryptedPassword = encryptionService.Encrypt(plainPassword);
                routerRepo.UpdateAsync(router);
                migrated++;
            }
        }

        if (migrated > 0)
        {
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Migradas {Count} contraseñas de routers", migrated);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

---

## ?? Testing de Encriptación

### Test Manual via API:

```csharp
// 1. Crear router con password en texto plano
POST /api/routers
{
  "name": "Test Router",
  "ip": "192.168.1.100",
  "user": "admin",
  "password": "TestPassword123!",
  "organizationId": 1
}

// 2. Verificar en DB que está encriptado
// SELECT EncryptedPassword FROM Routers WHERE Id = 1
// Resultado: "kJ9mP2vX8qR5tY3wN6bV1cZ4sA7dF0gH=="

// 3. Probar conexión (usa password desencriptado internamente)
POST /api/routers/1/test

// 4. Crear un bridge (usa password desencriptado)
POST /api/mikrotik/routers/1/interfaces/bridge
{
  "name": "test-bridge",
  "comment": "Test de encriptación"
}

// Si funciona = Encriptación/Desencriptación OK ?
```

---

## ?? Troubleshooting

### Error: "Encryption:SecretKey no configurada"

**Causa**: Falta la configuración en appsettings.json

**Solución**:
```json
{
  "Encryption": {
    "SecretKey": "tu-clave-secreta-aqui-minimo-32-caracteres"
  }
}
```

---

### Error: "Error desencriptando datos. Verifique la clave secreta"

**Causa**: La SecretKey cambió después de encriptar los passwords

**Solución**:
1. Restaurar la SecretKey original, O
2. Re-encriptar todos los passwords con la nueva clave

```sql
-- Opción 1: Borrar routers de prueba y crear nuevos
DELETE FROM Routers;

-- Opción 2: Usar el script de migración con passwords en texto plano
```

---

### Error: "Authentication failed" al conectar

**Posibles causas**:
1. Password incorrecto en la creación
2. Usuario MikroTik no tiene permisos API
3. API del router no habilitada

**Verificación en MikroTik**:
```routeros
# Conectarse al router via WinBox o SSH
/ip service print
# Verificar que "api" está enabled

/user print
# Verificar que el usuario tiene permisos "api"
```

---

## ?? Variables de Entorno (Producción)

En producción, **NO guardes la SecretKey en appsettings.json**. Usa variables de entorno:

### Azure App Service:
```bash
az webapp config appsettings set \
  --name mikroclean-api \
  --resource-group mikroclean-rg \
  --settings Encryption__SecretKey="tu-clave-super-secreta-produccion"
```

### Docker:
```yaml
# docker-compose.yml
services:
  mikroclean-api:
    environment:
      - Encryption__SecretKey=${ENCRYPTION_SECRET_KEY}
```

### Kubernetes:
```yaml
# secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: mikroclean-secrets
type: Opaque
stringData:
  encryption-secret-key: "tu-clave-super-secreta-produccion"
```

```yaml
# deployment.yaml
env:
  - name: Encryption__SecretKey
    valueFrom:
      secretKeyRef:
        name: mikroclean-secrets
        key: encryption-secret-key
```

---

## ?? Diagrama de Flujo Completo

```
???????????????????????????????????????????????????????????
?  1. Usuario crea router via API                         ?
?     POST /api/routers                                   ?
?     Body: { password: "texto-plano" }                   ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  2. RouterService recibe CreateRouterDTO                ?
?     Password: "MiPassword123!"                          ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  3. AesEncryptionService.Encrypt()                      ?
?     Input:  "MiPassword123!"                            ?
?     Output: "kJ9mP2vX8qR5tY3wN6bV1cZ4sA7dF0gH=="       ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  4. Se guarda en DB                                     ?
?     EncryptedPassword: "kJ9mP2...=="                    ?
???????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????
?  5. Usuario ejecuta operación MikroTik                  ?
?     POST /api/mikrotik/routers/1/interfaces/bridge      ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  6. MikroTikConnectionManager necesita conectar         ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  7. GetRouterWithDecryptedPasswordAsync()               ?
?     Lee router de DB: EncryptedPassword = "kJ9m..."     ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  8. AesEncryptionService.Decrypt()                      ?
?     Input:  "kJ9mP2vX8qR5tY3wN6bV1cZ4sA7dF0gH=="       ?
?     Output: "MiPassword123!"                            ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  9. Cache por 5 minutos (en memoria)                    ?
?     Key: "router_info_1"                                ?
?     Value: Router con password desencriptado            ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  10. MikroTikClient.ConnectAsync()                      ?
?      connection.Open(ip, port, user, password)          ?
?      Password en texto plano (solo en memoria)          ?
???????????????????????????????????????????????????????????
                     ?
???????????????????????????????????????????????????????????
?  11. Conexión establecida ?                            ?
?      Bridge creado exitosamente                         ?
???????????????????????????????????????????????????????????
```

---

## ?? Código de Ejemplo en C#

### Crear Router desde código:

```csharp
public class MiServicio
{
    private readonly IRouterService _routerService;

    public async Task<int> CrearRouterAsync()
    {
        var createDto = new CreateRouterDTO
        {
            Name = "Router Principal",
            Ip = "192.168.1.1",
            User = "admin",
            Password = "MiPasswordSeguro123!", // ? Texto plano
            OrganizationId = 1
        };

        var response = await _routerService.CreateRouterAsync(createDto);

        if (response.Status == ResponseStatus.Success)
        {
            // Password ya está encriptado en DB ?
            return response.Data!.Id;
        }

        throw new Exception(response.Message);
    }
}
```

### Conectarse y ejecutar operación:

```csharp
public class MiServicio
{
    private readonly IMikroTikService _mikroTikService;

    public async Task CrearBridgeAsync(int routerId)
    {
        var request = new CreateBridgeRequest
        {
            Name = "bridge-lan",
            Comment = "Bridge principal"
        };

        // Internamente se desencripta el password automáticamente ?
        var response = await _mikroTikService.CreateBridgeAsync(routerId, request);

        if (response.Status == ResponseStatus.Success)
        {
            Console.WriteLine($"Bridge creado: {response.Data!.Name}");
        }
    }
}
```

---

## ?? Base de Datos

### Tabla Routers:

| Campo | Tipo | Ejemplo |
|-------|------|---------|
| Id | int | 1 |
| Name | varchar(200) | Router Principal |
| Ip | varchar(50) | 192.168.1.1 |
| User | varchar(50) | admin |
| **EncryptedPassword** | **varchar(500)** | **kJ9mP2vX8qR5...==** |
| IsActive | bit | 1 |
| OrganizationId | int | 1 |

> ?? **Nota**: El campo `EncryptedPassword` siempre contiene Base64, nunca texto plano

---

## ?? Rotación de Claves (Key Rotation)

Si necesitas cambiar la SecretKey:

### Proceso:

1. **Exportar passwords actuales**:
```sql
SELECT Id, EncryptedPassword 
INTO RouterPasswordsBackup
FROM Routers;
```

2. **Crear servicio de re-encriptación**:
```csharp
public class KeyRotationService
{
    private readonly IEncryptionService _oldEncryption; // Con clave vieja
    private readonly IEncryptionService _newEncryption; // Con clave nueva

    public async Task RotateKeysAsync()
    {
        var routers = await _routerRepo.GetAllAsync();
        
        foreach (var router in routers)
        {
            // Desencriptar con clave vieja
            var plainPassword = _oldEncryption.Decrypt(router.EncryptedPassword);
            
            // Re-encriptar con clave nueva
            router.EncryptedPassword = _newEncryption.Encrypt(plainPassword);
            
            _routerRepo.UpdateAsync(router);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
}
```

3. **Actualizar appsettings** con nueva SecretKey
4. **Reiniciar aplicación**

---

## ?? Best Practices

### ? DO:
- Usa claves de **mínimo 32 caracteres** aleatorios
- Almacena la SecretKey en **Azure Key Vault** o similar en producción
- Rota la SecretKey cada **6-12 meses**
- Implementa **audit logging** para cambios de passwords
- Valida que el password cumple requisitos antes de encriptar
- Usa **HTTPS** siempre en producción

### ? DON'T:
- No versiones la SecretKey en Git
- No uses la misma clave en Dev y Producción
- No expongas passwords en logs o APIs
- No almacenes passwords en texto plano nunca
- No compartas la SecretKey por email/chat

---

## ?? Auditoría de Cambios de Password

Registra en `AuditLog` cuando se cambia password:

```csharp
// En RouterService.UpdateRouterAsync()

if (!string.IsNullOrEmpty(updateDto.Password))
{
    router.EncryptedPassword = _encryptionService.Encrypt(updateDto.Password);
    
    // Registrar en audit log
    var auditLog = new AuditLog
    {
        EntityType = "Router",
        EntityId = router.Id,
        Action = "PasswordChanged",
        UserId = currentUserId,
        Timestamp = DateTime.UtcNow,
        Details = $"Password actualizado para router {router.Name}"
    };
    
    _auditLogRepo.Add(auditLog);
}
```

---

## ?? Referencias

- [AES-256 Encryption](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard)
- [OWASP Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [Azure Key Vault](https://learn.microsoft.com/azure/key-vault/)
- [.NET Data Protection API](https://learn.microsoft.com/aspnet/core/security/data-protection/)
