# Documentacin del Mdulo de IP Pools (IP/Pool)

Este documento detalla los endpoints, parmetros y respuestas del mdulo de gestin de IP Pools para los routers MikroTik.

## Informacin General
- **Base URL:** `/api/MikroTik/routers/{routerId}/ip/pools`
- **Autenticacin:** Se requiere JWT Token en el header `Authorization: Bearer {token}`.
- **Formato:** JSON (UTF-8).

---

## 1. Listar IP Pools (Paginado, Filtrado y Ordenado)
Obtiene la lista de pools de IP configurados en el router, aplicando paginacin, bsqueda y ordenamiento.

- **URL:** `GET /api/MikroTik/routers/{routerId}/ip/pools`
- **Query Parameters:**
  - `pageNumber` (int, default: 1): Nmero de pgina.
  - `pageSize` (int, default: 10): Cantidad de elementos por pgina.
  - `searchTerm` (string, opcional): Trmino de bsqueda. Filtra por `name`, `ranges`, `nextPool` o `comment`.
  - `sortBy` (string, opcional): Nombre de la columna por la cual ordenar (ej: `name`, `ranges`, `nextPool`).
  - `sortDescending` (bool, default: false): `true` para orden descendente, `false` para ascendente.

### Ejemplo de bsqueda y ordenamiento:
`GET /api/MikroTik/routers/1/ip/pools?searchTerm=dhcp&sortBy=name&sortDescending=false`

### Respuesta Exitosa (200 OK)
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "*1",
        "name": "dhcp_pool0",
        "ranges": "192.168.88.2-192.168.88.254",
        "nextPool": "none",
        "comment": "Pool principal"
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": "Se encontraron 1 pools de IP (Pgina 1)",
  "statusCode": 200
}
```

---

## 2. Crear IP Pool
Crea un nuevo pool de direcciones IP en el router.

- **URL:** `POST /api/MikroTik/routers/{routerId}/ip/pools`
- **Body:**
```json
{
  "name": "nuevo_pool",
  "ranges": "10.0.0.2-10.0.0.100",
  "nextPool": "none",
  "comment": "Comentario opcional"
}
```

### Validaciones:
- El `name` es obligatorio.
- El `ranges` es obligatorio (ej: `192.168.1.10-192.168.1.50`).
- No se permite crear dos pools con el mismo nombre (ignora maysculas/minsculas).

### Respuesta Exitosa (200 OK)
```json
{
  "isSuccess": true,
  "data": {
    "id": "*2",
    "name": "nuevo_pool",
    "ranges": "10.0.0.2-10.0.0.100",
    "nextPool": "none",
    "comment": "Comentario opcional"
  },
  "message": "IP Pool creado exitosamente",
  "statusCode": 200
}
```

---

## 3. Actualizar IP Pool
Actualiza los datos de un pool existente.

- **URL:** `PUT /api/MikroTik/routers/{routerId}/ip/pools`
- **Body:**
```json
{
  "id": "*2",
  "name": "pool_modificado",
  "ranges": "10.0.0.2-10.0.0.200",
  "nextPool": "none",
  "comment": "Comentario actualizado"
}
```

### Validaciones:
- El `id` es obligatorio (es el ID interno de MikroTik, ej: `*1`).
- Si se cambia el `name`, se verifica que no exista otro pool con el nuevo nombre.

### Respuesta Exitosa (200 OK)
```json
{
  "isSuccess": true,
  "data": {
    "id": "*2",
    "name": "pool_modificado",
    "ranges": "10.0.0.2-10.0.0.200",
    "nextPool": "none",
    "comment": "Comentario actualizado"
  },
  "message": "IP Pool actualizado exitosamente",
  "statusCode": 200
}
```

---

## 4. Eliminar IP Pool
Elimina un pool de IP del router.

- **URL:** `DELETE /api/MikroTik/routers/{routerId}/ip/pools`
- **Body:**
```json
{
  "id": "*2"
}
```

### Respuesta Exitosa (200 OK)
```json
{
  "isSuccess": true,
  "data": {
    "id": "*2"
  },
  "message": "IP Pool eliminado exitosamente",
  "statusCode": 200
}
```

---

## Manejo de Errores Comunes

### Error de Validacin (Nombre Duplicado)
```json
{
  "isSuccess": false,
  "message": "Ya existe un IP Pool con el nombre 'dhcp_pool0'",
  "statusCode": 400
}
```

### Error de MikroTik (Sintaxis o Rangos Invlidos)
```json
{
  "isSuccess": false,
  "message": "Error creando IP pool: invalid value for range",
  "errors": {
    "errorType": "MikroTikError"
  },
  "statusCode": 400
}
```
