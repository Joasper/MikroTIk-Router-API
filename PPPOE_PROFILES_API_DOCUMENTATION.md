# Documentacin del Mdulo de PPPoE Profiles (PPP/Profile)

Este documento detalla los endpoints, parmetros y respuestas del mdulo de gestin de Perfiles PPPoE para los routers MikroTik.

## Informacin General
- **Base URL:** `/api/MikroTik/routers/{routerId}/ppp/profiles`
- **Autenticacin:** Se requiere JWT Token en el header `Authorization: Bearer {token}`.
- **Formato:** JSON (UTF-8).

---

## 1. Listar PPPoE Profiles (Paginado, Filtrado y Ordenado)
Obtiene la lista de perfiles configurados en el router, aplicando paginacin, bsqueda y ordenamiento.

- **URL:** `GET /api/MikroTik/routers/{routerId}/ppp/profiles`
- **Query Parameters:**
  - `pageNumber` (int, default: 1): Nmero de pgina.
  - `pageSize` (int, default: 10): Cantidad de elementos por pgina.
  - `searchTerm` (string, opcional): Trmino de bsqueda. Filtra por `name`, `localAddress`, `remoteAddress` o `comment`.
  - `sortBy` (string, opcional): Nombre de la columna por la cual ordenar (ej: `name`, `rateLimit`).
  - `sortDescending` (bool, default: false): `true` para orden descendente, `false` para ascendente.

### Ejemplo:
`GET /api/MikroTik/routers/1/ppp/profiles?searchTerm=Plan&sortBy=name&sortDescending=false`

### Respuesta Exitosa (200 OK)
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "*1",
        "name": "Plan_100Mbps",
        "localAddress": "10.0.0.1",
        "remoteAddress": "dhcp_pool0",
        "dnsServers": "8.8.8.8,1.1.1.1",
        "rateLimit": "100M/100M",
        "onlyOne": "yes",
        "comment": "Perfil para clientes premium"
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": "Se encontraron 1 perfiles PPPoE (Pgina 1)",
  "statusCode": 200
}
```

---

## 2. Crear PPPoE Profile
Crea un nuevo perfil de servicio en el router.

- **URL:** `POST /api/MikroTik/routers/{routerId}/ppp/profile`
- **Body:**
```json
{
  "name": "Plan_50Mbps",
  "localAddress": "10.0.0.1",
  "remoteAddress": "pool_50mb",
  "dnsServers": "8.8.8.8",
  "rateLimit": "50M/50M",
  "onlyOne": "yes",
  "comment": "Plan bsico"
}
```

### Validaciones:
- El `name` es obligatorio y debe ser nico.
- El `remoteAddress` suele ser el nombre de un IP Pool.

---

## 3. Actualizar PPPoE Profile
Actualiza los datos de un perfil existente.

- **URL:** `PUT /api/MikroTik/routers/{routerId}/ppp/profile`
- **Body:**
```json
{
  "id": "*1",
  "name": "Plan_150Mbps",
  "rateLimit": "150M/150M",
  "comment": "Upgrade de plan"
}
```

---

## 4. Eliminar PPPoE Profile
Elimina un perfil del router.

- **URL:** `DELETE /api/MikroTik/routers/{routerId}/ppp/profile`
- **Body:**
```json
{
  "id": "*1"
}
```

---

## Columnas Disponibles para `sortBy`
- `name`
- `localAddress`
- `remoteAddress`
- `dnsServers`
- `rateLimit`
- `onlyOne`
- `comment`
