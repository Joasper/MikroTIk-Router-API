# Usuario SuperAdmin por Defecto

## ?? Credenciales de Acceso

Al iniciar la aplicación por primera vez, se crea automáticamente un usuario **SuperAdmin** con las siguientes credenciales:

### Credenciales
```
Username: superadmin
Email: admin@mikroclean.com
Password: Admin123!
```

### Características
- ? **Rol**: SuperAdmin (acceso completo al sistema)
- ? **Sin Organización**: No pertenece a ninguna organización
- ? **Permisos**: Todos los permisos del sistema
- ? **Estado**: Activo

---

## ?? NOTA IMPORTANTE SOBRE EL HASH

El hash de BCrypt en `SeedData.cs` es un **placeholder**. Al ejecutar la aplicación por primera vez:

1. **Opción A (Recomendada)**: Eliminar el usuario del seed y crearlo manualmente
   - Comentar o eliminar la sección de User del SeedData
   - Usar el endpoint `/api/organizations/with-admin` para crear tu primer admin
   
2. **Opción B**: Generar el hash correcto
   - El hash actual es un placeholder que NO funcionará
   - Necesitas generar el hash real usando BCrypt con la contraseña "Admin123!"
   - Reemplazar en SeedData.cs

### Generar Hash Correcto

Puedes usar este código C# para generar el hash:

```csharp
using MikroClean.Infrastructure.Security;

var passwordHasher = new BCryptPasswordHasher();
var hash = passwordHasher.HashPassword("Admin123!");
Console.WriteLine($"Hash: {hash}");
// Copiar este hash y reemplazarlo en SeedData.cs
```

---

## ?? Primer Acceso (Opción Recomendada)

### En lugar de usar el seed, crea tu primer admin así:

**Endpoint**: `POST http://localhost:5000/api/organizations/with-admin`

**Request Body**:
```json
{
  "organizationName": "Administración del Sistema",
  "organizationEmail": "admin@mikroclean.com",
  "organizationPhone": "+000000000",
  "adminUsername": "superadmin",
  "adminEmail": "admin@mikroclean.com",
  "adminPassword": "Admin123!"
}
```

Esto creará:
- ? Una organización para administración
- ? Una licencia trial de 30 días
- ? Un usuario admin con la contraseña correctamente hasheada
- ? Rol "Administrador" automático

### 2. Luego hacer Login

**Endpoint**: `POST http://localhost:5000/api/auth/login`

**Request Body**:
```json
{
  "usernameOrEmail": "superadmin",
  "password": "Admin123!"
}
```

---

## ?? ¿Qué puede hacer el SuperAdmin?

El SuperAdmin tiene acceso completo a:

### Organizaciones
- ? Ver todas las organizaciones
- ? Crear organizaciones con admin
- ? Actualizar organizaciones
- ? Eliminar organizaciones

### Licencias
- ? Ver todas las licencias
- ? Crear licencias
- ? Actualizar licencias
- ? Eliminar licencias
- ? Ver licencias disponibles
- ? Ver licencias expiradas

### Usuarios
- ? Ver todos los usuarios del sistema
- ? Crear usuarios
- ? Actualizar usuarios
- ? Eliminar usuarios
- ? Gestionar roles

### Routers
- ? Ver todos los routers
- ? Gestionar routers de cualquier organización
- ? Ejecutar operaciones en routers

### Logs y Auditoría
- ? Ver logs de auditoría
- ? Ver actividad del sistema

---

## ?? Cambiar la Contraseña

Es **altamente recomendable** cambiar la contraseña por defecto.

**Endpoint**: `POST http://localhost:5000/api/users/{userId}/change-password`

**Request Body**:
```json
{
  "currentPassword": "Admin123!",
  "newPassword": "NuevaContraseñaSegura123!@#"
}
```

---

## ?? Seguridad

### Recomendaciones
1. ?? **El hash en SeedData es un placeholder y NO funcionará**
2. ? **Usa el endpoint `/api/organizations/with-admin`** para crear tu primer admin
3. ?? **Cambia la contraseña** después del primer acceso
4. ?? **Habilita HTTPS** en producción
5. ?? **Audita las acciones** del SuperAdmin regularmente

### Rotación de Contraseñas
- Cambiar contraseña cada 90 días
- Usar contraseñas complejas (mínimo 12 caracteres)
- No reutilizar contraseñas anteriores

---

## ?? Para Desarrollo

Si estás en desarrollo y quieres un usuario rápido sin seed:

### Opción 1: Crear con Postman/Insomnia
```http
POST http://localhost:5000/api/organizations/with-admin
Content-Type: application/json

{
  "organizationName": "Sistema",
  "organizationEmail": "admin@mikroclean.com",
  "organizationPhone": "+000",
  "adminUsername": "superadmin",
  "adminEmail": "admin@mikroclean.com",
  "adminPassword": "Admin123!"
}
```

### Opción 2: Crear con cURL
```bash
curl -X POST http://localhost:5000/api/organizations/with-admin \
  -H "Content-Type: application/json" \
  -d '{
    "organizationName": "Sistema",
    "organizationEmail": "admin@mikroclean.com",
    "organizationPhone": "+000",
    "adminUsername": "superadmin",
    "adminEmail": "admin@mikroclean.com",
    "adminPassword": "Admin123!"
  }'
```

---

## ?? Resumen

**Método Recomendado**:
1. ? NO usar el seed de usuario (el hash es placeholder)
2. ? Usar `/api/organizations/with-admin` para crear el primer admin
3. ? Login con las credenciales creadas
4. ? El hash se genera correctamente automáticamente

**Credenciales sugeridas**:
- **Username**: `superadmin`
- **Password**: `Admin123!` (cambiar después del primer acceso)

?? **Recuerda: El endpoint `/api/organizations/with-admin` NO requiere autenticación, así que protégele en producción.**
