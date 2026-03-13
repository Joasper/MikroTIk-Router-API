# Documentación API - Módulo de Autenticación

## ?? Información General

El módulo de autenticación maneja el inicio de sesión, validación de tokens JWT y cierre de sesión de usuarios en el sistema MikroClean.

---

## ?? Base URL

```
http://localhost:5000/api/auth
```

---

## ?? Características de Seguridad

### JWT (JSON Web Tokens)
- **Algoritmo**: HMAC SHA256
- **Expiración**: 24 horas (configurable)
- **Claims incluidos**: UserId, Username, Email, Role, OrganizationId, SystemRoleId

### Protección contra Fuerza Bruta
- **Máximo de intentos**: 5 intentos fallidos
- **Bloqueo temporal**: 30 minutos
- **Reseteo**: Al login exitoso se resetea el contador

### Almacenamiento de Token
- **Cookie HTTP-Only**: `authToken` (no accesible desde JavaScript)
- **Cookie Regular**: `userId` (accesible para UI)
- **Configuración**:
  - HttpOnly: true (para authToken)
  - Secure: true (solo HTTPS)
  - SameSite: Strict
  - Expira junto con el token

### Validación de Licencia
- Se valida automáticamente al iniciar sesión
- Licencias expiradas bloquean el acceso
- Licencias inactivas bloquean el acceso

---

## ?? Endpoints Disponibles

### 1. Iniciar Sesión (Login) ?

**POST** `/api/auth/login`

Autentica un usuario y genera un token JWT.

#### Request Body
```json
{
  "usernameOrEmail": "admin",
  "password": "SecurePass123!"
}
```

#### Validaciones
| Campo | Validación |
|-------|------------|
| usernameOrEmail | Requerido, puede ser username o email |
| password | Requerido |

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Inicio de sesión exitoso",
  "data": {
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@techcorp.com",
      "isActive": true,
      "lastLogin": "2024-03-15T14:30:00Z",
      "organizationId": 1,
      "organizationName": "TechCorp",
      "systemRoleId": 1,
      "systemRoleName": "Administrador",
      "createdAt": "2024-03-01T10:00:00Z"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "expiresAt": "2024-03-16T14:30:00Z"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Cookies Establecidas
```
Set-Cookie: authToken=eyJhbGciOi...; HttpOnly; Secure; SameSite=Strict; Expires=2024-03-16T14:30:00Z
Set-Cookie: userId=1; Secure; SameSite=Strict; Expires=2024-03-16T14:30:00Z
```

#### Response 400 Bad Request (Credenciales Inválidas)
```json
{
  "status": "validation_error",
  "message": "Credenciales inválidas",
  "data": null,
  "errors": {
    "Credentials": "Usuario o contraseña incorrectos"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Response 400 Bad Request (Cuenta Bloqueada)
```json
{
  "status": "validation_error",
  "message": "Cuenta bloqueada. Intente nuevamente en 25 minutos",
  "data": null,
  "errors": {
    "Account": "Cuenta bloqueada temporalmente"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Response 400 Bad Request (Cuenta Desactivada)
```json
{
  "status": "validation_error",
  "message": "La cuenta está desactivada",
  "data": null,
  "errors": {
    "Account": "Cuenta desactivada"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Response 400 Bad Request (Licencia Expirada)
```json
{
  "status": "validation_error",
  "message": "La licencia de su organización ha expirado",
  "data": null,
  "errors": {
    "License": "Licencia expirada"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Otros Errores Posibles
- **Licencia inactiva**: `"La licencia de su organización está inactiva"`
- **Intentos restantes**: `"Credenciales inválidas. 3 intentos restantes"`
- **Bloqueo por intentos**: `"Cuenta bloqueada por 30 minutos debido a múltiples intentos fallidos"`

---

### 2. Validar Token

**POST** `/api/auth/validate-token`

Valida si un token JWT es válido y no ha expirado.

#### Request Body
```json
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

#### Response 200 OK (Token Válido)
```json
{
  "status": "success",
  "message": "Token válido",
  "data": true,
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Response 400 Bad Request (Token Expirado)
```json
{
  "status": "validation_error",
  "message": "El token ha expirado",
  "data": false,
  "errors": {
    "Token": "Token expirado"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Response 400 Bad Request (Token Inválido)
```json
{
  "status": "validation_error",
  "message": "Token inválido",
  "data": false,
  "errors": {
    "Token": "Token no válido"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

---

### 3. Cerrar Sesión (Logout)

**POST** `/api/auth/logout/{userId}`

Cierra la sesión del usuario y elimina las cookies.

#### Path Parameters
- `userId` (int): ID del usuario

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Sesión cerrada exitosamente",
  "data": true,
  "timestamp": "2024-03-15T15:00:00Z"
}
```

#### Cookies Eliminadas
```
Set-Cookie: authToken=; Expires=Thu, 01 Jan 1970 00:00:00 GMT
Set-Cookie: userId=; Expires=Thu, 01 Jan 1970 00:00:00 GMT
```

#### Response 404 Not Found
```json
{
  "status": "not_found",
  "message": "Usuario no encontrado",
  "data": false,
  "timestamp": "2024-03-15T15:00:00Z"
}
```

---

## ?? Modelos de Datos

### LoginRequestDTO
```typescript
{
  usernameOrEmail: string; // Requerido
  password: string;        // Requerido
}
```

### LoginResponseDTO
```typescript
{
  user: UserDTO;
  token: string;
  expiresAt: string; // ISO 8601
}
```

### UserDTO
```typescript
{
  id: number;
  username: string;
  email: string;
  isActive: boolean;
  lastLogin: string | null; // ISO 8601
  organizationId: number | null;
  organizationName: string | null;
  systemRoleId: number;
  systemRoleName: string;
  createdAt: string; // ISO 8601
}
```

---

## ?? Claims en el Token JWT

El token JWT incluye los siguientes claims:

```json
{
  "sub": "1",                          // User ID (ClaimTypes.NameIdentifier)
  "name": "admin",                     // Username (ClaimTypes.Name)
  "email": "admin@techcorp.com",       // Email (ClaimTypes.Email)
  "role": "Administrador",             // Role Name (ClaimTypes.Role)
  "OrganizationId": "1",               // Organization ID (custom)
  "SystemRoleId": "1",                 // System Role ID (custom)
  "exp": 1710518400,                   // Expiration timestamp
  "iss": "MikroCleanAPI",              // Issuer
  "aud": "MikroCleanClient"            // Audience
}
```

---

## ?? Integración con Frontend

### Opción 1: Usar Cookies (Recomendado) ??

El backend automáticamente maneja las cookies, el frontend solo necesita hacer fetch con `credentials: 'include'`:

```typescript
// login.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface LoginResponse {
  status: string;
  message: string;
  data: {
    user: UserDTO;
    token: string;
    expiresAt: string;
  };
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/login`,
      credentials,
      { withCredentials: true } // ? Importante para cookies
    ).pipe(
      tap(response => {
        if (response.status === 'success' && response.data) {
          // Guardar datos del usuario en localStorage
          localStorage.setItem('user', JSON.stringify(response.data.user));
          localStorage.setItem('tokenExpires', response.data.expiresAt);
        }
      })
    );
  }

  logout(userId: number): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/logout/${userId}`,
      {},
      { withCredentials: true } // ? Importante para cookies
    ).pipe(
      tap(() => {
        localStorage.removeItem('user');
        localStorage.removeItem('tokenExpires');
      })
    );
  }

  validateToken(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/validate-token`, token);
  }

  isAuthenticated(): boolean {
    const user = localStorage.getItem('user');
    const tokenExpires = localStorage.getItem('tokenExpires');
    
    if (!user || !tokenExpires) {
      return false;
    }

    return new Date(tokenExpires) > new Date();
  }

  getCurrentUser(): any {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }
}
```

### Configurar HttpClient para Cookies

```typescript
// app.config.ts (Angular 18+)
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi())
  ]
};
```

### HTTP Interceptor para Manejo de Errores

```typescript
// auth.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Las cookies se envían automáticamente con withCredentials: true
    const clonedReq = req.clone({
      withCredentials: true
    });

    return next.handle(clonedReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expirado o inválido
          localStorage.removeItem('user');
          localStorage.removeItem('tokenExpires');
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
```

### Guard de Autenticación

```typescript
// auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }

    // Redirigir al login
    this.router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }
}
```

### Componente de Login

```typescript
// login.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, LoginRequest } from './services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  credentials: LoginRequest = {
    usernameOrEmail: '',
    password: ''
  };
  
  loading = false;
  errorMessage = '';
  fieldErrors: { [key: string]: string } = {};

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit() {
    this.loading = true;
    this.errorMessage = '';
    this.fieldErrors = {};

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          console.log('? Login exitoso:', response.data.user);
          this.router.navigate(['/dashboard']);
        } else if (response.status === 'validation_error') {
          this.errorMessage = response.message;
          this.fieldErrors = response.errors || {};
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('? Error en login:', error);
        this.errorMessage = error.error?.message || 'Error de conexión';
        this.fieldErrors = error.error?.errors || {};
        this.loading = false;
      }
    });
  }

  logout() {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.authService.logout(user.id).subscribe({
        next: () => {
          console.log('? Logout exitoso');
          this.router.navigate(['/login']);
        },
        error: (error) => {
          console.error('? Error en logout:', error);
        }
      });
    }
  }
}
```

```html
<!-- login.component.html -->
<div class="login-container">
  <h2>Iniciar Sesión</h2>
  
  <div *ngIf="errorMessage" class="alert alert-danger">
    {{ errorMessage }}
  </div>

  <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
    <div class="form-group">
      <label>Usuario o Email</label>
      <input
        type="text"
        [(ngModel)]="credentials.usernameOrEmail"
        name="usernameOrEmail"
        class="form-control"
        [class.is-invalid]="fieldErrors['Credentials']"
        required
      />
      <div *ngIf="fieldErrors['Credentials']" class="invalid-feedback">
        {{ fieldErrors['Credentials'] }}
      </div>
    </div>

    <div class="form-group">
      <label>Contraseña</label>
      <input
        type="password"
        [(ngModel)]="credentials.password"
        name="password"
        class="form-control"
        [class.is-invalid]="fieldErrors['Credentials']"
        required
      />
    </div>

    <button 
      type="submit" 
      class="btn btn-primary"
      [disabled]="loading || !loginForm.valid"
    >
      <span *ngIf="loading">Iniciando sesión...</span>
      <span *ngIf="!loading">Iniciar Sesión</span>
    </button>
  </form>
</div>
```

---

### Opción 2: Usar localStorage (Si no puedes usar cookies)

```typescript
// auth.service.ts (versión con localStorage)
login(credentials: LoginRequest): Observable<LoginResponse> {
  return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials)
    .pipe(
      tap(response => {
        if (response.status === 'success' && response.data) {
          // Guardar token y usuario en localStorage
          localStorage.setItem('authToken', response.data.token);
          localStorage.setItem('user', JSON.stringify(response.data.user));
          localStorage.setItem('tokenExpires', response.data.expiresAt);
        }
      })
    );
}

// Interceptor para agregar token a las peticiones
intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
  const token = localStorage.getItem('authToken');
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next.handle(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        localStorage.removeItem('tokenExpires');
        this.router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
}
```

---

## ?? Flujo Completo de Autenticación

### 1. Usuario Inicia Sesión

```mermaid
sequenceDiagram
    participant U as Usuario
    participant F as Frontend
    participant A as API
    participant D as DB

    U->>F: Ingresa credenciales
    F->>A: POST /api/auth/login
    A->>D: Buscar usuario
    D-->>A: Usuario encontrado
    A->>A: Validar contraseña (BCrypt)
    A->>A: Validar licencia
    A->>A: Generar JWT
    A->>D: Actualizar LastLogin
    A-->>F: 200 OK + Token + Cookies
    F->>F: Guardar user en localStorage
    F->>U: Redirigir a Dashboard
```

### 2. Peticiones Autenticadas

```mermaid
sequenceDiagram
    participant F as Frontend
    participant A as API

    F->>A: GET /api/organizations (Cookie: authToken)
    A->>A: Validar JWT
    A->>A: Verificar permisos
    A-->>F: 200 OK + Data
```

### 3. Token Expirado

```mermaid
sequenceDiagram
    participant F as Frontend
    participant A as API

    F->>A: GET /api/users (Token expirado)
    A->>A: Validar JWT (FAIL)
    A-->>F: 401 Unauthorized
    F->>F: Limpiar localStorage
    F->>F: Eliminar cookies
    F->>U: Redirigir a Login
```

---

## ?? Ejemplo Completo de Componente Dashboard

```typescript
// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  currentUser: any;
  tokenExpiresIn: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.currentUser = this.authService.getCurrentUser();
    this.updateTokenExpiration();
    
    // Actualizar cada minuto
    setInterval(() => this.updateTokenExpiration(), 60000);
  }

  updateTokenExpiration() {
    const expiresAt = localStorage.getItem('tokenExpires');
    if (expiresAt) {
      const now = new Date();
      const expires = new Date(expiresAt);
      const diff = expires.getTime() - now.getTime();
      const hours = Math.floor(diff / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      
      this.tokenExpiresIn = `${hours}h ${minutes}m`;
      
      // Advertir si quedan menos de 30 minutos
      if (diff < 30 * 60 * 1000 && diff > 0) {
        console.warn('?? Tu sesión expirará pronto. Renueva tu token.');
      }
    }
  }

  logout() {
    if (this.currentUser) {
      this.authService.logout(this.currentUser.id).subscribe({
        next: () => {
          this.router.navigate(['/login']);
        }
      });
    }
  }
}
```

```html
<!-- dashboard.component.html -->
<div class="dashboard">
  <header>
    <h1>Dashboard</h1>
    <div class="user-info">
      <span>Bienvenido, {{ currentUser?.username }}</span>
      <span class="role-badge">{{ currentUser?.systemRoleName }}</span>
      <span class="token-expiration">?? {{ tokenExpiresIn }}</span>
      <button (click)="logout()" class="btn btn-logout">
        Cerrar Sesión
      </button>
    </div>
  </header>

  <div class="organization-info" *ngIf="currentUser?.organizationName">
    <p>Organización: {{ currentUser.organizationName }}</p>
  </div>

  <!-- Contenido del dashboard -->
</div>
```

---

## ?? Manejo de Errores Comunes

### 1. Credenciales Incorrectas

```typescript
// Después de 5 intentos fallidos
{
  "status": "validation_error",
  "message": "Cuenta bloqueada por 30 minutos debido a múltiples intentos fallidos",
  "errors": {
    "Account": "Cuenta bloqueada temporalmente"
  }
}
```

**Manejo en Frontend**:
```typescript
if (error.error?.errors?.Account === 'Cuenta bloqueada temporalmente') {
  // Mostrar mensaje especial con tiempo de espera
  this.showBlockedAccountMessage(error.error.message);
}
```

### 2. Licencia Expirada

```typescript
{
  "status": "validation_error",
  "message": "La licencia de su organización ha expirado",
  "errors": {
    "License": "Licencia expirada"
  }
}
```

**Manejo en Frontend**:
```typescript
if (error.error?.errors?.License === 'Licencia expirada') {
  // Redirigir a página de renovación de licencia
  this.router.navigate(['/renew-license']);
}
```

### 3. Token Expirado

```typescript
// Interceptor detecta 401
this.http.get('/api/users').subscribe({
  error: (error) => {
    if (error.status === 401) {
      // Limpiar sesión y redirigir a login
      this.authService.clearSession();
      this.router.navigate(['/login']);
    }
  }
});
```

---

## ?? Mejores Prácticas de Seguridad

### 1. Almacenamiento Seguro
- ? **Usar cookies HTTP-Only** para el token (recomendado)
- ? Cookies con `Secure=true` (solo HTTPS)
- ? Cookies con `SameSite=Strict`
- ?? Si usas localStorage, ten cuidado con XSS

### 2. Validación del Token
- ? Validar token en cada petición crítica
- ? Renovar token antes de expirar (implementar refresh token)
- ? Invalidar token al hacer logout

### 3. Manejo de Sesiones
- ? Cerrar sesión automáticamente al expirar
- ? Advertir al usuario cuando falte poco tiempo
- ? Implementar "Remember Me" con refresh tokens

### 4. Protección de Rutas
- ? Usar guards en todas las rutas protegidas
- ? Verificar permisos según el rol del usuario
- ? Redirigir a login si no está autenticado

---

## ?? Estados de Cuenta

| Estado | Descripción | Puede Iniciar Sesión |
|--------|-------------|----------------------|
| Activa | Usuario activo y licencia válida | ? Sí |
| Desactivada | Usuario desactivado | ? No |
| Bloqueada | Más de 5 intentos fallidos | ? No (30 min) |
| Licencia Expirada | Licencia de organización vencida | ? No |
| Licencia Inactiva | Licencia desactivada | ? No |

---

## ?? Testing

### Pruebas de Login

```typescript
// auth.service.spec.ts
describe('AuthService', () => {
  it('should login successfully', (done) => {
    const credentials = {
      usernameOrEmail: 'admin',
      password: 'password123'
    };

    service.login(credentials).subscribe(response => {
      expect(response.status).toBe('success');
      expect(response.data.token).toBeDefined();
      expect(response.data.user.username).toBe('admin');
      done();
    });
  });

  it('should handle invalid credentials', (done) => {
    const credentials = {
      usernameOrEmail: 'admin',
      password: 'wrongpassword'
    };

    service.login(credentials).subscribe({
      error: (error) => {
        expect(error.error.status).toBe('validation_error');
        expect(error.error.errors.Credentials).toBeDefined();
        done();
      }
    });
  });
});
```

---

## ? Checklist de Integración

- [ ] Configurar `withCredentials: true` en HttpClient
- [ ] Implementar AuthService con login/logout
- [ ] Crear HTTP Interceptor para manejo de errores
- [ ] Implementar AuthGuard para rutas protegidas
- [ ] Crear componente de Login
- [ ] Manejar cookies en el navegador
- [ ] Implementar manejo de errores específicos (bloqueo, licencia)
- [ ] Mostrar información de usuario en UI
- [ ] Implementar logout con limpieza de cookies
- [ ] Probar flujo completo de autenticación
- [ ] Implementar renovación automática de token (opcional)
- [ ] Configurar CORS correctamente en backend

---

## ?? Resumen

El sistema de autenticación de MikroClean proporciona:

1. **Login seguro** con validación de credenciales y licencia
2. **Tokens JWT** con expiración configurable
3. **Cookies HTTP-Only** para almacenamiento seguro
4. **Protección contra fuerza bruta** con bloqueo temporal
5. **Validación automática** de licencias al iniciar sesión
6. **Logout limpio** con eliminación de cookies
7. **Manejo robusto de errores** con mensajes específicos

¡Todo listo para conectar con tu frontend Angular! ??
