# ?? Guía Rápida de Inicio - MikroClean API

## ? Cambios Implementados

### 1. CORS Configurado
- ? Angular localhost:4200 y 4201 permitidos
- ? Credentials habilitados para cookies

### 2. Autenticación con Cookies
- ? Token JWT se guarda en cookie HTTP-Only (`authToken`)
- ? UserId en cookie accesible (`userId`)
- ? Cookies se limpian al hacer logout

### 3. Módulo de Licencias Actualizado
- ? Licencias se crean **sin organización**
- ? Campo `OrganizationId` es nullable
- ? Endpoint `/api/licenses/available` para licencias sin asignar
- ? Endpoint `/api/licenses/by-key/{key}` para buscar
- ? Paginación completa con filtros

### 4. Usuario SuperAdmin por Defecto
- ?? Hash en SeedData es **placeholder**
- ? Usar endpoint para crear primer admin

---

## ?? Inicio Rápido

### Paso 1: Crear tu Primer Admin

**Opción A (Recomendada)**: Usar endpoint

```http
POST http://localhost:5000/api/organizations/with-admin
Content-Type: application/json

{
  "organizationName": "Sistema MikroClean",
  "organizationEmail": "admin@mikroclean.com",
  "organizationPhone": "+000",
  "adminUsername": "superadmin",
  "adminEmail": "admin@mikroclean.com",
  "adminPassword": "Admin123!"
}
```

### Paso 2: Login

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "usernameOrEmail": "superadmin",
  "password": "Admin123!"
}
```

**Response** incluye:
- ? Token JWT en el body
- ? Cookie `authToken` (HTTP-Only)
- ? Cookie `userId`

### Paso 3: Usar la API

```typescript
// Angular - Todas las peticiones automáticamente incluyen las cookies
this.http.get('http://localhost:5000/api/organizations', {
  withCredentials: true // ? Importante
}).subscribe();
```

---

## ?? Documentación Completa

- **AUTHENTICATION_API_DOCUMENTATION.md** - Sistema de autenticación con cookies
- **ORGANIZATIONS_API_DOCUMENTATION.md** - CRUD de organizaciones con paginación
- **LICENSES_API_QUICK_REFERENCE.md** - Gestión de licencias
- **DEFAULT_SUPERADMIN_CREDENTIALS.md** - Usuario admin por defecto

---

## ?? Credenciales por Defecto

```
Username: superadmin
Password: Admin123!
```

?? **IMPORTANTE**: Cambiar después del primer acceso.

---

## ?? Configuración Angular

### HttpClient
```typescript
// Todas las peticiones con:
{ withCredentials: true }
```

### Interceptor
```typescript
intercept(req: HttpRequest<any>, next: HttpHandler) {
  const cloned = req.clone({ withCredentials: true });
  return next.handle(cloned);
}
```

---

## ? Características Principales

### Organizaciones
- ? CRUD completo
- ? Paginación, búsqueda, ordenamiento
- ? Creación con admin y licencia automática

### Licencias
- ? Creación sin asignar
- ? Asociación dinámica
- ? Filtros múltiples (tipo, estado, expiración)
- ? Monitoreo de expiración

### Autenticación
- ? JWT con cookies HTTP-Only
- ? Protección contra fuerza bruta (5 intentos)
- ? Validación de licencia al login
- ? Bloqueo temporal 30 minutos

---

## ?? ¡Listo para Conectar con Angular!

1. ? Backend configurado con CORS
2. ? Cookies HTTP-Only implementadas
3. ? Endpoints documentados
4. ? Ejemplos de integración Angular
5. ? Usuario admin disponible

**Siguiente paso**: Consumir desde tu frontend Angular con `withCredentials: true` ??
