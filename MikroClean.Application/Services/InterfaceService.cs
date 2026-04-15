using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MikroClean.Application.Dtos.Interfaces;
using MikroClean.Application.Interfaces;
using MikroClean.Application.Models;
using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;
using MikroClean.Domain.MikroTik.Operations.Interfaces;
using System.Text.Json;

namespace MikroClean.Application.Services;

/// <summary>
/// Servicio de aplicación para gestionar interfaces de routers MikroTik
/// Implementa patrón de graceful degradation: usa router cuando está online, 
/// base de datos local cuando está offline
/// </summary>
public class InterfaceService : IInterfaceService
{
    private readonly TimeSpan _routerQueryTimeout;
    private readonly IMikroTikConnectionManager _connectionManager;
    private readonly IRouterInterfaceRepository _interfaceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterfaceService> _logger;

    public InterfaceService(
        IMikroTikConnectionManager connectionManager,
        IRouterInterfaceRepository interfaceRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<InterfaceService> logger)
    {
        _connectionManager = connectionManager;
        _interfaceRepository = interfaceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;

        var queryTimeoutRaw = configuration["MikroTik:QueryTimeoutSeconds"];
        var queryTimeoutSeconds = int.TryParse(queryTimeoutRaw, out var parsedTimeout) ? parsedTimeout : 8;
        _routerQueryTimeout = TimeSpan.FromSeconds(Math.Max(2, queryTimeoutSeconds));
    }

    /// <summary>
    /// Obtiene todas las interfaces de un router con paginación y filtrado
    /// Patrón: Intenta sincronizar desde router, siempre lee desde BD local
    /// </summary>
    public async Task<ApiResponse<PagedResult<RouterInterfaceDTO>>> GetInterfacesPagedAsync(
        int routerId, 
        PaginationParams paginationParams,
        string? typeFilter = null,
        bool? runningFilter = null,
        bool? disabledFilter = null)
    {
        try
        {
            // 1. Intentar sincronizar desde router (graceful degradation)
            var (syncedFromRouter, offlineRouter) = await TrySyncInterfacesFromRouterAsync(routerId);

            // 2. Siempre leer desde BD local como fuente de verdad
            var dbInterfaces = (await _interfaceRepository.GetAllAsync())
                .Where(x => x.RouterId == routerId)
                .ToList();

            var allInterfaces = dbInterfaces.Select(MapInterfaceToDTO).ToList();

            // 3. Filtrado por tipo
            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                allInterfaces = allInterfaces.Where(i => 
                    i.Type.Equals(typeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // 4. Filtrado por running
            if (runningFilter.HasValue)
            {
                allInterfaces = allInterfaces.Where(i => i.Running == runningFilter.Value).ToList();
            }

            // 5. Filtrado por disabled
            if (disabledFilter.HasValue)
            {
                allInterfaces = allInterfaces.Where(i => i.Disabled == disabledFilter.Value).ToList();
            }

            // 6. Filtrado por SearchTerm
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var term = paginationParams.SearchTerm.ToLower();
                allInterfaces = allInterfaces.Where(i =>
                    i.Name.ToLower().Contains(term) ||
                    i.Type.ToLower().Contains(term) ||
                    i.MacAddress.ToLower().Contains(term) ||
                    (!string.IsNullOrEmpty(i.Comment) && i.Comment.ToLower().Contains(term))
                ).ToList();
            }

            // 7. Ordenamiento
            if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
            {
                var propertyInfo = typeof(RouterInterfaceDTO).GetProperty(paginationParams.SortBy,
                    System.Reflection.BindingFlags.IgnoreCase | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    allInterfaces = paginationParams.SortDescending
                        ? allInterfaces.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList()
                        : allInterfaces.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
            }
            else
            {
                allInterfaces = allInterfaces.OrderBy(i => i.Name).ToList();
            }

            // 8. Paginación manual
            var totalCount = allInterfaces.Count;
            var pagedItems = allInterfaces
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            var pagedResult = new PagedResult<RouterInterfaceDTO>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize
            };

            var message = offlineRouter
                ? $"Router offline: mostrando {totalCount} interfaces desde base de datos local (Página {paginationParams.PageNumber})"
                : syncedFromRouter
                    ? $"Se encontraron {totalCount} interfaces (sincronizadas, Página {paginationParams.PageNumber})"
                    : $"Se encontraron {totalCount} interfaces desde base de datos local (Página {paginationParams.PageNumber})";

            return ApiResponse<PagedResult<RouterInterfaceDTO>>.Success(pagedResult, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo interfaces paginadas del router {RouterId}", routerId);
            return ApiResponse<PagedResult<RouterInterfaceDTO>>.Error($"Error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene los detalles de una interfaz específica
    /// </summary>
    public async Task<ApiResponse<RouterInterfaceDTO>> GetInterfaceByIdAsync(int routerId, string mikroTikId)
    {
        try
        {
            // Intentar sincronizar primero
            await TrySyncInterfacesFromRouterAsync(routerId);

            // Buscar en BD local
            var interfaceEntity = await _interfaceRepository.GetByMikroTikIdAsync(routerId, mikroTikId);

            if (interfaceEntity == null)
            {
                return ApiResponse<RouterInterfaceDTO>.NotFound(
                    $"No se encontró la interfaz con ID '{mikroTikId}' en el router {routerId}");
            }

            return ApiResponse<RouterInterfaceDTO>.Success(MapInterfaceToDTO(interfaceEntity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo interfaz {MikroTikId} del router {RouterId}", mikroTikId, routerId);
            return ApiResponse<RouterInterfaceDTO>.Error($"Error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza una interfaz existente en el router
    /// Patrón: Intenta actualizar en router, si está offline aplica localmente y encola como pendiente
    /// </summary>
    public async Task<ApiResponse<RouterInterfaceResponse>> UpdateInterfaceAsync(
        int routerId, 
        UpdateRouterInterfaceDTO request)
    {
        try
        {
            // 1. Validar que la interfaz existe en BD local
            var existingInterface = await _interfaceRepository.GetByMikroTikIdAsync(routerId, request.MikroTikId);
            
            if (existingInterface == null)
            {
                return ApiResponse<RouterInterfaceResponse>.NotFound(
                    $"No se encontró la interfaz con ID '{request.MikroTikId}'");
            }

            // 2. Validar que no haya duplicados de nombre si se está cambiando
            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != existingInterface.Name)
            {
                var duplicateCheck = await _interfaceRepository.GetByNameAsync(routerId, request.Name);
                if (duplicateCheck != null)
                {
                    return ApiResponse<RouterInterfaceResponse>.Error(
                        $"Ya existe una interfaz con el nombre '{request.Name}'");
                }
            }

            // 3. Ejecutar actualización en router con timeout
            var operation = new UpdateInterfaceOperation();
            var mikroTikRequest = new UpdateInterfaceRequest
            {
                MikroTikId = request.MikroTikId,
                Name = request.Name,
                Mtu = request.Mtu,
                Disabled = request.Disabled,
                Comment = request.Comment
            };

            var result = await ExecuteMutationWithTimeoutAsync(routerId, operation, mikroTikRequest);

            if (!result.IsSuccess)
            {
                // 4. Fallback offline: aplicar localmente y encolar como pendiente
                if (IsRouterOfflineError(result.ErrorType))
                {
                    var localUpdate = await ApplyLocalInterfaceUpdateAsync(routerId, request);
                    return ApiResponse<RouterInterfaceResponse>.Warning(
                        "Router offline: actualización guardada localmente y encolada como pendiente para sincronizar",
                        localUpdate);
                }

                return ApiResponse<RouterInterfaceResponse>.Error(
                    $"Error actualizando interfaz: {result.ErrorMessage}",
                    new { ErrorType = result.ErrorType.ToString() });
            }

            // 5. Confirmar actualización re-sincronizando
            await TrySyncInterfacesFromRouterAsync(routerId);

            // 6. Obtener datos actualizados
            var updatedInterface = await _interfaceRepository.GetByMikroTikIdAsync(routerId, request.MikroTikId);
            
            if (updatedInterface == null)
            {
                return ApiResponse<RouterInterfaceResponse>.Warning(
                    "Interfaz actualizada en MikroTik, pero no se pudo recuperar para confirmar",
                    null);
            }

            var response = MapInterfaceToResponse(updatedInterface);
            return ApiResponse<RouterInterfaceResponse>.Success(response, "Interfaz actualizada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando interfaz en router {RouterId}", routerId);
            return ApiResponse<RouterInterfaceResponse>.Error($"Error inesperado: {ex.Message}");
        }
    }

    /// <summary>
    /// Sincroniza todas las interfaces del router a la base de datos local
    /// </summary>
    public async Task<ApiResponse<bool>> SyncInterfacesAsync(int routerId)
    {
        try
        {
            var (synced, offline) = await TrySyncInterfacesFromRouterAsync(routerId);

            if (offline)
            {
                return ApiResponse<bool>.Warning("Router offline: no se pudo sincronizar", false);
            }

            return synced
                ? ApiResponse<bool>.Success(true, "Interfaces sincronizadas exitosamente")
                : ApiResponse<bool>.Error("Error sincronizando interfaces");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando interfaces del router {RouterId}", routerId);
            return ApiResponse<bool>.Error($"Error inesperado: {ex.Message}");
        }
    }

    #region Métodos Privados - Patrón Router Online/Offline

    /// <summary>
    /// Intenta sincron interfaces desde router. Retorna (SyncedFromRouter, OfflineRouter)
    /// </summary>
    private async Task<(bool SyncedFromRouter, bool OfflineRouter)> TrySyncInterfacesFromRouterAsync(int routerId)
    {
        var result = await ExecuteQueryWithTimeoutAsync(routerId, new GetAllInterfacesFullQuery());
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("No se pudo sincronizar interfaces desde router {RouterId}: {Error}", 
                routerId, result.ErrorMessage);
            return (false, IsRouterOfflineError(result.ErrorType));
        }

        var data = result.Data ?? new List<InterfaceFullResponse>();
        var syncSuccess = await SyncInterfacesToDatabaseAsync(routerId, data);
        
        return (syncSuccess, false);
    }

    /// <summary>
    /// Sincroniza interfaces desde router a BD local (upsert + delete removidas)
    /// </summary>
    private async Task<bool> SyncInterfacesToDatabaseAsync(int routerId, IEnumerable<InterfaceFullResponse> routerInterfaces)
    {
        try
        {
            var dbInterfaces = (await _interfaceRepository.GetAllAsync())
                .Where(x => x.RouterId == routerId)
                .ToList();

            var routerById = routerInterfaces
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToDictionary(x => x.Id.Trim(), x => x, StringComparer.OrdinalIgnoreCase);

            var routerByName = routerInterfaces
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => NormalizeKey(x.Name), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Actualizar o eliminar interfaces existentes en BD
            foreach (var dbInterface in dbInterfaces)
            {
                var key = (dbInterface.MikroTikId ?? string.Empty).Trim();
                InterfaceFullResponse? routerInterface = null;

                if (!string.IsNullOrWhiteSpace(key) && routerById.TryGetValue(key, out var byId))
                {
                    routerInterface = byId;
                }
                else
                {
                    var nameKey = NormalizeKey(dbInterface.Name);
                    if (!string.IsNullOrWhiteSpace(nameKey) && routerByName.TryGetValue(nameKey, out var byName))
                    {
                        routerInterface = byName;
                    }
                }

                if (routerInterface != null)
                {
                    // Actualizar datos desde router
                    dbInterface.MikroTikId = routerInterface.Id;
                    dbInterface.Name = routerInterface.Name;
                    dbInterface.DefaultName = routerInterface.DefaultName;
                    dbInterface.Type = routerInterface.Type;
                    dbInterface.Mtu = routerInterface.Mtu;
                    dbInterface.ActualMtu = routerInterface.ActualMtu;
                    dbInterface.MaxL2Mtu = routerInterface.MaxL2Mtu;
                    dbInterface.MacAddress = routerInterface.MacAddress;
                    dbInterface.LinkDowns = routerInterface.LinkDowns;
                    dbInterface.RxByte = routerInterface.RxByte;
                    dbInterface.TxByte = routerInterface.TxByte;
                    dbInterface.RxPacket = routerInterface.RxPacket;
                    dbInterface.TxPacket = routerInterface.TxPacket;
                    dbInterface.RxDrop = routerInterface.RxDrop;
                    dbInterface.TxDrop = routerInterface.TxDrop;
                    dbInterface.TxQueueDrop = routerInterface.TxQueueDrop;
                    dbInterface.RxError = routerInterface.RxError;
                    dbInterface.TxError = routerInterface.TxError;
                    dbInterface.FpRxByte = routerInterface.FpRxByte;
                    dbInterface.FpTxByte = routerInterface.FpTxByte;
                    dbInterface.FpRxPacket = routerInterface.FpRxPacket;
                    dbInterface.FpTxPacket = routerInterface.FpTxPacket;
                    dbInterface.Running = routerInterface.Running;
                    dbInterface.Disabled = routerInterface.Disabled;
                    dbInterface.Comment = string.IsNullOrWhiteSpace(routerInterface.Comment) 
                        ? null 
                        : routerInterface.Comment;
                    dbInterface.SyncState = SyncState.Synced;

                    _interfaceRepository.UpdateAsync(dbInterface);
                    routerById.Remove(routerInterface.Id.Trim());
                }
                else
                {
                    // Interfaz ya no existe en router, eliminar de BD
                    _interfaceRepository.DeleteAsync(dbInterface);
                }
            }

            // Agregar interfaces nuevas que están en router pero no en BD
            foreach (var routerInterface in routerById.Values)
            {
                _interfaceRepository.Add(new RouterInterface
                {
                    RouterId = routerId,
                    MikroTikId = routerInterface.Id,
                    Name = routerInterface.Name,
                    DefaultName = routerInterface.DefaultName,
                    Type = routerInterface.Type,
                    Mtu = routerInterface.Mtu,
                    ActualMtu = routerInterface.ActualMtu,
                    MaxL2Mtu = routerInterface.MaxL2Mtu,
                    MacAddress = routerInterface.MacAddress,
                    LinkDowns = routerInterface.LinkDowns,
                    RxByte = routerInterface.RxByte,
                    TxByte = routerInterface.TxByte,
                    RxPacket = routerInterface.RxPacket,
                    TxPacket = routerInterface.TxPacket,
                    RxDrop = routerInterface.RxDrop,
                    TxDrop = routerInterface.TxDrop,
                    TxQueueDrop = routerInterface.TxQueueDrop,
                    RxError = routerInterface.RxError,
                    TxError = routerInterface.TxError,
                    FpRxByte = routerInterface.FpRxByte,
                    FpTxByte = routerInterface.FpTxByte,
                    FpRxPacket = routerInterface.FpRxPacket,
                    FpTxPacket = routerInterface.FpTxPacket,
                    Running = routerInterface.Running,
                    Disabled = routerInterface.Disabled,
                    Comment = string.IsNullOrWhiteSpace(routerInterface.Comment) 
                        ? null 
                        : routerInterface.Comment,
                    SyncState = SyncState.Synced
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando interfaces en base de datos para router {RouterId}", routerId);
            _unitOfWork.ClearChangeTracker();
            return false;
        }
    }

    /// <summary>
    /// Aplica actualización localmente cuando el router está offline
    /// </summary>
    private async Task<RouterInterfaceResponse> ApplyLocalInterfaceUpdateAsync(
        int routerId, 
        UpdateRouterInterfaceDTO request)
    {
        var entity = await _interfaceRepository.GetByMikroTikIdAsync(routerId, request.MikroTikId);
        
        if (entity == null)
        {
            throw new InvalidOperationException($"Interfaz {request.MikroTikId} no encontrada");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            entity.Name = request.Name;
        
        if (request.Mtu.HasValue)
            entity.Mtu = request.Mtu.Value;
        
        if (request.Disabled.HasValue)
            entity.Disabled = request.Disabled.Value;
        
        if (!string.IsNullOrWhiteSpace(request.Comment))
            entity.Comment = request.Comment;

        entity.SyncState = SyncState.PendingUpdate;

        _interfaceRepository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapInterfaceToResponse(entity);
    }

    /// <summary>
    /// Ejecuta query con timeout hacia el router
    /// </summary>
    private async Task<MikroTikResult<TResponse>> ExecuteQueryWithTimeoutAsync<TResponse>(
        int routerId, 
        IMikroTikQuery<TResponse> query)
    {
        var queryTask = _connectionManager.ExecuteQueryAsync(routerId, query);
        var completedTask = await Task.WhenAny(queryTask, Task.Delay(_routerQueryTimeout));

        if (completedTask == queryTask)
        {
            return await queryTask;
        }

        _ = queryTask.ContinueWith(
            t =>
            {
                _logger.LogWarning(t.Exception, 
                    "Query tarda/fallida después de timeout para router {RouterId}. Comando: {Command}", 
                    routerId, query.Command);
            },
            TaskContinuationOptions.OnlyOnFaulted);

        _logger.LogWarning(
            "Timeout ejecutando query MikroTik para router {RouterId}. Comando: {Command}. Se usará fallback local.", 
            routerId, query.Command);
        
        return MikroTikResult<TResponse>.Failure(
            "Timeout consultando router MikroTik", 
            MikroTikErrorType.Timeout, 
            routerId);
    }

    /// <summary>
    /// Ejecuta mutación con timeout hacia el router
    /// </summary>
    private async Task<MikroTikResult<TResponse>> ExecuteMutationWithTimeoutAsync<TRequest, TResponse>(
        int routerId,
        IMikroTikMutation<TRequest, TResponse> operation,
        TRequest request)
    {
        var mutationTask = _connectionManager.ExecuteMutationAsync(routerId, operation, request);
        var completedTask = await Task.WhenAny(mutationTask, Task.Delay(_routerQueryTimeout));

        if (completedTask == mutationTask)
        {
            return await mutationTask;
        }

        _ = mutationTask.ContinueWith(
            t =>
            {
                _logger.LogWarning(t.Exception, 
                    "Mutación tarda/fallida después de timeout para router {RouterId}. Comando: {Command}", 
                    routerId, operation.Command);
            },
            TaskContinuationOptions.OnlyOnFaulted);

        _logger.LogWarning(
            "Timeout ejecutando mutación MikroTik para router {RouterId}. Comando: {Command}.", 
            routerId, operation.Command);
        
        return MikroTikResult<TResponse>.Failure(
            "Timeout ejecutando cambio en router MikroTik", 
            MikroTikErrorType.Timeout, 
            routerId);
    }

    /// <summary>
    /// Determina si un error indica que el router está offline
    /// </summary>
    private static bool IsRouterOfflineError(MikroTikErrorType errorType)
    {
        return errorType == MikroTikErrorType.ConnectionFailed
            || errorType == MikroTikErrorType.Timeout
            || errorType == MikroTikErrorType.RouterUnavailable;
    }

    /// <summary>
    /// Normaliza una clave para comparación case-insensitive
    /// </summary>
    private static string NormalizeKey(string key)
    {
        return (key ?? string.Empty).Trim().ToLowerInvariant();
    }

    #endregion

    #region Métodos de Mapeo

    private RouterInterfaceDTO MapInterfaceToDTO(RouterInterface entity)
    {
        return new RouterInterfaceDTO
        {
            Id = entity.Id,
            RouterId = entity.RouterId,
            MikroTikId = entity.MikroTikId,
            Name = entity.Name,
            DefaultName = entity.DefaultName,
            Type = entity.Type,
            Mtu = entity.Mtu,
            ActualMtu = entity.ActualMtu,
            MaxL2Mtu = entity.MaxL2Mtu,
            MacAddress = entity.MacAddress,
            LinkDowns = entity.LinkDowns,
            RxByte = entity.RxByte,
            TxByte = entity.TxByte,
            RxPacket = entity.RxPacket,
            TxPacket = entity.TxPacket,
            RxDrop = entity.RxDrop,
            TxDrop = entity.TxDrop,
            TxQueueDrop = entity.TxQueueDrop,
            RxError = entity.RxError,
            TxError = entity.TxError,
            FpRxByte = entity.FpRxByte,
            FpTxByte = entity.FpTxByte,
            FpRxPacket = entity.FpRxPacket,
            FpTxPacket = entity.FpTxPacket,
            Running = entity.Running,
            Disabled = entity.Disabled,
            Comment = entity.Comment,
            SyncState = entity.SyncState
        };
    }

    private RouterInterfaceResponse MapInterfaceToResponse(RouterInterface entity)
    {
        return new RouterInterfaceResponse
        {
            MikroTikId = entity.MikroTikId,
            Name = entity.Name,
            Type = entity.Type,
            Running = entity.Running,
            Disabled = entity.Disabled,
            Comment = entity.Comment,
            SyncState = entity.SyncState
        };
    }

    #endregion
}
