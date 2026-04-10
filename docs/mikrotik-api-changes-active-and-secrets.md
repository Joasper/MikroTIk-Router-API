# Cambios API MikroTik (PPPoE Active + PPPoE Secret)

## Resumen

Se implementaron estos cambios:

1. PPPoE Active Connections:
- Listado con paginacion, busqueda y ordenamiento.
- Eliminacion de una conexion activa (remove/disconnect).

2. PPPoE Secrets:
- Se agrega soporte del campo `disabled` en requests y operaciones MikroTik.
- El campo `disabled` ya se expone en responses y entidad local.

---

## Endpoints nuevos

### 1) Obtener conexiones PPPoE activas (paginado)

- Metodo: `GET`
- Ruta: `/api/MikroTik/routers/{routerId}/ppp/active`
- Controlador: `MikroTikController.GetActivePppConnectionsAsync`
- Servicio: `IMikroTikService.GetActivePPPoEConnectionsAsync` / `MikroTikService.GetActivePPPoEConnectionsAsync`
- Operacion MikroTik: `/ppp/active/print`

### Query params soportados

Usa `PaginationParams`:

- `pageNumber` (int)
- `pageSize` (int)
- `searchTerm` (string, opcional)
- `sortBy` (string, opcional)
- `sortDescending` (bool, opcional)

### Search aplicado sobre

- `name`
- `service`
- `callerId`
- `address`
- `uptime`

### Sort

- Dinamico por nombre de propiedad de `PPPoEActiveConnectionResponse` (case-insensitive).
- Si no se envia `sortBy`, ordena por `Name` ascendente.

### Respuesta

`ApiResponse<PagedResult<PPPoEActiveConnectionResponse>>`

Campos por item:

- `id`
- `name`
- `service`
- `callerId`
- `address`
- `uptime`

---

### 2) Eliminar conexion PPPoE activa

- Metodo: `DELETE`
- Ruta: `/api/MikroTik/routers/{routerId}/ppp/active`
- Controlador: `MikroTikController.DeleteActivePppConnectionAsync`
- Servicio: `IMikroTikService.DeleteActivePPPoEConnectionAsync` / `MikroTikService.DeleteActivePPPoEConnectionAsync`
- Operacion MikroTik: `/ppp/active/remove`

### Body esperado

```json
{
  "id": "*A"
}
```

`id` corresponde al `.id` de la sesion activa en MikroTik.

### Respuesta

`ApiResponse<PPPoEActiveConnectionResponse>` con al menos:

- `id`

---

## Cambios en PPPoE Secrets

### Modelos de request actualizados

Archivo: `MikroClean.Domain/MikroTik/Operations/OperationModels.cs`

1. `CreatePPPoESecretRequest`:
- Nuevo campo: `disabled` (bool, default `false`)

2. `UpdatePPPoESecretRequest`:
- Nuevo campo: `disabled` (bool?, opcional)

### Operaciones MikroTik actualizadas

Archivo: `MikroClean.Application/MikroTik/Operations/StandardOperations.cs`

1. `CreatePPPoESecretOperation`:
- Ahora envia parametro `disabled` como `yes/no`.

2. `UpdatePppSecretOperation`:
- Si viene `disabled`, envia parametro `disabled` como `yes/no`.

3. `GetAllPppSecretsQuery` y `GetPppSecretByIdOperation`:
- Ahora parsean `disabled` desde la respuesta del router.

### Servicio actualizado

Archivo: `MikroClean.Application/Services/MikroTikService.cs`

1. Persistencia local offline:
- `ApplyLocalPppSecretCreateAsync` ahora guarda `Disabled` en entidad local.
- `ApplyLocalPppSecretUpdateAsync` ahora permite actualizar `Disabled` cuando viene en request.

### Entidad local

Archivo: `MikroClean.Domain/Entities/PppSecret.cs`

- La entidad ya tenia `Disabled` (sin cambios estructurales requeridos).

---

## Compatibilidad

- No se elimina ningun endpoint existente.
- Los cambios de `disabled` en secrets son backward-compatible:
  - Create: si no se envia, queda `false`.
  - Update: si no se envia, no modifica el valor actual.

---

## Recursos del Router (nuevo)

### Endpoint

- Metodo: `GET`
- Ruta: `/api/MikroTik/routers/{routerId}/system/resources`
- Controlador: `MikroTikController.GetSystemResources`
- Servicio: `IMikroTikService.GetResourcesRouterAsync` / `MikroTikService.GetResourcesRouterAsync`
- Operacion MikroTik: `/system/resource/print`

### Parametros

- `routerId` (route, int)

### Respuesta

`ApiResponse<ResourcesRouterResponse>`

Campos esperados:

- `version` (string)
- `boardName` (string)
- `architecture` (string)
- `totalMemory` (long)
- `freeMemory` (long)
- `cpuLoad` (double)
- `totalHddSpace` (long)
- `freeHddSpace` (long)
- `uptime` (string)

### Ejemplo de respuesta

```json
{
  "success": true,
  "message": "Recursos del router obtenidos exitosamente",
  "data": {
    "version": "7.16.1",
    "boardName": "CHR",
    "architecture": "x86_64",
    "totalMemory": 2147483648,
    "freeMemory": 1436729344,
    "cpuLoad": 8,
    "totalHddSpace": 4294967296,
    "freeHddSpace": 3913285632,
    "uptime": "1d02:14:31"
  }
}
```
