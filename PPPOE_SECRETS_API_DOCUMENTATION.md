# Documentacin del Mdulo de PPPoE Secrets (PPP/Secret)

Este documento detalla los endpoints, parmetros y respuestas del mdulo de gestin de Secretos PPPoE para los routers MikroTik.

## Informacin General
- **Base URL:** `/api/MikroTik/routers/{routerId}/ppp/secrets`
- **Autenticacin:** Se requiere JWT Token en el header `Authorization: Bearer {token}`.
- **Formato:** JSON (UTF-8).

---

## 1. Listar PPPoE Secrets (Paginado, Filtrado y Ordenado)
Obtiene la lista de secretos configurados en el router, aplicando paginacin, bsqueda y ordenamiento.

- **URL:** `GET /api/MikroTik/routers/{routerId}/ppp/secrets`
- **Query Parameters:**
  - `pageNumber` (int, default: 1): Nmero de pgina.
  - `pageSize` (int, default: 10): Cantidad de elementos por pgina.
  - `searchTerm` (string, opcional): Trmino de bsqueda. Filtra por `name`, `service`, `profile` o `comment`.
  - `sortBy` (string, opcional): Nombre de la columna por la cual ordenar (ej: `name`, `service`, `profile`).
  - `sortDescending` (bool, default: false): `true` para orden descendente, `false` para ascendente.

### Ejemplo de bsqueda y ordenamiento:
`GET /api/MikroTik/routers/1/ppp/secrets?searchTerm=cliente1&sortBy=name&sortDescending=true`

### Respuesta Exitosa (200 OK)
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "*1",
        "name": "cliente_premium",
        "service": "pppoe",
        "profile": "100Mbps",
        "disabled": false,
        "comment": "Cliente VIP"
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": "Se encontraron 1 secretos PPPoE (Pgina 1)",
  "statusCode": 200
}
```

---

## 2. Crear PPPoE Secret
Crea un nuevo secreto (usuario) en el router.

- **URL:** `POST /api/MikroTik/routers/{routerId}/ppp/secret`
- **Body:**
```json
{
  "name": "nuevo_usuario",
  "password": "password123",
  "service": "pppoe",
  "profile": "default",
  "comment": "Instalacin nueva"
}
```

### Validaciones:
- El `name` es obligatorio y debe ser nico en el router.
- El `password` es obligatorio.
- El `service` por defecto es `pppoe`.
- El `profile` debe existir en el router (se recomienda listar perfiles antes).

---

## 3. Actualizar PPPoE Secret
Actualiza los datos de un secreto existente.

- **URL:** `PUT /api/MikroTik/routers/{routerId}/ppp/secret`
- **Body:**
```json
{
  "id": "*1",
  "name": "usuario_modificado",
  "password": "nueva_password",
  "profile": "Plan_200Mbps",
  "comment": "Cambio de plan"
}
```

---

## 4. Eliminar PPPoE Secret
Elimina un secreto del router.

- **URL:** `DELETE /api/MikroTik/routers/{routerId}/ppp/secret`
- **Body:**
```json
{
  "id": "*1"
}
```

---

## Columnas Disponibles para `sortBy`
- `name`
- `service`
- `profile`
- `disabled`
- `comment`
