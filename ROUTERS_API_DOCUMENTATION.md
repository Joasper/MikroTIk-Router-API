# Documentación API - Módulo de Routers MikroTik

## ?? Información General

El módulo de routers gestiona dispositivos MikroTik, incluyendo:
- **CRUD de routers** (crear, leer, actualizar, eliminar)
- **Operaciones MikroTik** (interfaces, VLAN, firewall, IP, etc.)
- **Gestión de conexiones** (pool de conexiones, test, estado)
- **Seguridad** (encriptación automática de contraseñas)

---

## ?? Base URLs

```
http://localhost:5000/api/routers       # CRUD de routers
http://localhost:5000/api/mikrotik      # Operaciones MikroTik
```

---

## ??? Arquitectura del Módulo

### Capas del Sistema

```
???????????????????????????????????????????????????????????
?                    Controllers                           ?
?  RoutersController  ?  MikroTikController               ?
???????????????????????????????????????????????????????????
                ?                     ?
???????????????????????????????????????????????????????????
?                     Services                             ?
?  RouterService      ?  MikroTikService                  ?
???????????????????????????????????????????????????????????
                ?                     ?
???????????????????????????????????????????????????????????
?                Infrastructure                            ?
?  RouterRepository   ?  MikroTikConnectionManager        ?
?  EncryptionService  ?  RouterConnectionPool             ?
???????????????????????????????????????????????????????????
```

### Separación de Responsabilidades

| Componente | Responsabilidad |
|------------|-----------------|
| **RoutersController** | CRUD básico (crear, leer, actualizar, eliminar routers) |
| **MikroTikController** | Operaciones específicas de MikroTik (VLAN, firewall, etc.) |
| **RouterService** | Lógica de negocio CRUD, validaciones, encriptación |
| **MikroTikService** | Orquestación de operaciones MikroTik |
| **MikroTikConnectionManager** | Gestión del pool de conexiones |
| **IMikroTikOperation** | Interfaz para operaciones extensibles |

---

## ?? Endpoints - CRUD de Routers

### 1. Obtener Routers por Organización

**GET** `/api/routers/organization/{organizationId}`

Obtiene todos los routers de una organización específica.

#### Path Parameters
- `organizationId` (int): ID de la organización

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Se encontraron 3 routers",
  "data": [
    {
      "id": 1,
      "name": "Router Principal",
      "ip": "192.168.1.1",
      "user": "admin",
      "isActive": true,
      "model": "RB4011iGS+",
      "version": "7.12.1",
      "lastSeen": "2024-03-15T14:30:00Z",
      "macAddress": "6C:3B:6B:12:34:56",
      "location": "Oficina Central",
      "organizationId": 1,
      "organizationName": "TechCorp",
      "connectionStatus": {
        "isConnected": true,
        "lastChecked": "2024-03-15T14:30:00Z",
        "errorMessage": null
      },
      "createdAt": "2024-03-01T10:00:00Z"
    }
  ],
  "timestamp": "2024-03-15T14:30:00Z"
}
```

---

### 2. Obtener Router por ID

**GET** `/api/routers/{id}`

#### Path Parameters
- `id` (int): ID del router

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Router obtenido exitosamente",
  "data": {
    "id": 1,
    "name": "Router Principal",
    "ip": "192.168.1.1",
    "user": "admin",
    "isActive": true,
    "model": "RB4011iGS+",
    "version": "7.12.1",
    "lastSeen": "2024-03-15T14:30:00Z",
    "macAddress": "6C:3B:6B:12:34:56",
    "location": "Oficina Central",
    "organizationId": 1,
    "organizationName": "TechCorp",
    "createdAt": "2024-03-01T10:00:00Z"
  },
  "timestamp": "2024-03-15T14:30:00Z"
}
```

#### Response 404 Not Found
```json
{
  "status": "not_found",
  "message": "Router no encontrado",
  "data": null,
  "timestamp": "2024-03-15T14:30:00Z"
}
```

---

### 3. Crear Router ?

**POST** `/api/routers`

Crea un nuevo router. **La contraseña se encripta automáticamente** con AES-256.

#### Request Body
```json
{
  "name": "Router Sucursal Norte",
  "ip": "192.168.10.1",
  "user": "admin",
  "password": "MikroTikPass123!",
  "model": "hEX S",
  "location": "Sucursal Norte - Piso 2",
  "organizationId": 1
}
```

#### Validaciones
| Campo | Validación |
|-------|------------|
| name | Requerido, máx 200 caracteres |
| ip | Requerido, formato IP válido, único en sistema |
| user | Requerido, máx 100 caracteres |
| password | Requerido (se encripta antes de guardar) |
| model | Opcional, máx 100 caracteres |
| location | Opcional, máx 200 caracteres |
| organizationId | Requerido, debe existir |

#### Response 201 Created
```json
{
  "status": "success",
  "message": "Router creado exitosamente",
  "data": {
    "id": 5,
    "name": "Router Sucursal Norte",
    "ip": "192.168.10.1",
    "user": "admin",
    "isActive": true,
    "model": "hEX S",
    "version": null,
    "lastSeen": null,
    "macAddress": null,
    "location": "Sucursal Norte - Piso 2",
    "organizationId": 1,
    "organizationName": "TechCorp",
    "createdAt": "2024-03-15T15:00:00Z"
  },
  "timestamp": "2024-03-15T15:00:00Z"
}
```

**Nota**: La contraseña NO se devuelve en el response por seguridad.

#### Response 400 Bad Request (IP Duplicada)
```json
{
  "status": "validation_error",
  "message": "Ya existe un router con esa IP",
  "data": null,
  "errors": {
    "Ip": "La dirección IP ya está registrada"
  },
  "timestamp": "2024-03-15T15:00:00Z"
}
```

#### Response 400 Bad Request (Organización No Existe)
```json
{
  "status": "validation_error",
  "message": "Organización no encontrada",
  "data": null,
  "errors": {
    "OrganizationId": "La organización no existe"
  },
  "timestamp": "2024-03-15T15:00:00Z"
}
```

---

### 4. Actualizar Router

**PUT** `/api/routers/{id}`

#### Path Parameters
- `id` (int): ID del router

#### Request Body
```json
{
  "name": "Router Principal Actualizado",
  "ip": "192.168.1.1",
  "user": "admin",
  "password": "NuevaContraseña123!",
  "model": "RB4011iGS+",
  "location": "Datacenter - Rack A1",
  "isActive": true
}
```

**Nota**: `password` es **opcional**. Si se proporciona, se re-encripta y la conexión activa se cierra para forzar reconexión.

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Router actualizado exitosamente",
  "data": {
    "id": 1,
    "name": "Router Principal Actualizado",
    "ip": "192.168.1.1",
    "user": "admin",
    "isActive": true,
    "model": "RB4011iGS+",
    "location": "Datacenter - Rack A1",
    "organizationId": 1,
    "organizationName": "TechCorp",
    "createdAt": "2024-03-01T10:00:00Z"
  },
  "timestamp": "2024-03-15T16:00:00Z"
}
```

---

### 5. Eliminar Router (Soft Delete)

**DELETE** `/api/routers/{id}`

Elimina lógicamente el router y cierra su conexión activa.

#### Path Parameters
- `id` (int): ID del router

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Router eliminado exitosamente",
  "data": true,
  "timestamp": "2024-03-15T17:00:00Z"
}
```

---

### 6. Probar Conexión a Router

**POST** `/api/routers/{id}/test`

Prueba la conexión al router y actualiza su campo `lastSeen`.

#### Path Parameters
- `id` (int): ID del router

#### Response 200 OK (Conectado)
```json
{
  "status": "success",
  "message": "Router conectado exitosamente",
  "data": true,
  "timestamp": "2024-03-15T18:00:00Z"
}
```

#### Response 200 OK (No Conectado)
```json
{
  "status": "success",
  "message": "No se pudo conectar al router",
  "data": false,
  "timestamp": "2024-03-15T18:00:00Z"
}
```

---

## ?? Endpoints - Operaciones MikroTik

### 7. Test de Conexión con Estado

**GET** `/api/mikrotik/routers/{routerId}/test-connection`

Prueba la conexión y retorna información detallada del estado.

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Conexión exitosa",
  "data": {
    "isConnected": true,
    "lastChecked": "2024-03-15T18:30:00Z",
    "errorMessage": null,
    "connectionTime": "45ms"
  },
  "timestamp": "2024-03-15T18:30:00Z"
}
```

---

### 8. Obtener Estado del Router

**GET** `/api/mikrotik/routers/{routerId}/status`

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Estado obtenido exitosamente",
  "data": {
    "isConnected": true,
    "lastChecked": "2024-03-15T18:30:00Z",
    "connectionPoolSize": 1
  },
  "timestamp": "2024-03-15T18:30:00Z"
}
```

---

### 9. Pre-calentar Conexiones de Organización

**POST** `/api/mikrotik/organizations/{organizationId}/warm-up`

Establece conexiones con todos los routers de la organización.

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Conexiones pre-calentadas",
  "data": {
    "1": true,
    "2": true,
    "3": false
  },
  "timestamp": "2024-03-15T18:30:00Z"
}
```

---

### 10. Crear Bridge

**POST** `/api/mikrotik/routers/{routerId}/interfaces/bridge`

#### Request Body
```json
{
  "name": "bridge-lan",
  "comment": "Bridge para LAN"
}
```

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Bridge creado exitosamente",
  "data": {
    "id": ".id=*5",
    "name": "bridge-lan",
    "running": true
  },
  "timestamp": "2024-03-15T19:00:00Z"
}
```

---

### 11. Crear VLAN

**POST** `/api/mikrotik/routers/{routerId}/interfaces/vlan`

#### Request Body
```json
{
  "name": "vlan-invitados",
  "vlanId": 100,
  "interface": "ether1"
}
```

#### Response 200 OK
```json
{
  "status": "success",
  "message": "VLAN creada exitosamente",
  "data": {
    "id": ".id=*7",
    "name": "vlan-invitados",
    "vlanId": 100,
    "interface": "ether1"
  },
  "timestamp": "2024-03-15T19:00:00Z"
}
```

---

### 12. Listar Interfaces

**GET** `/api/mikrotik/routers/{routerId}/interfaces`

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Interfaces obtenidas exitosamente",
  "data": [
    {
      "name": "ether1",
      "type": "ether",
      "running": true,
      "disabled": false,
      "comment": "WAN"
    },
    {
      "name": "ether2",
      "type": "ether",
      "running": true,
      "disabled": false,
      "comment": "LAN"
    }
  ],
  "timestamp": "2024-03-15T19:00:00Z"
}
```

---

### 13. Agregar Dirección IP

**POST** `/api/mikrotik/routers/{routerId}/ip/address`

#### Request Body
```json
{
  "address": "192.168.1.1/24",
  "interface": "ether2",
  "network": "192.168.1.0",
  "comment": "Gateway LAN"
}
```

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Dirección IP agregada exitosamente",
  "data": {
    "id": ".id=*10",
    "address": "192.168.1.1/24",
    "interface": "ether2",
    "network": "192.168.1.0"
  },
  "timestamp": "2024-03-15T19:30:00Z"
}
```

---

### 14. Crear Regla de Firewall

**POST** `/api/mikrotik/routers/{routerId}/firewall/rules`

#### Request Body
```json
{
  "chain": "forward",
  "action": "accept",
  "protocol": "tcp",
  "dstPort": "80,443",
  "comment": "Permitir HTTP/HTTPS"
}
```

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Regla de firewall creada exitosamente",
  "data": {
    "id": ".id=*15",
    "chain": "forward",
    "action": "accept",
    "protocol": "tcp",
    "dstPort": "80,443"
  },
  "timestamp": "2024-03-15T20:00:00Z"
}
```

---

### 15. Obtener Recursos del Sistema

**GET** `/api/mikrotik/routers/{routerId}/system/resources`

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Recursos obtenidos exitosamente",
  "data": {
    "uptime": "2w3d15h30m",
    "version": "7.12.1 (stable)",
    "buildTime": "Feb/15/2024 10:30:00",
    "freeMemory": "512000000",
    "totalMemory": "1073741824",
    "cpuLoad": 15,
    "freeHddSpace": "128000000",
    "totalHddSpace": "268435456",
    "architecture": "arm",
    "boardName": "RB4011iGS+",
    "platform": "MikroTik"
  },
  "timestamp": "2024-03-15T20:30:00Z"
}
```

---

## ?? Modelos de Datos

### RouterDTO
```typescript
{
  id: number;
  name: string;
  ip: string;
  user: string;
  // password NO se expone por seguridad
  isActive: boolean;
  model: string | null;
  version: string | null;
  lastSeen: string | null;          // ISO 8601
  macAddress: string | null;
  location: string | null;
  organizationId: number;
  organizationName: string;
  connectionStatus?: {
    isConnected: boolean;
    lastChecked: string;            // ISO 8601
    errorMessage: string | null;
  };
  createdAt: string;                // ISO 8601
}
```

### CreateRouterDTO
```typescript
{
  name: string;                     // Requerido
  ip: string;                       // Requerido, formato IP válido
  user: string;                     // Requerido
  password: string;                 // Requerido (se encripta automáticamente)
  model?: string;                   // Opcional
  location?: string;                // Opcional
  organizationId: number;           // Requerido
}
```

### UpdateRouterDTO
```typescript
{
  name: string;                     // Requerido
  ip: string;                       // Requerido
  user: string;                     // Requerido
  password?: string;                // Opcional (si se cambia)
  model?: string;                   // Opcional
  location?: string;                // Opcional
  isActive: boolean;                // Requerido
}
```

---

## ?? Seguridad de Contraseñas

### Encriptación Automática

- **Algoritmo**: AES-256-CBC
- **Key**: Configurada en `appsettings.json`
- **IV**: Aleatorio por cada contraseña
- **Formato almacenado**: `{IV}:{CipherText}` (Base64)

### Flujo de Encriptación

```
????????????????      ????????????????      ????????????????
?   Frontend   ????????    Backend   ????????   Database   ?
?  password:   ? POST ?  Encrypt with? SAVE ?  Encrypted:  ?
?  "MyPass123" ?      ?   AES-256    ?      ? "aB3c...xyz" ?
????????????????      ????????????????      ????????????????
```

### Al Conectar al Router

```
????????????????      ????????????????      ????????????????
?   Database   ????????   Service    ????????  MikroTik    ?
?  Encrypted   ? READ ?   Decrypt    ? CONN ?   Router     ?
?  Password    ?      ?   Password   ?      ?              ?
????????????????      ????????????????      ????????????????
```

### Configuración en appsettings.json

```json
{
  "Encryption": {
    "Key": "YOUR-32-CHARACTER-ENCRYPTION-KEY-HERE-123456"
  }
}
```

---

## ?? Gestión de Conexiones

### Pool de Conexiones

El sistema mantiene un **pool de conexiones persistentes** para evitar reconexiones constantes:

- **Máximo de conexiones**: 100 simultáneas
- **Timeout de inactividad**: 15 minutos
- **Reconexión automática**: Si falla
- **Thread-safe**: Seguro para concurrencia

### Estados de Conexión

| Estado | Descripción |
|--------|-------------|
| **Connected** | Conexión activa y funcional |
| **Disconnected** | Sin conexión |
| **Connecting** | Intentando conectar |
| **Error** | Error en la última operación |

---

## ?? Integración con Angular

### Service de Routers

```typescript
// router.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RouterDTO {
  id: number;
  name: string;
  ip: string;
  user: string;
  isActive: boolean;
  model?: string;
  version?: string;
  lastSeen?: string;
  macAddress?: string;
  location?: string;
  organizationId: number;
  organizationName: string;
  createdAt: string;
}

export interface CreateRouterDTO {
  name: string;
  ip: string;
  user: string;
  password: string;
  model?: string;
  location?: string;
  organizationId: number;
}

@Injectable({
  providedIn: 'root'
})
export class RouterService {
  private apiUrl = 'http://localhost:5000/api/routers';

  constructor(private http: HttpClient) {}

  // Obtener routers por organización
  getByOrganization(organizationId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/organization/${organizationId}`);
  }

  // Obtener router por ID
  getById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  // Crear router
  create(router: CreateRouterDTO): Observable<any> {
    return this.http.post(this.apiUrl, router);
  }

  // Actualizar router
  update(id: number, router: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, router);
  }

  // Eliminar router
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // Probar conexión
  testConnection(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/test`, {});
  }
}
```

### Service de Operaciones MikroTik

```typescript
// mikrotik.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class MikroTikService {
  private apiUrl = 'http://localhost:5000/api/mikrotik';

  constructor(private http: HttpClient) {}

  // Obtener recursos del sistema
  getSystemResources(routerId: number) {
    return this.http.get(`${this.apiUrl}/routers/${routerId}/system/resources`);
  }

  // Crear bridge
  createBridge(routerId: number, data: any) {
    return this.http.post(`${this.apiUrl}/routers/${routerId}/interfaces/bridge`, data);
  }

  // Crear VLAN
  createVlan(routerId: number, data: any) {
    return this.http.post(`${this.apiUrl}/routers/${routerId}/interfaces/vlan`, data);
  }

  // Listar interfaces
  getInterfaces(routerId: number) {
    return this.http.get(`${this.apiUrl}/routers/${routerId}/interfaces`);
  }

  // Agregar IP
  addIpAddress(routerId: number, data: any) {
    return this.http.post(`${this.apiUrl}/routers/${routerId}/ip/address`, data);
  }

  // Crear regla de firewall
  createFirewallRule(routerId: number, data: any) {
    return this.http.post(`${this.apiUrl}/routers/${routerId}/firewall/rules`, data);
  }
}
```

---

### Componente de Lista de Routers

```typescript
// routers-list.component.ts
import { Component, OnInit } from '@angular/core';
import { RouterService, RouterDTO } from './services/router.service';

@Component({
  selector: 'app-routers-list',
  templateUrl: './routers-list.component.html'
})
export class RoutersListComponent implements OnInit {
  routers: RouterDTO[] = [];
  loading = false;
  organizationId: number = 1; // Obtener del contexto del usuario

  constructor(private routerService: RouterService) {}

  ngOnInit() {
    this.loadRouters();
  }

  loadRouters() {
    this.loading = true;
    this.routerService.getByOrganization(this.organizationId).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          this.routers = response.data;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error cargando routers:', error);
        this.loading = false;
      }
    });
  }

  testConnection(routerId: number) {
    this.routerService.testConnection(routerId).subscribe({
      next: (response) => {
        if (response.status === 'success' && response.data) {
          alert('? Conexión exitosa');
        } else {
          alert('? No se pudo conectar');
        }
        this.loadRouters(); // Actualizar lastSeen
      }
    });
  }

  deleteRouter(id: number, name: string) {
    if (confirm(`¿Eliminar router "${name}"?`)) {
      this.routerService.delete(id).subscribe({
        next: (response) => {
          if (response.status === 'success') {
            alert('? Router eliminado');
            this.loadRouters();
          }
        },
        error: (error) => {
          alert('? Error: ' + error.error?.message);
        }
      });
    }
  }

  getStatusClass(router: RouterDTO): string {
    if (!router.isActive) return 'status-inactive';
    if (!router.lastSeen) return 'status-unknown';
    
    const lastSeen = new Date(router.lastSeen);
    const now = new Date();
    const diffMinutes = (now.getTime() - lastSeen.getTime()) / 1000 / 60;
    
    if (diffMinutes < 5) return 'status-online';
    if (diffMinutes < 30) return 'status-warning';
    return 'status-offline';
  }
}
```

```html
<!-- routers-list.component.html -->
<div class="routers-container">
  <h2>Routers de la Organización</h2>

  <button routerLink="/routers/create" class="btn btn-primary">
    ? Agregar Router
  </button>

  <div *ngIf="loading">Cargando routers...</div>

  <table *ngIf="!loading" class="table">
    <thead>
      <tr>
        <th>Estado</th>
        <th>Nombre</th>
        <th>IP</th>
        <th>Modelo</th>
        <th>Versión</th>
        <th>Ubicación</th>
        <th>Última Conexión</th>
        <th>Acciones</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let router of routers">
        <td>
          <span [class]="getStatusClass(router)">
            ? {{ router.isActive ? 'Activo' : 'Inactivo' }}
          </span>
        </td>
        <td>{{ router.name }}</td>
        <td><code>{{ router.ip }}</code></td>
        <td>{{ router.model || '-' }}</td>
        <td>{{ router.version || '-' }}</td>
        <td>{{ router.location || '-' }}</td>
        <td>{{ router.lastSeen | date:'short' || 'Nunca' }}</td>
        <td>
          <button (click)="testConnection(router.id)" class="btn btn-sm btn-info">
            ?? Test
          </button>
          <button [routerLink]="['/routers', router.id, 'edit']" class="btn btn-sm btn-primary">
            ?? Editar
          </button>
          <button [routerLink]="['/routers', router.id, 'operations']" class="btn btn-sm btn-success">
            ?? Operar
          </button>
          <button (click)="deleteRouter(router.id, router.name)" class="btn btn-sm btn-danger">
            ??? Eliminar
          </button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

---

### Componente de Creación de Router

```typescript
// create-router.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RouterService, CreateRouterDTO } from './services/router.service';

@Component({
  selector: 'app-create-router',
  templateUrl: './create-router.component.html'
})
export class CreateRouterComponent {
  router: CreateRouterDTO = {
    name: '',
    ip: '',
    user: 'admin',
    password: '',
    model: '',
    location: '',
    organizationId: 1 // Obtener del contexto del usuario
  };

  loading = false;
  errors: { [key: string]: string } = {};
  showPassword = false;

  constructor(
    private routerService: RouterService,
    private router: Router
  ) {}

  onSubmit() {
    this.loading = true;
    this.errors = {};

    this.routerService.create(this.router).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          alert('? ' + response.message);
          this.router.navigate(['/routers']);
        } else if (response.status === 'validation_error') {
          this.errors = response.errors || {};
          alert('? ' + response.message);
        }
        this.loading = false;
      },
      error: (error) => {
        this.errors = error.error?.errors || {};
        alert('? Error: ' + (error.error?.message || 'Error de conexión'));
        this.loading = false;
      }
    });
  }

  testBeforeCreate() {
    // Opcional: probar conexión antes de guardar
    alert('?? Probando conexión...');
    // Implementar lógica de test sin guardar
  }
}
```

```html
<!-- create-router.component.html -->
<div class="create-router-container">
  <h2>Agregar Nuevo Router</h2>

  <form (ngSubmit)="onSubmit()" #routerForm="ngForm">
    <div class="form-group">
      <label>Nombre del Router *</label>
      <input
        type="text"
        [(ngModel)]="router.name"
        name="name"
        class="form-control"
        placeholder="Router Oficina Central"
        required
      />
    </div>

    <div class="row">
      <div class="col-md-6">
        <div class="form-group">
          <label>Dirección IP *</label>
          <input
            type="text"
            [(ngModel)]="router.ip"
            name="ip"
            class="form-control"
            [class.is-invalid]="errors['Ip']"
            placeholder="192.168.1.1"
            required
          />
          <div *ngIf="errors['Ip']" class="invalid-feedback">
            {{ errors['Ip'] }}
          </div>
        </div>
      </div>

      <div class="col-md-6">
        <div class="form-group">
          <label>Usuario *</label>
          <input
            type="text"
            [(ngModel)]="router.user"
            name="user"
            class="form-control"
            placeholder="admin"
            required
          />
        </div>
      </div>
    </div>

    <div class="form-group">
      <label>Contraseña *</label>
      <div class="input-group">
        <input
          [type]="showPassword ? 'text' : 'password'"
          [(ngModel)]="router.password"
          name="password"
          class="form-control"
          placeholder="????????"
          required
        />
        <button
          type="button"
          class="btn btn-outline-secondary"
          (click)="showPassword = !showPassword"
        >
          {{ showPassword ? '??' : '???' }}
        </button>
      </div>
      <small class="form-text text-muted">
        ?? La contraseña se encripta automáticamente antes de guardar
      </small>
    </div>

    <div class="row">
      <div class="col-md-6">
        <div class="form-group">
          <label>Modelo</label>
          <input
            type="text"
            [(ngModel)]="router.model"
            name="model"
            class="form-control"
            placeholder="RB4011iGS+"
          />
        </div>
      </div>

      <div class="col-md-6">
        <div class="form-group">
          <label>Ubicación</label>
          <input
            type="text"
            [(ngModel)]="router.location"
            name="location"
            class="form-control"
            placeholder="Oficina Central - Piso 2"
          />
        </div>
      </div>
    </div>

    <div class="alert alert-info">
      <strong>?? Información de Seguridad:</strong>
      <ul>
        <li>La contraseña se encripta con AES-256 antes de guardar</li>
        <li>NUNCA se expone la contraseña en las APIs</li>
        <li>Solo se desencripta al momento de conectar al router</li>
      </ul>
    </div>

    <button 
      type="submit" 
      class="btn btn-primary"
      [disabled]="loading || !routerForm.valid"
    >
      {{ loading ? 'Creando...' : '?? Guardar Router' }}
    </button>

    <button 
      type="button" 
      class="btn btn-secondary"
      (click)="testBeforeCreate()"
    >
      ?? Probar Conexión
    </button>
  </form>
</div>
```

---

## ?? Componente de Operaciones MikroTik

```typescript
// router-operations.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MikroTikService } from './services/mikrotik.service';

@Component({
  selector: 'app-router-operations',
  templateUrl: './router-operations.component.html'
})
export class RouterOperationsComponent implements OnInit {
  routerId!: number;
  systemResources: any = null;
  interfaces: any[] = [];
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private mikrotikService: MikroTikService
  ) {}

  ngOnInit() {
    this.routerId = +this.route.snapshot.paramMap.get('id')!;
    this.loadSystemResources();
    this.loadInterfaces();
  }

  loadSystemResources() {
    this.mikrotikService.getSystemResources(this.routerId).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          this.systemResources = response.data;
        }
      }
    });
  }

  loadInterfaces() {
    this.mikrotikService.getInterfaces(this.routerId).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          this.interfaces = response.data;
        }
      }
    });
  }

  createBridge() {
    const bridgeName = prompt('Nombre del bridge:');
    if (!bridgeName) return;

    this.mikrotikService.createBridge(this.routerId, {
      name: bridgeName,
      comment: 'Creado desde panel'
    }).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          alert('? Bridge creado: ' + response.data.name);
          this.loadInterfaces();
        }
      },
      error: (error) => {
        alert('? Error: ' + error.error?.message);
      }
    });
  }

  createVlan() {
    const vlanName = prompt('Nombre del VLAN:');
    const vlanId = prompt('VLAN ID (1-4094):');
    const interface_ = prompt('Interface (ej: ether1):');

    if (!vlanName || !vlanId || !interface_) return;

    this.mikrotikService.createVlan(this.routerId, {
      name: vlanName,
      vlanId: parseInt(vlanId),
      interface: interface_
    }).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          alert('? VLAN creado: ' + response.data.name);
          this.loadInterfaces();
        }
      },
      error: (error) => {
        alert('? Error: ' + error.error?.message);
      }
    });
  }

  getCpuLoadClass(): string {
    if (!this.systemResources) return '';
    const load = this.systemResources.cpuLoad;
    if (load < 30) return 'text-success';
    if (load < 70) return 'text-warning';
    return 'text-danger';
  }

  formatBytes(bytes: string): string {
    const num = parseInt(bytes);
    if (num < 1024) return num + ' B';
    if (num < 1024 * 1024) return (num / 1024).toFixed(2) + ' KB';
    if (num < 1024 * 1024 * 1024) return (num / 1024 / 1024).toFixed(2) + ' MB';
    return (num / 1024 / 1024 / 1024).toFixed(2) + ' GB';
  }
}
```

```html
<!-- router-operations.component.html -->
<div class="router-operations">
  <h2>Operaciones del Router</h2>

  <!-- Recursos del Sistema -->
  <div class="card" *ngIf="systemResources">
    <h3>?? Recursos del Sistema</h3>
    <div class="resources-grid">
      <div class="resource-item">
        <span class="label">Uptime:</span>
        <span class="value">{{ systemResources.uptime }}</span>
      </div>
      <div class="resource-item">
        <span class="label">Versión:</span>
        <span class="value">{{ systemResources.version }}</span>
      </div>
      <div class="resource-item">
        <span class="label">CPU:</span>
        <span [class]="getCpuLoadClass()">
          {{ systemResources.cpuLoad }}%
        </span>
      </div>
      <div class="resource-item">
        <span class="label">Memoria:</span>
        <span class="value">
          {{ formatBytes(systemResources.freeMemory) }} / 
          {{ formatBytes(systemResources.totalMemory) }}
        </span>
      </div>
    </div>
  </div>

  <!-- Interfaces -->
  <div class="card">
    <h3>?? Interfaces</h3>
    
    <button (click)="createBridge()" class="btn btn-sm btn-primary">
      ? Crear Bridge
    </button>
    <button (click)="createVlan()" class="btn btn-sm btn-primary">
      ? Crear VLAN
    </button>

    <table class="table">
      <thead>
        <tr>
          <th>Nombre</th>
          <th>Tipo</th>
          <th>Estado</th>
          <th>Comentario</th>
        </tr>
      </thead>
      <tbody>
        <tr *ngFor="let iface of interfaces">
          <td>{{ iface.name }}</td>
          <td>{{ iface.type }}</td>
          <td>
            <span [class.text-success]="iface.running" 
                  [class.text-danger]="!iface.running">
              {{ iface.running ? '? Activo' : '? Inactivo' }}
            </span>
          </td>
          <td>{{ iface.comment || '-' }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</div>
```

---

## ?? Cómo Agregar Nuevas Funcionalidades

### Arquitectura Extensible

El sistema usa el patrón **Strategy** con `IMikroTikOperation<TRequest, TResponse>` para agregar operaciones fácilmente.

### Paso 1: Definir los Modelos de Request/Response

```csharp
// En MikroClean.Domain/MikroTik/Operations/OperationModels.cs

public class CreateDhcpServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public string AddressPool { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public string Gateway { get; set; } = string.Empty;
    public string DnsServers { get; set; } = string.Empty;
}

public class DhcpServerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public bool Disabled { get; set; }
}
```

### Paso 2: Crear la Operación

```csharp
// En MikroClean.Application/MikroTik/Operations/DhcpOperations.cs

using MikroClean.Domain.MikroTik;
using tik4net.Objects;
using tik4net.Objects.Ip.Dhcp;

namespace MikroClean.Application.MikroTik.Operations
{
    public class CreateDhcpServerOperation 
        : IMikroTikOperation<CreateDhcpServerRequest, DhcpServerResponse>
    {
        public string OperationName => "CreateDhcpServer";

        public async Task<MikroTikResult<DhcpServerResponse>> ExecuteAsync(
            ITikConnection connection, 
            CreateDhcpServerRequest request)
        {
            try
            {
                var dhcpServer = new DhcpServer
                {
                    Name = request.Name,
                    Interface = request.Interface,
                    AddressPool = request.AddressPool,
                    Disabled = false
                };

                connection.Save(dhcpServer);

                // Crear DHCP Network
                var dhcpNetwork = new DhcpNetwork
                {
                    Address = request.Network,
                    Gateway = request.Gateway,
                    DnsServer = request.DnsServers
                };

                connection.Save(dhcpNetwork);

                var response = new DhcpServerResponse
                {
                    Id = dhcpServer.Id,
                    Name = dhcpServer.Name,
                    Interface = dhcpServer.Interface,
                    Disabled = dhcpServer.Disabled
                };

                return MikroTikResult<DhcpServerResponse>.Success(
                    response,
                    $"Servidor DHCP '{request.Name}' creado exitosamente"
                );
            }
            catch (Exception ex)
            {
                return MikroTikResult<DhcpServerResponse>.Failure(
                    $"Error creando DHCP server: {ex.Message}"
                );
            }
        }
    }
}
```

### Paso 3: Agregar Método en IMikroTikService

```csharp
// En MikroClean.Application/Interfaces/IMikroTikService.cs

public interface IMikroTikService
{
    // ...existing methods...

    /// <summary>
    /// Crea un servidor DHCP en un router
    /// </summary>
    Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
        int routerId, 
        CreateDhcpServerRequest request);
}
```

### Paso 4: Implementar en MikroTikService

```csharp
// En MikroClean.Application/Services/MikroTikService.cs

public async Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
    int routerId, 
    CreateDhcpServerRequest request)
{
    var operation = new CreateDhcpServerOperation();
    var result = await ExecuteOperationAsync(routerId, operation, request);
    
    return result.IsSuccess
        ? ApiResponse<DhcpServerResponse>.Success(result.Data!, result.Message)
        : ApiResponse<DhcpServerResponse>.Error(result.Message);
}
```

### Paso 5: Agregar Endpoint en Controller

```csharp
// En MikroClean.WebAPI/Controllers/MikroTikController.cs

/// <summary>
/// Crea un servidor DHCP
/// POST: api/mikrotik/routers/{routerId}/dhcp/server
/// </summary>
[HttpPost("routers/{routerId}/dhcp/server")]
public async Task<IActionResult> CreateDhcpServer(
    int routerId, 
    [FromBody] CreateDhcpServerRequest request)
{
    var response = await _mikroTikService.CreateDhcpServerAsync(routerId, request);
    return HandleResponse(response);
}
```

### Paso 6: Usar en Angular

```typescript
// En mikrotik.service.ts
createDhcpServer(routerId: number, data: any) {
  return this.http.post(
    `${this.apiUrl}/routers/${routerId}/dhcp/server`, 
    data
  );
}

// En componente
onCreateDhcp() {
  const dhcpData = {
    name: 'dhcp-lan',
    interface: 'ether2',
    addressPool: 'pool-lan',
    network: '192.168.1.0/24',
    gateway: '192.168.1.1',
    dnsServers: '8.8.8.8,8.8.4.4'
  };

  this.mikrotikService.createDhcpServer(this.routerId, dhcpData).subscribe({
    next: (response) => {
      if (response.status === 'success') {
        alert('? Servidor DHCP creado');
      }
    }
  });
}
```

---

## ?? Operaciones MikroTik Disponibles

### Implementadas Actualmente

| Operación | Endpoint | Descripción |
|-----------|----------|-------------|
| **Crear Bridge** | `POST /api/mikrotik/routers/{id}/interfaces/bridge` | Crea interfaz bridge |
| **Crear VLAN** | `POST /api/mikrotik/routers/{id}/interfaces/vlan` | Crea interfaz VLAN |
| **Listar Interfaces** | `GET /api/mikrotik/routers/{id}/interfaces` | Obtiene todas las interfaces |
| **Agregar IP** | `POST /api/mikrotik/routers/{id}/ip/address` | Asigna IP a interfaz |
| **Firewall Rule** | `POST /api/mikrotik/routers/{id}/firewall/rules` | Crea regla de firewall |
| **System Resources** | `GET /api/mikrotik/routers/{id}/system/resources` | Info del sistema |

### Operaciones Pendientes (Ejemplos de Extensión)

| Operación Sugerida | Ruta Propuesta | Dificultad |
|-------------------|----------------|------------|
| **DHCP Server** | `POST /dhcp/server` | ? Fácil |
| **NAT Rules** | `POST /firewall/nat` | ? Fácil |
| **Wireless Config** | `POST /wireless/setup` | ?? Media |
| **VPN (IPSec)** | `POST /vpn/ipsec` | ??? Alta |
| **PPPoE Server** | `POST /pppoe/server` | ?? Media |
| **Hotspot** | `POST /hotspot/setup` | ??? Alta |
| **Queue Trees** | `POST /queue/tree` | ?? Media |
| **Backup Config** | `GET /system/backup` | ?? Media |
| **Update RouterOS** | `POST /system/upgrade` | ??? Alta |
| **User Manager** | `POST /user-manager/users` | ?? Media |

---

## ?? Flujo Completo - Crear y Operar Router

### 1. Crear Router

```typescript
const newRouter: CreateRouterDTO = {
  name: "Router Oficina",
  ip: "192.168.1.1",
  user: "admin",
  password: "SecurePass123!",
  model: "RB4011iGS+",
  location: "Oficina Central",
  organizationId: 1
};

routerService.create(newRouter).subscribe({
  next: (response) => {
    const routerId = response.data.id;
    console.log('? Router creado:', routerId);
  }
});
```

### 2. Probar Conexión

```typescript
routerService.testConnection(routerId).subscribe({
  next: (response) => {
    if (response.data) {
      console.log('? Conexión exitosa');
      // Continuar con operaciones
    } else {
      console.log('? Sin conexión');
    }
  }
});
```

### 3. Configurar Interfaces

```typescript
// Crear Bridge
mikrotikService.createBridge(routerId, {
  name: 'bridge-lan',
  comment: 'Bridge para LAN'
}).subscribe();

// Crear VLAN para invitados
mikrotikService.createVlan(routerId, {
  name: 'vlan-invitados',
  vlanId: 100,
  interface: 'bridge-lan'
}).subscribe();
```

### 4. Configurar IPs

```typescript
mikrotikService.addIpAddress(routerId, {
  address: '192.168.1.1/24',
  interface: 'bridge-lan',
  network: '192.168.1.0',
  comment: 'Gateway LAN'
}).subscribe();
```

### 5. Configurar Firewall

```typescript
mikrotikService.createFirewallRule(routerId, {
  chain: 'forward',
  action: 'accept',
  protocol: 'tcp',
  dstPort: '80,443',
  comment: 'Permitir web'
}).subscribe();
```

---

## ?? Casos de Uso Avanzados

### Caso 1: Setup Inicial de Router

```typescript
async setupNewRouter(routerId: number) {
  try {
    // 1. Test conexión
    await this.testConnection(routerId);

    // 2. Crear bridge
    await this.createBridge(routerId, 'bridge-lan');

    // 3. Asignar IP al bridge
    await this.addIpToBridge(routerId, '192.168.1.1/24');

    // 4. Configurar firewall básico
    await this.setupBasicFirewall(routerId);

    // 5. Crear VLAN para invitados
    await this.createGuestVlan(routerId);

    alert('? Router configurado exitosamente');
  } catch (error) {
    alert('? Error en configuración: ' + error);
  }
}
```

### Caso 2: Monitoreo en Tiempo Real

```typescript
export class RouterMonitorComponent implements OnInit, OnDestroy {
  private interval: any;

  ngOnInit() {
    this.loadResources();
    // Actualizar cada 30 segundos
    this.interval = setInterval(() => {
      this.loadResources();
    }, 30000);
  }

  ngOnDestroy() {
    if (this.interval) {
      clearInterval(this.interval);
    }
  }

  loadResources() {
    this.mikrotikService.getSystemResources(this.routerId).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          this.updateResourcesChart(response.data);
        }
      }
    });
  }

  updateResourcesChart(data: any) {
    // Actualizar gráficas de CPU, memoria, etc.
    this.cpuChart.data = data.cpuLoad;
    this.memoryChart.data = data.freeMemory;
  }
}
```

### Caso 3: Operación en Múltiples Routers

```typescript
// Ejecutar backup en todos los routers de la organización
warmUpAndBackup(organizationId: number) {
  // 1. Pre-calentar conexiones
  this.mikrotikService.warmUpConnections(organizationId).subscribe({
    next: (response) => {
      const connectedRouters = Object.keys(response.data)
        .filter(key => response.data[key]);

      // 2. Ejecutar backup en cada router conectado
      connectedRouters.forEach(routerId => {
        this.createBackup(parseInt(routerId));
      });
    }
  });
}
```

---

## ??? Guía de Extensión - Agregar Operación DHCP

Voy a mostrarte **paso a paso** cómo agregar una nueva funcionalidad (DHCP Server):

### ?? Paso 1: Crear los DTOs

```csharp
// MikroClean.Domain/MikroTik/Operations/OperationModels.cs
// Agregar al final del archivo:

public class CreateDhcpServerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public string AddressPool { get; set; } = string.Empty;
    public string Lease { get; set; } = "1d";
}

public class DhcpServerResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Interface { get; set; } = string.Empty;
    public string AddressPool { get; set; } = string.Empty;
    public bool Disabled { get; set; }
}
```

### ?? Paso 2: Implementar la Operación

```csharp
// MikroClean.Application/MikroTik/Operations/DhcpOperations.cs (NUEVO ARCHIVO)

using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;
using tik4net;
using tik4net.Objects.Ip.Dhcp;

namespace MikroClean.Application.MikroTik.Operations
{
    public class CreateDhcpServerOperation 
        : IMikroTikOperation<CreateDhcpServerRequest, DhcpServerResponse>
    {
        public string OperationName => "CreateDhcpServer";

        public async Task<MikroTikResult<DhcpServerResponse>> ExecuteAsync(
            ITikConnection connection, 
            CreateDhcpServerRequest request)
        {
            try
            {
                // Crear el servidor DHCP
                var dhcpServer = new DhcpServer
                {
                    Name = request.Name,
                    Interface = request.Interface,
                    AddressPool = request.AddressPool,
                    LeaseTime = request.Lease,
                    Disabled = false
                };

                // Guardar en MikroTik
                connection.Save(dhcpServer);

                // Mapear respuesta
                var response = new DhcpServerResponse
                {
                    Id = dhcpServer.Id,
                    Name = dhcpServer.Name,
                    Interface = dhcpServer.Interface,
                    AddressPool = dhcpServer.AddressPool,
                    Disabled = dhcpServer.Disabled
                };

                return MikroTikResult<DhcpServerResponse>.Success(
                    response,
                    $"Servidor DHCP '{request.Name}' creado exitosamente"
                );
            }
            catch (Exception ex)
            {
                return MikroTikResult<DhcpServerResponse>.Failure(
                    $"Error al crear servidor DHCP: {ex.Message}"
                );
            }
        }
    }

    public class ListDhcpServersOperation 
        : IMikroTikOperation<object, List<DhcpServerResponse>>
    {
        public string OperationName => "ListDhcpServers";

        public async Task<MikroTikResult<List<DhcpServerResponse>>> ExecuteAsync(
            ITikConnection connection, 
            object request)
        {
            try
            {
                var servers = connection.LoadList<DhcpServer>();

                var response = servers.Select(s => new DhcpServerResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Interface = s.Interface,
                    AddressPool = s.AddressPool,
                    Disabled = s.Disabled
                }).ToList();

                return MikroTikResult<List<DhcpServerResponse>>.Success(
                    response,
                    $"Se encontraron {response.Count} servidores DHCP"
                );
            }
            catch (Exception ex)
            {
                return MikroTikResult<List<DhcpServerResponse>>.Failure(
                    $"Error listando servidores DHCP: {ex.Message}"
                );
            }
        }
    }
}
```

### ?? Paso 3: Agregar a la Interface

```csharp
// MikroClean.Application/Interfaces/IMikroTikService.cs

public interface IMikroTikService
{
    // ...existing methods...

    // ============= DHCP =============
    
    /// <summary>
    /// Crea un servidor DHCP
    /// </summary>
    Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
        int routerId, 
        CreateDhcpServerRequest request);

    /// <summary>
    /// Lista servidores DHCP
    /// </summary>
    Task<ApiResponse<List<DhcpServerResponse>>> GetDhcpServersAsync(int routerId);
}
```

### ?? Paso 4: Implementar en el Service

```csharp
// MikroClean.Application/Services/MikroTikService.cs

public async Task<ApiResponse<DhcpServerResponse>> CreateDhcpServerAsync(
    int routerId, 
    CreateDhcpServerRequest request)
{
    var operation = new CreateDhcpServerOperation();
    var result = await ExecuteOperationAsync(routerId, operation, request);
    
    return result.IsSuccess
        ? ApiResponse<DhcpServerResponse>.Success(result.Data!, result.Message)
        : ApiResponse<DhcpServerResponse>.Error(result.Message);
}

public async Task<ApiResponse<List<DhcpServerResponse>>> GetDhcpServersAsync(int routerId)
{
    var operation = new ListDhcpServersOperation();
    var result = await ExecuteOperationAsync(routerId, operation, new object());
    
    return result.IsSuccess
        ? ApiResponse<List<DhcpServerResponse>>.Success(result.Data!, result.Message)
        : ApiResponse<List<DhcpServerResponse>>.Error(result.Message);
}
```

### ?? Paso 5: Agregar Endpoints

```csharp
// MikroClean.WebAPI/Controllers/MikroTikController.cs

// ============= DHCP =============

/// <summary>
/// Crea un servidor DHCP
/// POST: api/mikrotik/routers/{routerId}/dhcp/server
/// </summary>
[HttpPost("routers/{routerId}/dhcp/server")]
[ProducesResponseType(typeof(ApiResponse<DhcpServerResponse>), 200)]
public async Task<IActionResult> CreateDhcpServer(
    int routerId, 
    [FromBody] CreateDhcpServerRequest request)
{
    var response = await _mikroTikService.CreateDhcpServerAsync(routerId, request);
    return HandleResponse(response);
}

/// <summary>
/// Lista servidores DHCP
/// GET: api/mikrotik/routers/{routerId}/dhcp/servers
/// </summary>
[HttpGet("routers/{routerId}/dhcp/servers")]
[ProducesResponseType(typeof(ApiResponse<List<DhcpServerResponse>>), 200)]
public async Task<IActionResult> GetDhcpServers(int routerId)
{
    var response = await _mikroTikService.GetDhcpServersAsync(routerId);
    return HandleResponse(response);
}
```

### ?? Paso 6: Agregar al Service Angular

```typescript
// mikrotik.service.ts

// DHCP Operations
createDhcpServer(routerId: number, data: CreateDhcpServerRequest) {
  return this.http.post(
    `${this.apiUrl}/routers/${routerId}/dhcp/server`,
    data
  );
}

getDhcpServers(routerId: number) {
  return this.http.get(
    `${this.apiUrl}/routers/${routerId}/dhcp/servers`
  );
}
```

### ?? Paso 7: Crear Componente Angular

```typescript
// dhcp-config.component.ts
import { Component } from '@angular/core';
import { MikroTikService } from './services/mikrotik.service';

@Component({
  selector: 'app-dhcp-config',
  template: `
    <div class="dhcp-config">
      <h3>Configurar Servidor DHCP</h3>
      
      <form (ngSubmit)="onSubmit()">
        <input [(ngModel)]="dhcpData.name" placeholder="Nombre" required />
        <input [(ngModel)]="dhcpData.interface" placeholder="Interface" required />
        <input [(ngModel)]="dhcpData.addressPool" placeholder="Pool" required />
        
        <button type="submit">Crear DHCP Server</button>
      </form>

      <div *ngFor="let server of dhcpServers">
        {{ server.name }} - {{ server.interface }}
      </div>
    </div>
  `
})
export class DhcpConfigComponent {
  dhcpData = {
    name: '',
    interface: '',
    addressPool: '',
    lease: '1d'
  };

  dhcpServers: any[] = [];

  constructor(private mikrotikService: MikroTikService) {
    this.loadServers();
  }

  loadServers() {
    this.mikrotikService.getDhcpServers(this.routerId).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          this.dhcpServers = response.data;
        }
      }
    });
  }

  onSubmit() {
    this.mikrotikService.createDhcpServer(this.routerId, this.dhcpData).subscribe({
      next: (response) => {
        if (response.status === 'success') {
          alert('? Servidor DHCP creado');
          this.loadServers();
        }
      },
      error: (error) => {
        alert('? Error: ' + error.error?.message);
      }
    });
  }
}
```

---

## ?? Ejemplos de Operaciones Comunes

### Setup Completo de Red

```typescript
async setupCompleteNetwork(routerId: number) {
  // 1. Crear bridge para LAN
  await this.mikrotikService.createBridge(routerId, {
    name: 'bridge-lan',
    comment: 'Bridge principal LAN'
  }).toPromise();

  // 2. Asignar IP al bridge
  await this.mikrotikService.addIpAddress(routerId, {
    address: '192.168.1.1/24',
    interface: 'bridge-lan',
    network: '192.168.1.0'
  }).toPromise();

  // 3. Crear VLAN para invitados
  await this.mikrotikService.createVlan(routerId, {
    name: 'vlan-guest',
    vlanId: 100,
    interface: 'bridge-lan'
  }).toPromise();

  // 4. IP para VLAN invitados
  await this.mikrotikService.addIpAddress(routerId, {
    address: '192.168.100.1/24',
    interface: 'vlan-guest',
    network: '192.168.100.0'
  }).toPromise();

  // 5. Reglas de firewall
  await this.mikrotikService.createFirewallRule(routerId, {
    chain: 'forward',
    action: 'accept',
    srcAddress: '192.168.1.0/24',
    comment: 'Permitir LAN a internet'
  }).toPromise();

  alert('? Red configurada completamente');
}
```

---

## ?? Seguridad y Mejores Prácticas

### 1. Contraseñas
- ? **SIEMPRE** se encriptan con AES-256
- ? **NUNCA** se exponen en APIs
- ? **Solo** se desencriptan al conectar
- ? Se re-encriptan al actualizar

### 2. Validación de IPs
- ? IP única en todo el sistema
- ? Formato IP válido
- ? No permitir IPs duplicadas

### 3. Gestión de Conexiones
- ? Pool de conexiones con límite (100)
- ? Timeout de inactividad (15 min)
- ? Reconexión automática en errores
- ? Cierre automático al eliminar router

### 4. Permisos
- ? Validar organización del usuario
- ? Solo acceder a routers de su organización
- ? SuperAdmin puede acceder a todos

---

## ?? Mejoras Futuras Sugeridas

### 1. Paginación de Routers

```csharp
Task<ApiResponse<PagedResult<RouterDTO>>> GetRoutersPagedAsync(
    int organizationId,
    PaginationParams paginationParams);
```

### 2. Filtros Avanzados

```csharp
Task<ApiResponse<IEnumerable<RouterDTO>>> GetRoutersByStatusAsync(
    int organizationId, 
    bool isActive);

Task<ApiResponse<IEnumerable<RouterDTO>>> GetRoutersWithoutConnectionAsync(
    int organizationId);
```

### 3. Operaciones Batch

```csharp
Task<ApiResponse<Dictionary<int, bool>>> RebootMultipleRoutersAsync(
    int organizationId, 
    int[] routerIds);
```

### 4. Webhooks/Notificaciones

```csharp
Task<ApiResponse<bool>> SubscribeToRouterEventsAsync(
    int routerId, 
    string webhookUrl);
```

### 5. Templates de Configuración

```csharp
Task<ApiResponse<bool>> ApplyConfigTemplateAsync(
    int routerId, 
    string templateName);
```

---

## ? Checklist de Integración Frontend

### Módulo de Routers
- [ ] Implementar RouterService con todos los endpoints
- [ ] Crear componente de lista de routers
- [ ] Implementar formulario de creación
- [ ] Implementar formulario de edición
- [ ] Agregar test de conexión con indicador visual
- [ ] Mostrar estado de conexión en tiempo real
- [ ] Implementar confirmación antes de eliminar

### Operaciones MikroTik
- [ ] Implementar MikroTikService
- [ ] Crear componente de operaciones por router
- [ ] Agregar formularios para crear Bridge, VLAN, etc.
- [ ] Mostrar recursos del sistema (CPU, RAM)
- [ ] Listar interfaces del router
- [ ] Configurar reglas de firewall
- [ ] Implementar gestión de IPs

### Seguridad
- [ ] NUNCA mostrar contraseñas en UI
- [ ] Validar IPs antes de enviar
- [ ] Manejar errores de conexión
- [ ] Implementar retry automático
- [ ] Mostrar alertas cuando conexión falla

---

## ?? Roadmap de Funcionalidades

### Fase 1: CRUD Básico (? Completado)
- ? Crear router
- ? Listar routers
- ? Actualizar router
- ? Eliminar router
- ? Test de conexión

### Fase 2: Operaciones Básicas (? Completado)
- ? Interfaces (Bridge, VLAN)
- ? IP Address
- ? Firewall
- ? System Resources

### Fase 3: Operaciones Avanzadas (?? Pendiente)
- [ ] DHCP Server
- [ ] NAT Rules
- [ ] Port Forwarding
- [ ] Wireless Configuration
- [ ] Queue Management (QoS)

### Fase 4: Características Premium (?? Pendiente)
- [ ] VPN (IPSec, OpenVPN, WireGuard)
- [ ] Hotspot
- [ ] User Manager
- [ ] RADIUS Integration
- [ ] Backup/Restore
- [ ] RouterOS Upgrade

### Fase 5: Monitoreo y Analytics (?? Pendiente)
- [ ] Gráficas en tiempo real
- [ ] Alertas proactivas
- [ ] Logs centralizados
- [ ] Reportes de uso
- [ ] Dashboard de métricas

---

## ?? Arquitectura de Operaciones

### Diagrama de Clases

```
???????????????????????????????????????????
?     IMikroTikOperation<TReq, TRes>      ?
?  + OperationName: string                ?
?  + ExecuteAsync(): MikroTikResult<T>    ?
???????????????????????????????????????????
                ?
                ? Implementan
                ?
    ???????????????????????????????????????
    ?                      ?              ?
??????????????????  ????????????????  ?????????????????
? CreateBridge   ?  ? CreateVlan   ?  ? CreateDhcp    ?
? Operation      ?  ? Operation    ?  ? Operation     ?
??????????????????  ????????????????  ?????????????????
```

### Flujo de Ejecución

```
Request from Frontend
        ?
        ?
    Controller
        ?
        ?
    MikroTikService
        ?
        ?
    ExecuteOperationAsync()
        ?
        ???? Get Router from DB
        ???? Decrypt Password
        ???? Get/Create Connection
        ???? Execute Operation
        ???? Return Result
        ?
        ?
    Response to Frontend
```

---

## ?? Estructura de Archivos Recomendada

```
MikroClean.Application/
??? MikroTik/
?   ??? Operations/
?   ?   ??? InterfaceOperations.cs     ? Implementado
?   ?   ??? IpOperations.cs            ? Implementado
?   ?   ??? FirewallOperations.cs      ? Implementado
?   ?   ??? SystemOperations.cs        ? Implementado
?   ?   ??? DhcpOperations.cs          ?? Ejemplo en docs
?   ?   ??? NatOperations.cs           ?? Pendiente
?   ?   ??? WirelessOperations.cs      ?? Pendiente
?   ?   ??? VpnOperations.cs           ?? Pendiente
?   ?   ??? HotspotOperations.cs       ?? Pendiente
?   ??? Validators/
?       ??? IpValidator.cs
?       ??? ConfigValidator.cs
??? Services/
?   ??? RouterService.cs               ? CRUD
?   ??? MikroTikService.cs             ? Operaciones
??? Interfaces/
    ??? IRouterService.cs
    ??? IMikroTikService.cs
```

---

## ?? Quick Start - Agregar Nueva Funcionalidad

### Template Rápido

```csharp
// 1. DTOs
public class CreateXXXRequest { /* propiedades */ }
public class XXXResponse { /* propiedades */ }

// 2. Operation
public class CreateXXXOperation : IMikroTikOperation<CreateXXXRequest, XXXResponse>
{
    public string OperationName => "CreateXXX";
    
    public async Task<MikroTikResult<XXXResponse>> ExecuteAsync(
        ITikConnection connection, 
        CreateXXXRequest request)
    {
        // Implementar lógica con tik4net
    }
}

// 3. Interface
Task<ApiResponse<XXXResponse>> CreateXXXAsync(int routerId, CreateXXXRequest request);

// 4. Service Implementation
public async Task<ApiResponse<XXXResponse>> CreateXXXAsync(...)
{
    var operation = new CreateXXXOperation();
    return await ExecuteOperationAsync(routerId, operation, request);
}

// 5. Controller
[HttpPost("routers/{routerId}/xxx")]
public async Task<IActionResult> CreateXXX(int routerId, [FromBody] CreateXXXRequest request)
{
    return HandleResponse(await _mikroTikService.CreateXXXAsync(routerId, request));
}

// 6. Angular
createXXX(routerId: number, data: any) {
  return this.http.post(`${apiUrl}/routers/${routerId}/xxx`, data);
}
```

---

## ?? Resumen

### Arquitectura del Módulo de Routers

1. **RoutersController**: CRUD básico de routers
2. **MikroTikController**: Operaciones específicas de MikroTik
3. **Patrón Strategy**: `IMikroTikOperation` para extensibilidad
4. **Encriptación**: Automática con AES-256
5. **Pool de Conexiones**: Gestión eficiente de conexiones
6. **Validaciones**: IP única, organización existe, password requerido

### Ventajas de la Arquitectura

- ? **Separación clara** entre CRUD y operaciones
- ? **Fácil de extender** con nuevas operaciones
- ? **Seguridad incorporada** (encriptación automática)
- ? **Eficiencia** (pool de conexiones)
- ? **Mantenible** (cada operación en su clase)
- ? **Testeable** (inyección de dependencias)

### Para Agregar Nueva Funcionalidad

1. **Crear DTOs** (Request/Response)
2. **Implementar Operation** (`IMikroTikOperation`)
3. **Agregar a Interface**
4. **Implementar en Service**
5. **Crear Endpoint en Controller**
6. **Agregar a Angular Service**
7. **Crear UI Component**

**¡Todo listo para escalar con nuevas funcionalidades MikroTik!** ??
