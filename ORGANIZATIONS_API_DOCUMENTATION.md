# Documentación API - Módulo de Organizaciones

## ?? Información General

El módulo de organizaciones permite gestionar empresas/organizaciones dentro del sistema MikroClean, incluyendo la creación automática de usuarios administradores y licencias.

---

## ?? Base URL

```
http://localhost:5000/api/organizations
```

---

## ?? Endpoints Disponibles

### 1. Obtener Todas las Organizaciones

**GET** `/api/organizations`

Obtiene todas las organizaciones activas (sin paginación).

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Se encontraron 5 organizaciones",
  "data": [
    {
      "id": 1,
      "name": "TechCorp",
      "email": "info@techcorp.com",
      "phone": "+1234567890",
      "createdAt": "2024-03-15T10:30:00Z",
      "license": {
        "id": 1,
        "key": "ABCD1234-EFGH5678-IJKL9012-MNOP3456",
        "type": "Trial",
        "startDate": "2024-03-15T10:30:00Z",
        "endDate": "2024-04-14T10:30:00Z",
        "isActive": true,
        "maxRouters": 5,
        "maxUsers": 3,
        "isExpired": false,
        "daysRemaining": 30
      }
    }
  ],
  "timestamp": "2024-03-15T10:30:00Z"
}
```

---

### 2. Obtener Organizaciones Paginadas

**GET** `/api/organizations/paged`

Obtiene organizaciones con paginación, búsqueda y ordenamiento.

#### Query Parameters

| Parámetro | Tipo | Requerido | Default | Descripción |
|-----------|------|-----------|---------|-------------|
| pageNumber | int | No | 1 | Número de página |
| pageSize | int | No | 10 | Cantidad de items por página (max: 100) |
| sortBy | string | No | "createdAt" | Campo para ordenar: `name`, `email`, `createdAt` |
| sortDescending | bool | No | false | Orden descendente |
| searchTerm | string | No | null | Busca en nombre, email y teléfono |

#### Ejemplos de Uso

```http
# Obtener primera página con 10 items
GET /api/organizations/paged?pageNumber=1&pageSize=10

# Buscar organizaciones que contengan "tech"
GET /api/organizations/paged?searchTerm=tech

# Ordenar por nombre ascendente
GET /api/organizations/paged?sortBy=name&sortDescending=false

# Combinación: búsqueda + ordenamiento + paginación
GET /api/organizations/paged?searchTerm=corp&sortBy=name&pageNumber=1&pageSize=20
```

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Se encontraron 25 organizaciones",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "TechCorp",
        "email": "info@techcorp.com",
        "phone": "+1234567890",
        "createdAt": "2024-03-15T10:30:00Z",
        "license": {
          "id": 1,
          "key": "ABCD1234-EFGH5678-IJKL9012-MNOP3456",
          "type": "Trial",
          "startDate": "2024-03-15T10:30:00Z",
          "endDate": "2024-04-14T10:30:00Z",
          "isActive": true,
          "maxRouters": 5,
          "maxUsers": 3,
          "isExpired": false,
          "daysRemaining": 30
        }
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "timestamp": "2024-03-15T10:30:00Z"
}
```

---

### 3. Obtener Organización por ID

**GET** `/api/organizations/{id}`

#### Path Parameters
- `id` (int): ID de la organización

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Organización obtenida exitosamente",
  "data": {
    "id": 1,
    "name": "TechCorp",
    "email": "info@techcorp.com",
    "phone": "+1234567890",
    "createdAt": "2024-03-15T10:30:00Z",
    "license": {
      "id": 1,
      "key": "ABCD1234-EFGH5678-IJKL9012-MNOP3456",
      "type": "Pro",
      "startDate": "2024-03-15T10:30:00Z",
      "endDate": "2025-03-15T10:30:00Z",
      "isActive": true,
      "maxRouters": 50,
      "maxUsers": 20,
      "isExpired": false,
      "daysRemaining": 365
    }
  },
  "timestamp": "2024-03-15T10:30:00Z"
}
```

#### Response 404 Not Found
```json
{
  "status": "not_found",
  "message": "Organización no encontrada",
  "data": null,
  "timestamp": "2024-03-15T10:30:00Z"
}
```

---

### 4. Crear Organización Simple

**POST** `/api/organizations`

Crea una organización sin usuario administrador ni licencia (uso interno).

#### Request Body
```json
{
  "name": "Nueva Empresa",
  "email": "contacto@nuevaempresa.com",
  "phone": "+9876543210"
}
```

#### Response 201 Created
```json
{
  "status": "success",
  "message": "Organización creada exitosamente",
  "data": {
    "id": 2,
    "name": "Nueva Empresa",
    "email": "contacto@nuevaempresa.com",
    "phone": "+9876543210",
    "createdAt": "2024-03-15T11:00:00Z",
    "license": null
  },
  "timestamp": "2024-03-15T11:00:00Z"
}
```

#### Response 400 Bad Request (Validación)
```json
{
  "status": "validation_error",
  "message": "Ya existe una organización con ese nombre",
  "data": null,
  "errors": {
    "Name": "El nombre ya está en uso"
  },
  "timestamp": "2024-03-15T11:00:00Z"
}
```

---

### 5. Crear Organización Completa con Admin y Licencia ?

**POST** `/api/organizations/with-admin`

Crea una organización completa con:
- Usuario administrador inicial
- Licencia (trial de 30 días o asocia una existente por key)
- Rol "Administrador" (se crea automáticamente si no existe)

#### Request Body (Con Licencia Trial Automática)
```json
{
  "organizationName": "TechCorp Solutions",
  "organizationEmail": "info@techcorp.com",
  "organizationPhone": "+1234567890",
  "adminUsername": "admin_techcorp",
  "adminEmail": "admin@techcorp.com",
  "adminPassword": "SecurePass123!"
}
```

#### Request Body (Con Licencia Existente)
```json
{
  "organizationName": "TechCorp Solutions",
  "organizationEmail": "info@techcorp.com",
  "organizationPhone": "+1234567890",
  "adminUsername": "admin_techcorp",
  "adminEmail": "admin@techcorp.com",
  "adminPassword": "SecurePass123!",
  "licenseKey": "ABCD1234-EFGH5678-IJKL9012-MNOP3456"
}
```

#### Validaciones
| Campo | Validación |
|-------|------------|
| organizationName | Requerido, máx 200 caracteres, único |
| organizationEmail | Requerido, email válido, máx 100 caracteres, único |
| organizationPhone | Teléfono válido, máx 20 caracteres, opcional |
| adminUsername | Requerido, máx 100 caracteres, único en sistema |
| adminEmail | Requerido, email válido, máx 100 caracteres, único en sistema |
| adminPassword | Requerido, mín 6 caracteres, máx 100 caracteres |
| licenseKey | Opcional, máx 50 caracteres, debe existir y estar disponible |

#### Response 201 Created (Con Trial)
```json
{
  "status": "success",
  "message": "Organización creada exitosamente con usuario administrador y licencia trial de 30 días",
  "data": {
    "id": 3,
    "name": "TechCorp Solutions",
    "email": "info@techcorp.com",
    "phone": "+1234567890",
    "createdAt": "2024-03-15T12:00:00Z",
    "license": {
      "id": 3,
      "key": "WXYZ9876-ABCD5432-EFGH1098-IJKL7654",
      "type": "Trial",
      "startDate": "2024-03-15T12:00:00Z",
      "endDate": "2024-04-14T12:00:00Z",
      "isActive": true,
      "maxRouters": 5,
      "maxUsers": 3,
      "isExpired": false,
      "daysRemaining": 30
    },
    "adminUser": {
      "id": 5,
      "username": "admin_techcorp",
      "email": "admin@techcorp.com",
      "isActive": true,
      "lastLogin": null,
      "organizationId": 3,
      "organizationName": "TechCorp Solutions",
      "systemRoleId": 1,
      "systemRoleName": "Administrador",
      "createdAt": "2024-03-15T12:00:00Z"
    }
  },
  "timestamp": "2024-03-15T12:00:00Z"
}
```

#### Response 201 Created (Con Licencia Existente)
```json
{
  "status": "success",
  "message": "Organización creada exitosamente con usuario administrador y licencia proporcionada",
  "data": {
    "id": 3,
    "name": "TechCorp Solutions",
    "email": "info@techcorp.com",
    "phone": "+1234567890",
    "createdAt": "2024-03-15T12:00:00Z",
    "license": {
      "id": 10,
      "key": "ABCD1234-EFGH5678-IJKL9012-MNOP3456",
      "type": "Pro",
      "startDate": "2024-03-15T00:00:00Z",
      "endDate": "2025-03-15T00:00:00Z",
      "isActive": true,
      "maxRouters": 50,
      "maxUsers": 20,
      "isExpired": false,
      "daysRemaining": 365
    },
    "adminUser": {
      "id": 5,
      "username": "admin_techcorp",
      "email": "admin@techcorp.com",
      "isActive": true,
      "lastLogin": null,
      "organizationId": 3,
      "organizationName": "TechCorp Solutions",
      "systemRoleId": 1,
      "systemRoleName": "Administrador",
      "createdAt": "2024-03-15T12:00:00Z"
    }
  },
  "timestamp": "2024-03-15T12:00:00Z"
}
```

#### Response 400 Bad Request (Validaciones Posibles)
```json
{
  "status": "validation_error",
  "message": "Ya existe una organización con ese nombre",
  "data": null,
  "errors": {
    "OrganizationName": "El nombre ya está en uso"
  },
  "timestamp": "2024-03-15T12:00:00Z"
}
```

**Otros errores de validación:**
- `"Ya existe una organización con ese email"` ? `{ "OrganizationEmail": "El email ya está en uso" }`
- `"Ya existe un usuario con ese nombre de usuario"` ? `{ "AdminUsername": "El nombre de usuario ya está en uso" }`
- `"Ya existe un usuario con ese email"` ? `{ "AdminEmail": "El email ya está en uso" }`
- `"La clave de licencia proporcionada no existe"` ? `{ "LicenseKey": "Licencia no encontrada" }`
- `"La licencia ya está asociada a otra organización"` ? `{ "LicenseKey": "Licencia ya en uso" }`
- `"La licencia no está activa"` ? `{ "LicenseKey": "Licencia inactiva" }`
- `"La licencia ha expirado"` ? `{ "LicenseKey": "Licencia expirada" }`

---

### 6. Actualizar Organización

**PUT** `/api/organizations/{id}`

#### Path Parameters
- `id` (int): ID de la organización

#### Request Body
```json
{
  "name": "TechCorp Solutions Updated",
  "email": "newemail@techcorp.com",
  "phone": "+1234567899"
}
```

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Organización actualizada exitosamente",
  "data": {
    "id": 1,
    "name": "TechCorp Solutions Updated",
    "email": "newemail@techcorp.com",
    "phone": "+1234567899",
    "createdAt": "2024-03-15T10:30:00Z",
    "license": {
      "id": 1,
      "key": "ABCD1234-EFGH5678-IJKL9012-MNOP3456",
      "type": "Pro",
      "startDate": "2024-03-15T10:30:00Z",
      "endDate": "2025-03-15T10:30:00Z",
      "isActive": true,
      "maxRouters": 50,
      "maxUsers": 20,
      "isExpired": false,
      "daysRemaining": 365
    }
  },
  "timestamp": "2024-03-15T13:00:00Z"
}
```

#### Response 404 Not Found
```json
{
  "status": "not_found",
  "message": "Organización no encontrada",
  "data": null,
  "timestamp": "2024-03-15T13:00:00Z"
}
```

---

### 7. Eliminar Organización (Soft Delete)

**DELETE** `/api/organizations/{id}`

#### Path Parameters
- `id` (int): ID de la organización

#### Response 200 OK
```json
{
  "status": "success",
  "message": "Organización eliminada exitosamente",
  "data": true,
  "timestamp": "2024-03-15T14:00:00Z"
}
```

#### Response 404 Not Found
```json
{
  "status": "not_found",
  "message": "Organización no encontrada",
  "data": false,
  "timestamp": "2024-03-15T14:00:00Z"
}
```

---

## ?? Modelos de Datos

### OrganizationDTO
```typescript
{
  id: number;
  name: string;
  email: string;
  phone: string;
  createdAt: string; // ISO 8601
  license: LicenseDTO | null;
}
```

### OrganizationWithAdminDTO
```typescript
{
  id: number;
  name: string;
  email: string;
  phone: string;
  createdAt: string; // ISO 8601
  license: LicenseDTO | null;
  adminUser: UserDTO | null;
}
```

### LicenseDTO
```typescript
{
  id: number;
  key: string;
  type: "Basic" | "Pro" | "Enterprise" | "Trial"; // 1, 2, 3, 4
  startDate: string; // ISO 8601
  endDate: string; // ISO 8601
  isActive: boolean;
  maxRouters: number | null;
  maxUsers: number | null;
  isExpired: boolean; // calculado
  daysRemaining: number; // calculado
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

### PagedResult<T>
```typescript
{
  items: T[]; // Array de items del tipo especificado
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number; // calculado
  hasPreviousPage: boolean; // calculado
  hasNextPage: boolean; // calculado
}
```

---

## ?? Tipos de Licencia

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 1 | Basic | Licencia básica |
| 2 | Pro | Licencia profesional |
| 3 | Enterprise | Licencia empresarial |
| 4 | Trial | Licencia de prueba (30 días) |

---

## ?? Flujo Completo de Creación de Organización

### Opción 1: Con Licencia Trial Automática (Recomendado)

```javascript
// 1. Crear organización con admin
const response = await fetch('http://localhost:5000/api/organizations/with-admin', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    organizationName: "Mi Empresa",
    organizationEmail: "contacto@miempresa.com",
    organizationPhone: "+123456789",
    adminUsername: "admin",
    adminEmail: "admin@miempresa.com",
    adminPassword: "MiPassword123!"
  })
});

const data = await response.json();

// 2. Guardar los datos de la organización y admin
const organization = data.data;
console.log('Organización ID:', organization.id);
console.log('Admin Username:', organization.adminUser.username);
console.log('License Key:', organization.license.key);

// 3. El usuario puede hacer login inmediatamente
const loginResponse = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    usernameOrEmail: "admin",
    password: "MiPassword123!"
  })
});

const loginData = await loginResponse.json();
const token = loginData.data.token;

// 4. Usar el token para peticiones autenticadas
localStorage.setItem('authToken', token);
```

### Opción 2: Con Licencia Existente

```javascript
// 1. Primero, obtener o tener la license key disponible
const licenseKey = "ABCD1234-EFGH5678-IJKL9012-MNOP3456";

// 2. Crear organización con admin y asociar licencia
const response = await fetch('http://localhost:5000/api/organizations/with-admin', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    organizationName: "Enterprise Corp",
    organizationEmail: "info@enterprise.com",
    organizationPhone: "+9876543210",
    adminUsername: "admin_enterprise",
    adminEmail: "admin@enterprise.com",
    adminPassword: "EnterprisePass123!",
    licenseKey: licenseKey // ? Licencia existente
  })
});

const data = await response.json();
// La organización ahora tiene la licencia Pro/Enterprise asociada
```

---

## ?? Ejemplos de Búsqueda y Paginación

### JavaScript/TypeScript

```javascript
// Función helper para construir query params
function buildQueryParams(params) {
  return Object.entries(params)
    .filter(([_, value]) => value !== null && value !== undefined)
    .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
    .join('&');
}

// Obtener organizaciones paginadas
async function getOrganizations(page = 1, pageSize = 10, searchTerm = '', sortBy = 'name') {
  const params = buildQueryParams({
    pageNumber: page,
    pageSize: pageSize,
    searchTerm: searchTerm,
    sortBy: sortBy,
    sortDescending: false
  });

  const response = await fetch(
    `http://localhost:5000/api/organizations/paged?${params}`,
    {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('authToken')}`
      }
    }
  );

  return await response.json();
}

// Uso
const result = await getOrganizations(1, 20, 'tech', 'name');
console.log('Total:', result.data.totalCount);
console.log('Páginas:', result.data.totalPages);
console.log('Items:', result.data.items);
```

### Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Interceptor para agregar token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Obtener organizaciones paginadas
async function getOrganizationsPaged(params) {
  const response = await api.get('/organizations/paged', { params });
  return response.data;
}

// Uso
const result = await getOrganizationsPaged({
  pageNumber: 1,
  pageSize: 20,
  searchTerm: 'tech',
  sortBy: 'name',
  sortDescending: false
});
```

### React Hook Personalizado

```typescript
import { useState, useEffect } from 'react';

interface UsePaginatedOrganizationsParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

function usePaginatedOrganizations(params: UsePaginatedOrganizationsParams) {
  const [data, setData] = useState<PagedResult<OrganizationDTO> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchData() {
      setLoading(true);
      try {
        const queryParams = new URLSearchParams({
          pageNumber: params.pageNumber?.toString() || '1',
          pageSize: params.pageSize?.toString() || '10',
          ...(params.searchTerm && { searchTerm: params.searchTerm }),
          ...(params.sortBy && { sortBy: params.sortBy }),
          ...(params.sortDescending && { sortDescending: 'true' })
        });

        const response = await fetch(
          `http://localhost:5000/api/organizations/paged?${queryParams}`,
          {
            headers: {
              'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
          }
        );

        const result = await response.json();
        
        if (result.status === 'success') {
          setData(result.data);
        } else {
          setError(result.message);
        }
      } catch (err) {
        setError('Error al cargar organizaciones');
      } finally {
        setLoading(false);
      }
    }

    fetchData();
  }, [params.pageNumber, params.pageSize, params.searchTerm, params.sortBy, params.sortDescending]);

  return { data, loading, error };
}

// Uso en componente
function OrganizationsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  
  const { data, loading, error } = usePaginatedOrganizations({
    pageNumber: page,
    pageSize: 10,
    searchTerm: search,
    sortBy: 'name'
  });

  if (loading) return <div>Cargando...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <div>
      <input 
        value={search} 
        onChange={(e) => setSearch(e.target.value)} 
        placeholder="Buscar..."
      />
      
      {data?.items.map(org => (
        <div key={org.id}>
          <h3>{org.name}</h3>
          <p>{org.email}</p>
        </div>
      ))}
      
      <div>
        <button 
          disabled={!data?.hasPreviousPage} 
          onClick={() => setPage(page - 1)}
        >
          Anterior
        </button>
        <span>Página {data?.pageNumber} de {data?.totalPages}</span>
        <button 
          disabled={!data?.hasNextPage} 
          onClick={() => setPage(page + 1)}
        >
          Siguiente
        </button>
      </div>
    </div>
  );
}
```

---

## ?? Notas de Implementación

### Licencias Trial Automáticas
Cuando NO se proporciona `licenseKey`:
- Se crea una licencia tipo **Trial**
- Duración: **30 días**
- Límites:
  - MaxRouters: **5**
  - MaxUsers: **3**
- Key generada automáticamente con SHA256

### Asociación de Licencias Existentes
Cuando SÍ se proporciona `licenseKey`:
- La licencia debe existir en el sistema
- La licencia NO debe estar asociada a otra organización (`organizationId` debe ser 0 o null)
- La licencia debe estar activa
- La licencia NO debe estar expirada
- Se asocia la licencia a la organización recién creada

### Soft Delete
- Las organizaciones eliminadas tienen `deletedAt` != null
- No aparecen en consultas regulares
- Se pueden restaurar manualmente en la base de datos si es necesario

---

## ?? Códigos de Error Comunes

| Status Code | Status | Descripción |
|-------------|--------|-------------|
| 200 | success | Operación exitosa |
| 201 | success | Recurso creado exitosamente |
| 400 | validation_error | Error de validación de datos |
| 404 | not_found | Recurso no encontrado |
| 500 | error | Error interno del servidor |

---

## ?? Estructura de Respuesta Estándar

Todas las respuestas siguen esta estructura:

```typescript
interface ApiResponse<T> {
  status: 'success' | 'validation_error' | 'not_found' | 'unauthorized' | 'forbidden' | 'error';
  message: string;
  data: T | null;
  errors?: Record<string, string>; // Solo en validation_error
  timestamp: string; // ISO 8601
}
```

---

## ?? Flujo Completo con Frontend

```javascript
// 1. CREAR ORGANIZACIÓN
async function createOrganization() {
  const orgData = {
    organizationName: "Nueva Empresa",
    organizationEmail: "info@nuevaempresa.com",
    organizationPhone: "+123456789",
    adminUsername: "admin",
    adminEmail: "admin@nuevaempresa.com",
    adminPassword: "Password123!",
    // licenseKey: "XXXX-XXXX-XXXX-XXXX" // Opcional
  };

  const response = await fetch('http://localhost:5000/api/organizations/with-admin', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(orgData)
  });

  const result = await response.json();
  
  if (result.status === 'success') {
    console.log('? Organización creada:', result.data);
    return result.data;
  } else if (result.status === 'validation_error') {
    console.error('? Validación:', result.errors);
    // Mostrar errores en el form
    Object.entries(result.errors).forEach(([field, error]) => {
      console.log(`${field}: ${error}`);
    });
  }
}

// 2. LOGIN
async function login(username, password) {
  const response = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      usernameOrEmail: username,
      password: password
    })
  });

  const result = await response.json();
  
  if (result.status === 'success') {
    localStorage.setItem('authToken', result.data.token);
    localStorage.setItem('user', JSON.stringify(result.data.user));
    return result.data;
  }
  
  throw new Error(result.message);
}

// 3. LISTAR ORGANIZACIONES CON PAGINACIÓN
async function listOrganizations(page = 1, search = '') {
  const token = localStorage.getItem('authToken');
  const params = new URLSearchParams({
    pageNumber: page.toString(),
    pageSize: '10',
    sortBy: 'name',
    ...(search && { searchTerm: search })
  });

  const response = await fetch(
    `http://localhost:5000/api/organizations/paged?${params}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );

  return await response.json();
}

// 4. ACTUALIZAR ORGANIZACIÓN
async function updateOrganization(id, data) {
  const token = localStorage.getItem('authToken');
  
  const response = await fetch(`http://localhost:5000/api/organizations/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(data)
  });

  return await response.json();
}

// 5. ELIMINAR ORGANIZACIÓN
async function deleteOrganization(id) {
  const token = localStorage.getItem('authToken');
  
  const response = await fetch(`http://localhost:5000/api/organizations/${id}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  return await response.json();
}
```

---

## ? Checklist de Integración Frontend

- [ ] Implementar formulario de creación de organización
- [ ] Validar campos en frontend antes de enviar
- [ ] Manejar respuestas de error y mostrar mensajes al usuario
- [ ] Implementar tabla con paginación
- [ ] Agregar campo de búsqueda con debounce
- [ ] Implementar ordenamiento por columnas
- [ ] Guardar token JWT en localStorage
- [ ] Agregar interceptor/middleware para incluir token en requests
- [ ] Manejar expiración de token (401 ? redirect a login)
- [ ] Mostrar información de licencia en dashboard
- [ ] Alertar cuando la licencia esté próxima a expirar

---

## ?? Ejemplo de Componente React Completo

```typescript
import React, { useState } from 'react';

interface CreateOrgFormData {
  organizationName: string;
  organizationEmail: string;
  organizationPhone: string;
  adminUsername: string;
  adminEmail: string;
  adminPassword: string;
  licenseKey?: string;
}

function CreateOrganizationForm() {
  const [formData, setFormData] = useState<CreateOrgFormData>({
    organizationName: '',
    organizationEmail: '',
    organizationPhone: '',
    adminUsername: '',
    adminEmail: '',
    adminPassword: '',
    licenseKey: ''
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setErrors({});

    try {
      const response = await fetch('http://localhost:5000/api/organizations/with-admin', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      });

      const result = await response.json();

      if (result.status === 'success') {
        alert('? ' + result.message);
        // Redirect al login o dashboard
        window.location.href = '/login';
      } else if (result.status === 'validation_error') {
        setErrors(result.errors || {});
        alert('? ' + result.message);
      } else {
        alert('? ' + result.message);
      }
    } catch (error) {
      alert('Error de conexión');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>Crear Organización</h2>
      
      <div>
        <label>Nombre de la Organización *</label>
        <input
          name="organizationName"
          value={formData.organizationName}
          onChange={handleChange}
          required
        />
        {errors.OrganizationName && <span className="error">{errors.OrganizationName}</span>}
      </div>

      <div>
        <label>Email de la Organización *</label>
        <input
          type="email"
          name="organizationEmail"
          value={formData.organizationEmail}
          onChange={handleChange}
          required
        />
        {errors.OrganizationEmail && <span className="error">{errors.OrganizationEmail}</span>}
      </div>

      <div>
        <label>Teléfono</label>
        <input
          type="tel"
          name="organizationPhone"
          value={formData.organizationPhone}
          onChange={handleChange}
        />
      </div>

      <hr />

      <div>
        <label>Username del Admin *</label>
        <input
          name="adminUsername"
          value={formData.adminUsername}
          onChange={handleChange}
          required
        />
        {errors.AdminUsername && <span className="error">{errors.AdminUsername}</span>}
      </div>

      <div>
        <label>Email del Admin *</label>
        <input
          type="email"
          name="adminEmail"
          value={formData.adminEmail}
          onChange={handleChange}
          required
        />
        {errors.AdminEmail && <span className="error">{errors.AdminEmail}</span>}
      </div>

      <div>
        <label>Contraseña del Admin *</label>
        <input
          type="password"
          name="adminPassword"
          value={formData.adminPassword}
          onChange={handleChange}
          minLength={6}
          required
        />
      </div>

      <hr />

      <div>
        <label>Clave de Licencia (Opcional)</label>
        <input
          name="licenseKey"
          value={formData.licenseKey}
          onChange={handleChange}
          placeholder="XXXX-XXXX-XXXX-XXXX"
        />
        {errors.LicenseKey && <span className="error">{errors.LicenseKey}</span>}
        <small>Deja vacío para licencia trial de 30 días</small>
      </div>

      <button type="submit" disabled={loading}>
        {loading ? 'Creando...' : 'Crear Organización'}
      </button>
    </form>
  );
}
```

---

Esta documentación cubre todo el módulo de organizaciones con ejemplos completos para el frontend. ??
