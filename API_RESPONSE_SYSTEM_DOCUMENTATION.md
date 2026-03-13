# Sistema de Respuestas API - ApiResponse

## ?? Descripción

El sistema utiliza una clase genérica `ApiResponse<T>` para estandarizar todas las respuestas de la API, proporcionando una estructura consistente para el manejo de éxitos, errores y validaciones.

---

## ??? Estructura de ApiResponse

### Definición TypeScript
```typescript
interface ApiResponse<T> {
  status: ResponseStatus;
  message: string;
  data: T | null;
  errors?: Record<string, string>;
  timestamp: string; // ISO 8601
}

type ResponseStatus = 
  | 'success'
  | 'validation_error'
  | 'not_found'
  | 'unauthorized'
  | 'forbidden'
  | 'error';
```

### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| **status** | string | Estado de la respuesta (ver tipos abajo) |
| **message** | string | Mensaje descriptivo para el usuario |
| **data** | T \| null | Datos de la respuesta (tipo genérico) |
| **errors** | object | Diccionario de errores de validación (opcional) |
| **timestamp** | string | Fecha/hora de la respuesta en UTC |

---

## ?? Tipos de Status

### 1. Success (200, 201)
**Descripción**: Operación completada exitosamente

**HTTP Status**: 200 OK, 201 Created

**Ejemplo**:
```json
{
  "status": "success",
  "message": "Organización creada exitosamente",
  "data": {
    "id": 1,
    "name": "TechCorp"
  },
  "timestamp": "2024-03-15T10:30:00Z"
}
```

**Cuándo se usa**:
- Creación exitosa de recursos
- Actualización exitosa
- Consultas exitosas
- Eliminación exitosa

---

### 2. Validation Error (400)
**Descripción**: Los datos enviados no cumplen las validaciones

**HTTP Status**: 400 Bad Request

**Ejemplo**:
```json
{
  "status": "validation_error",
  "message": "Ya existe una organización con ese nombre",
  "data": null,
  "errors": {
    "OrganizationName": "El nombre ya está en uso",
    "OrganizationEmail": "El email ya está en uso"
  },
  "timestamp": "2024-03-15T10:30:00Z"
}
```

**Cuándo se usa**:
- Validaciones de modelo fallidas
- Datos duplicados (emails, usernames, etc.)
- Reglas de negocio no cumplidas
- Formato de datos inválido

**Manejo en Frontend**:
```javascript
if (response.status === 'validation_error') {
  // Mostrar errores específicos en cada campo
  Object.entries(response.errors || {}).forEach(([field, error]) => {
    showFieldError(field, error);
  });
  
  // O mostrar mensaje general
  showToast(response.message, 'error');
}
```

---

### 3. Not Found (404)
**Descripción**: El recurso solicitado no existe

**HTTP Status**: 404 Not Found

**Ejemplo**:
```json
{
  "status": "not_found",
  "message": "Organización no encontrada",
  "data": null,
  "timestamp": "2024-03-15T10:30:00Z"
}
```

**Cuándo se usa**:
- GET de un recurso que no existe
- PUT/DELETE de un recurso inexistente
- Recurso eliminado (soft delete)

**Manejo en Frontend**:
```javascript
if (response.status === 'not_found') {
  showToast(response.message, 'warning');
  redirectTo('/organizations'); // Redirigir a lista
}
```

---

### 4. Unauthorized (401)
**Descripción**: Usuario no autenticado o token inválido

**HTTP Status**: 401 Unauthorized

**Ejemplo**:
```json
{
  "status": "unauthorized",
  "message": "Token inválido o expirado",
  "data": null,
  "timestamp": "2024-03-15T10:30:00Z"
}
```

**Cuándo se usa**:
- Token JWT expirado
- Token inválido o malformado
- Sin token en endpoints protegidos

**Manejo en Frontend**:
```javascript
if (response.status === 'unauthorized') {
  // Limpiar sesión
  localStorage.removeItem('authToken');
  localStorage.removeItem('user');
  
  // Redirigir a login
  window.location.href = '/login';
  
  showToast('Sesión expirada. Por favor inicia sesión nuevamente', 'warning');
}
```

---

### 5. Forbidden (403)
**Descripción**: Usuario autenticado pero sin permisos

**HTTP Status**: 403 Forbidden

**Ejemplo**:
```json
{
  "status": "forbidden",
  "message": "No tienes permisos para realizar esta acción",
  "data": null,
  "timestamp": "2024-03-15T10:30:00Z"
}
```

**Cuándo se usa**:
- Usuario sin permisos para la acción
- Intentar acceder a recursos de otra organización

**Manejo en Frontend**:
```javascript
if (response.status === 'forbidden') {
  showToast(response.message, 'error');
  // Opcionalmente redirigir
  history.back();
}
```

---

### 6. Error (500)
**Descripción**: Error interno del servidor

**HTTP Status**: 500 Internal Server Error

**Ejemplo**:
```json
{
  "status": "error",
  "message": "Error al crear la organización: Connection timeout",
  "data": null,
  "timestamp": "2024-03-15T10:30:00Z"
}
```

**Cuándo se usa**:
- Errores de base de datos
- Excepciones no controladas
- Problemas de conexión

**Manejo en Frontend**:
```javascript
if (response.status === 'error') {
  console.error('Server error:', response.message);
  showToast('Ocurrió un error. Por favor intenta nuevamente', 'error');
  
  // Opcional: enviar a sistema de logging
  logError({
    endpoint: url,
    error: response.message,
    timestamp: response.timestamp
  });
}
```

---

## ?? Manejo Centralizado en Frontend

### Axios Interceptor

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api'
});

// Request interceptor - agregar token
api.interceptors.request.use(
  config => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error)
);

// Response interceptor - manejar errores globalmente
api.interceptors.response.use(
  response => {
    const data = response.data;
    
    // Si es error de validación, extraer y formatear errores
    if (data.status === 'validation_error') {
      const formattedErrors = {};
      Object.entries(data.errors || {}).forEach(([key, value]) => {
        formattedErrors[key] = value;
      });
      return Promise.reject({ 
        type: 'validation', 
        message: data.message, 
        errors: formattedErrors 
      });
    }
    
    return response;
  },
  error => {
    if (error.response) {
      const data = error.response.data;
      
      // Manejar unauthorized
      if (data.status === 'unauthorized') {
        localStorage.removeItem('authToken');
        window.location.href = '/login';
        return Promise.reject({ type: 'unauthorized', message: data.message });
      }
      
      // Manejar not found
      if (data.status === 'not_found') {
        return Promise.reject({ type: 'not_found', message: data.message });
      }
      
      // Manejar server error
      if (data.status === 'error') {
        return Promise.reject({ type: 'server_error', message: data.message });
      }
    }
    
    return Promise.reject({ type: 'network_error', message: 'Error de conexión' });
  }
);

export default api;
```

### Hook Personalizado para Manejo de Errores

```typescript
import { useState } from 'react';

interface ApiError {
  type: 'validation' | 'unauthorized' | 'not_found' | 'server_error' | 'network_error';
  message: string;
  errors?: Record<string, string>;
}

function useApiCall<T>() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<ApiError | null>(null);
  const [data, setData] = useState<T | null>(null);

  const execute = async (apiCall: () => Promise<any>) => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiCall();
      
      if (response.data.status === 'success') {
        setData(response.data.data);
        return { success: true, data: response.data.data };
      }
      
      setError({
        type: 'validation',
        message: response.data.message,
        errors: response.data.errors
      });
      
      return { success: false, error: response.data };
    } catch (err: any) {
      setError(err);
      return { success: false, error: err };
    } finally {
      setLoading(false);
    }
  };

  const reset = () => {
    setData(null);
    setError(null);
    setLoading(false);
  };

  return { execute, loading, error, data, reset };
}

// Uso
function MyComponent() {
  const { execute, loading, error, data } = useApiCall<OrganizationWithAdminDTO>();

  const handleCreate = async () => {
    const result = await execute(() => 
      api.post('/organizations/with-admin', formData)
    );

    if (result.success) {
      alert('? Organización creada');
      navigate('/dashboard');
    } else if (result.error.type === 'validation') {
      // Mostrar errores de validación
      setFieldErrors(result.error.errors);
    }
  };

  return (
    <div>
      {loading && <Spinner />}
      {error && <ErrorAlert message={error.message} />}
      {/* ... */}
    </div>
  );
}
```

---

## ?? Patrones de Respuesta por Operación

### Creación de Recursos (POST)
```javascript
// Success ? 201 Created
{
  "status": "success",
  "message": "Recurso creado exitosamente",
  "data": { /* recurso creado */ }
}

// Validation Error ? 400
{
  "status": "validation_error",
  "message": "Datos inválidos",
  "data": null,
  "errors": { "field": "error message" }
}
```

### Consulta de Recursos (GET)
```javascript
// Success ? 200 OK
{
  "status": "success",
  "message": "Recurso obtenido exitosamente",
  "data": { /* recurso */ }
}

// Not Found ? 404
{
  "status": "not_found",
  "message": "Recurso no encontrado",
  "data": null
}
```

### Actualización de Recursos (PUT)
```javascript
// Success ? 200 OK
{
  "status": "success",
  "message": "Recurso actualizado exitosamente",
  "data": { /* recurso actualizado */ }
}

// Not Found ? 404
{
  "status": "not_found",
  "message": "Recurso no encontrado",
  "data": null
}

// Validation Error ? 400
{
  "status": "validation_error",
  "message": "Datos inválidos",
  "data": null,
  "errors": { "field": "error message" }
}
```

### Eliminación de Recursos (DELETE)
```javascript
// Success ? 200 OK
{
  "status": "success",
  "message": "Recurso eliminado exitosamente",
  "data": true
}

// Not Found ? 404
{
  "status": "not_found",
  "message": "Recurso no encontrado",
  "data": false
}
```

---

## ?? Tabla de Códigos HTTP

| HTTP Code | Status API | Cuándo Usar | Color UI |
|-----------|------------|-------------|----------|
| 200 | success | GET, PUT, DELETE exitosos | Verde ?? |
| 201 | success | POST exitoso (creación) | Verde ?? |
| 400 | validation_error | Validación fallida | Amarillo ?? |
| 401 | unauthorized | Sin autenticación | Rojo ?? |
| 403 | forbidden | Sin permisos | Naranja ?? |
| 404 | not_found | Recurso no existe | Azul ?? |
| 500 | error | Error interno | Rojo ?? |

---

## ?? Ejemplos de Manejo en Frontend

### React con Context API

```typescript
// ApiContext.tsx
import React, { createContext, useContext, ReactNode } from 'react';
import axios from 'axios';

interface ApiContextType {
  handleResponse: <T>(response: ApiResponse<T>) => void;
}

const ApiContext = createContext<ApiContextType | undefined>(undefined);

export function ApiProvider({ children }: { children: ReactNode }) {
  const handleResponse = <T,>(response: ApiResponse<T>) => {
    switch (response.status) {
      case 'success':
        // Toast verde
        showToast(response.message, 'success');
        break;
      
      case 'validation_error':
        // Toast amarillo + mostrar errores
        showToast(response.message, 'warning');
        if (response.errors) {
          Object.entries(response.errors).forEach(([field, error]) => {
            console.error(`Validation error in ${field}: ${error}`);
          });
        }
        break;
      
      case 'not_found':
        // Toast azul
        showToast(response.message, 'info');
        break;
      
      case 'unauthorized':
        // Logout y redirect
        localStorage.clear();
        window.location.href = '/login';
        showToast('Sesión expirada', 'error');
        break;
      
      case 'forbidden':
        // Toast naranja
        showToast(response.message, 'warning');
        break;
      
      case 'error':
        // Toast rojo
        showToast('Error: ' + response.message, 'error');
        break;
    }
  };

  return (
    <ApiContext.Provider value={{ handleResponse }}>
      {children}
    </ApiContext.Provider>
  );
}

export const useApi = () => {
  const context = useContext(ApiContext);
  if (!context) throw new Error('useApi must be used within ApiProvider');
  return context;
};
```

### Servicio Reutilizable

```typescript
// api.service.ts
import axios, { AxiosResponse } from 'axios';

class ApiService {
  private baseURL = 'http://localhost:5000/api';
  private axios = axios.create({ baseURL: this.baseURL });

  constructor() {
    // Agregar token a todas las requests
    this.axios.interceptors.request.use(config => {
      const token = localStorage.getItem('authToken');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Manejar respuestas
    this.axios.interceptors.response.use(
      response => this.handleSuccess(response),
      error => this.handleError(error)
    );
  }

  private handleSuccess(response: AxiosResponse) {
    return response.data;
  }

  private handleError(error: any) {
    if (error.response) {
      const apiResponse = error.response.data;
      
      if (apiResponse.status === 'unauthorized') {
        localStorage.clear();
        window.location.href = '/login';
      }
      
      return Promise.reject(apiResponse);
    }
    
    return Promise.reject({
      status: 'error',
      message: 'Error de conexión con el servidor',
      data: null
    });
  }

  // Métodos de API
  async get<T>(url: string, params?: any): Promise<ApiResponse<T>> {
    return this.axios.get(url, { params });
  }

  async post<T>(url: string, data: any): Promise<ApiResponse<T>> {
    return this.axios.post(url, data);
  }

  async put<T>(url: string, data: any): Promise<ApiResponse<T>> {
    return this.axios.put(url, data);
  }

  async delete<T>(url: string): Promise<ApiResponse<T>> {
    return this.axios.delete(url);
  }
}

export const apiService = new ApiService();

// Uso
try {
  const response = await apiService.post<OrganizationDTO>(
    '/organizations/with-admin',
    formData
  );
  
  if (response.status === 'success') {
    console.log('Organización creada:', response.data);
  }
} catch (error: any) {
  if (error.status === 'validation_error') {
    // Manejar errores de validación
    console.log('Errores:', error.errors);
  }
}
```

---

## ?? Componente de Toast/Notificación

```typescript
// Toast.tsx
import React from 'react';

type ToastType = 'success' | 'error' | 'warning' | 'info';

interface ToastProps {
  message: string;
  type: ToastType;
  onClose: () => void;
}

function Toast({ message, type, onClose }: ToastProps) {
  const colors = {
    success: 'bg-green-500',
    error: 'bg-red-500',
    warning: 'bg-yellow-500',
    info: 'bg-blue-500'
  };

  const icons = {
    success: '?',
    error: '?',
    warning: '?',
    info: '?'
  };

  return (
    <div className={`toast ${colors[type]}`}>
      <span className="icon">{icons[type]}</span>
      <span className="message">{message}</span>
      <button onClick={onClose}>?</button>
    </div>
  );
}

// Hook para manejar toasts
function useToast() {
  const [toasts, setToasts] = useState<Array<{ id: number; message: string; type: ToastType }>>([]);

  const showToast = (message: string, type: ToastType = 'info') => {
    const id = Date.now();
    setToasts(prev => [...prev, { id, message, type }]);
    
    // Auto-cerrar después de 5 segundos
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id));
    }, 5000);
  };

  const removeToast = (id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id));
  };

  return { toasts, showToast, removeToast };
}
```

---

## ?? Checklist de Manejo de Respuestas

### En cada petición debes:

- ? **Verificar el status** antes de usar los datos
- ? **Manejar errores de validación** mostrando mensajes específicos por campo
- ? **Manejar 401 (unauthorized)** limpiando sesión y redirigiendo a login
- ? **Manejar 404 (not found)** con mensajes amigables
- ? **Manejar 500 (error)** con mensajes genéricos y logging
- ? **Mostrar el mensaje** de la respuesta al usuario
- ? **Usar loading states** mientras se procesan las peticiones
- ? **Limpiar estados** después de operaciones exitosas

---

## ?? Ejemplo de Testing

```typescript
// api.test.ts
describe('API Response Handling', () => {
  it('should handle success response', async () => {
    const mockResponse = {
      status: 'success',
      message: 'Organización creada',
      data: { id: 1, name: 'Test' },
      timestamp: '2024-03-15T10:30:00Z'
    };

    // Mock fetch
    global.fetch = jest.fn(() =>
      Promise.resolve({
        json: () => Promise.resolve(mockResponse)
      } as Response)
    );

    const result = await createOrganization(data);
    
    expect(result.status).toBe('success');
    expect(result.data).toBeDefined();
    expect(result.data.id).toBe(1);
  });

  it('should handle validation error', async () => {
    const mockResponse = {
      status: 'validation_error',
      message: 'Datos inválidos',
      data: null,
      errors: {
        'OrganizationName': 'El nombre ya está en uso'
      },
      timestamp: '2024-03-15T10:30:00Z'
    };

    global.fetch = jest.fn(() =>
      Promise.resolve({
        json: () => Promise.resolve(mockResponse)
      } as Response)
    );

    const result = await createOrganization(data);
    
    expect(result.status).toBe('validation_error');
    expect(result.errors).toBeDefined();
    expect(result.errors.OrganizationName).toBe('El nombre ya está en uso');
  });
});
```

---

## ?? Debugging

### Console Logging Útil

```javascript
// Logger helper
function logApiResponse(response, operation) {
  const emoji = {
    success: '?',
    validation_error: '??',
    not_found: '??',
    unauthorized: '??',
    forbidden: '??',
    error: '?'
  };

  console.group(`${emoji[response.status]} ${operation}`);
  console.log('Status:', response.status);
  console.log('Message:', response.message);
  console.log('Data:', response.data);
  if (response.errors) {
    console.table(response.errors);
  }
  console.log('Timestamp:', response.timestamp);
  console.groupEnd();
}

// Uso
const response = await createOrganization(data);
logApiResponse(response, 'Create Organization');
```

---

## ?? Ejemplo Completo de Integración

```typescript
// OrganizationService.ts
class OrganizationService {
  private api = apiService;

  async createWithAdmin(data: CreateOrganizationWithAdminDTO) {
    try {
      const response = await this.api.post<OrganizationWithAdminDTO>(
        '/organizations/with-admin',
        data
      );

      if (response.status === 'success') {
        return {
          success: true,
          organization: response.data,
          message: response.message
        };
      }

      return {
        success: false,
        errors: response.errors,
        message: response.message
      };
    } catch (error: any) {
      return {
        success: false,
        message: error.message || 'Error desconocido'
      };
    }
  }

  async getPagedOrganizations(params: PaginationParams) {
    const response = await this.api.get<PagedResult<OrganizationDTO>>(
      '/organizations/paged',
      params
    );

    if (response.status === 'success') {
      return response.data;
    }

    throw new Error(response.message);
  }
}

export const organizationService = new OrganizationService();
```

---

## ?? Mejores Prácticas

1. **Siempre verifica el status** antes de usar `data`
2. **No asumas que data existe** en respuestas con error
3. **Muestra mensajes específicos** de validación por campo
4. **Maneja el 401** limpiando sesión y redirigiendo
5. **Usa loading states** para mejor UX
6. **Implementa retry logic** solo para errores de red, no para 4xx
7. **Loggea errores 500** para debugging
8. **Formatea fechas** desde ISO 8601 a formato local
9. **Cachea datos** cuando sea apropiado
10. **Implementa offline mode** con cola de sincronización

---

Esta documentación proporciona todo lo necesario para integrar el módulo de organizaciones desde el frontend. ??
