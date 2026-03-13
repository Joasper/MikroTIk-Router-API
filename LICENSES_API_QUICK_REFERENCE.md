# Documentación API - Módulo de Licencias

## ?? Información General

El módulo de licencias gestiona las licencias del sistema. Las licencias se crean **sin asignar** y luego se asocian a organizaciones mediante su `key`.

---

## ?? Base URL

```
http://localhost:5000/api/licenses
```

---

## ?? Flujo de Trabajo

1. **Admin crea licencia** ? Licencia con `organizationId = null`
2. **Cliente crea organización** ? Proporciona `licenseKey` (opcional)
3. **Sistema asocia licencia** ? `organizationId` se asigna automáticamente
4. **Licencia ya no disponible** ? Para otras organizaciones

---

## ?? Endpoints

### 1. Obtener Todas las Licencias

**GET** `/api/licenses`

```json
{
  "status": "success",
  "message": "Se encontraron 10 licencias",
  "data": [
    {
      "id": 1,
      "key": "ABCD1234-EFGH5678-IJKL9012-MNOP3456",
      "type": 2,
      "startDate": "2024-03-15T00:00:00Z",
      "endDate": "2025-03-15T00:00:00Z",
      "isActive": true,
      "maxRouters": 50,
      "maxUsers": 20,
      "organizationId": 1,
      "isExpired": false,
      "daysRemaining": 365
    }
  ]
}
```

---

### 2. Obtener Licencias Disponibles ?

**GET** `/api/licenses/available`

Retorna solo licencias **sin asignar**, activas y no expiradas.

```json
{
  "status": "success",
  "message": "Se encontraron 3 licencias disponibles",
  "data": [
    {
      "id": 5,
      "key": "PRO12345-6789-0123-4567",
      "type": 2,
      "organizationId": null,
      "isExpired": false
    }
  ]
}
```

---

### 3. Licencias Paginadas

**GET** `/api/licenses/paged`

#### Query Parameters
- `pageNumber` (int): Página actual
- `pageSize` (int): Items por página (max: 100)
- `sortBy` (string): `key`, `type`, `startDate`, `endDate`, `isActive`, `createdAt`
- `sortDescending` (bool): Orden descendente
- `searchTerm` (string): Buscar en key
- `filterByType` (int): 1=Basic, 2=Pro, 3=Enterprise, 4=Trial
- `filterByStatus` (bool): true=Activas, false=Inactivas
- `filterExpired` (bool): true=Expiradas, false=Vigentes

#### Ejemplos
```http
GET /api/licenses/paged?filterByType=2&filterByStatus=true
GET /api/licenses/paged?searchTerm=ABCD&sortBy=endDate
GET /api/licenses/paged?filterExpired=false&pageSize=20
```

---

### 4. Obtener por ID

**GET** `/api/licenses/{id}`

---

### 5. Obtener por Key ?

**GET** `/api/licenses/by-key/{key}`

```http
GET /api/licenses/by-key/ABCD1234-EFGH5678-IJKL9012-MNOP3456
```

**Uso**: Validar antes de crear organización.

---

### 6. Obtener por Organización

**GET** `/api/licenses/organization/{organizationId}`

---

### 7. Licencias Expiradas

**GET** `/api/licenses/expired`

---

### 8. Licencias Próximas a Expirar

**GET** `/api/licenses/expiring/{days}`

```http
GET /api/licenses/expiring/7   # 7 días
GET /api/licenses/expiring/30  # 30 días
```

---

### 9. Crear Licencia ?

**POST** `/api/licenses`

```json
{
  "type": 2,
  "startDate": "2024-03-15T00:00:00Z",
  "endDate": "2025-03-15T00:00:00Z",
  "maxRouters": 50,
  "maxUsers": 20
}
```

**Response**:
```json
{
  "status": "success",
  "message": "Licencia creada exitosamente. Usa la Key para asociarla a una organización",
  "data": {
    "id": 15,
    "key": "NEW01234-5678-9012-3456",
    "organizationId": null
  }
}
```

---

### 10. Actualizar Licencia

**PUT** `/api/licenses/{id}`

---

### 11. Eliminar Licencia

**DELETE** `/api/licenses/{id}`

---

### 12. Validar Licencia

**POST** `/api/licenses/validate/{organizationId}`

---

## ?? Tipos de Licencia

| Valor | Nombre | Límites Típicos |
|-------|--------|-----------------|
| 1 | Basic | 10 routers, 5 usuarios |
| 2 | Pro | 50 routers, 20 usuarios |
| 3 | Enterprise | 200 routers, 100 usuarios |
| 4 | Trial | 5 routers, 3 usuarios, 30 días |

---

## ?? Angular Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class LicenseService {
  private apiUrl = 'http://localhost:5000/api/licenses';

  constructor(private http: HttpClient) {}

  getAvailable() {
    return this.http.get(`${this.apiUrl}/available`);
  }

  getByKey(key: string) {
    return this.http.get(`${this.apiUrl}/by-key/${key}`);
  }

  getPaged(filters: any) {
    let params = new HttpParams();
    Object.keys(filters).forEach(key => {
      if (filters[key] != null) {
        params = params.set(key, filters[key]);
      }
    });
    return this.http.get(`${this.apiUrl}/paged`, { params });
  }

  create(license: any) {
    return this.http.post(this.apiUrl, license);
  }
}
```

---

## ?? Flujo Completo

```typescript
// 1. Admin crea licencia
const newLicense = {
  type: 2, // Pro
  startDate: new Date().toISOString(),
  endDate: new Date(Date.now() + 365*24*60*60*1000).toISOString(),
  maxRouters: 50,
  maxUsers: 20
};

licenseService.create(newLicense).subscribe(response => {
  const key = response.data.key;
  console.log('Licencia creada:', key);
});

// 2. Cliente usa el key al crear su organización
const orgData = {
  organizationName: "Mi Empresa",
  // ... otros campos
  licenseKey: "ABCD1234-EFGH5678-IJKL9012-MNOP3456"
};

organizationService.createWithAdmin(orgData).subscribe();
```

---

## ? Puntos Clave

- ? Licencias se crean **sin organización**
- ? Se asocian al **crear la organización**
- ? Una vez asignadas, **no están disponibles**
- ? Paginación y filtros avanzados
- ? Validación automática de expiración

?? **Listo para producción!**
