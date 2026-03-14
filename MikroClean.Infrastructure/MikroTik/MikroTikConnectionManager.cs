using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Domain.Interfaces.Security;
using MikroClean.Domain.MikroTik;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Collections.Concurrent;

namespace MikroClean.Infrastructure.MikroTik
{
    /// <summary>
    /// Gestor central de conexiones MikroTik con pool por organizaci�n,
    /// retry policies y health monitoring
    /// </summary>
    public class MikroTikConnectionManager : IMikroTikConnectionManager, IDisposable
    {
        private readonly ConcurrentDictionary<int, RouterConnectionPool> _organizationPools;
        private readonly IMikroTikClientFactory _clientFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEncryptionService _encryptionService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MikroTikConnectionManager> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly MikroTikRetryPolicy _retryConfig;
        private bool _disposed;

        public MikroTikConnectionManager(
            IMikroTikClientFactory clientFactory,
            IServiceProvider serviceProvider,
            IEncryptionService encryptionService,
            IMemoryCache cache,
            ILogger<MikroTikConnectionManager> logger,
            MikroTikRetryPolicy? retryConfig = null)
        {
            _clientFactory = clientFactory;
            _serviceProvider = serviceProvider;
            _encryptionService = encryptionService;
            _cache = cache;
            _logger = logger;
            _organizationPools = new ConcurrentDictionary<int, RouterConnectionPool>();
            _retryConfig = retryConfig ?? new MikroTikRetryPolicy();

            // Configurar Polly retry policy con exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>(ex => ShouldRetry(ex))
                .WaitAndRetryAsync(
                    _retryConfig.MaxRetryAttempts,
                    attempt => TimeSpan.FromMilliseconds(
                        Math.Min(
                            _retryConfig.InitialDelay.TotalMilliseconds * Math.Pow(_retryConfig.BackoffMultiplier, attempt - 1),
                            _retryConfig.MaxDelay.TotalMilliseconds
                        )
                    ),
                    onRetry: (exception, timespan, attempt, context) =>
                    {
                        _logger.LogWarning(
                            "Reintentando operaci�n MikroTik. Intento {Attempt}/{MaxAttempts}. Error: {Error}",
                            attempt, _retryConfig.MaxRetryAttempts, exception.Message
                        );
                    }
                );
        }

        public async Task<MikroTikResult<TResponse>> ExecuteOperationAsync<TRequest, TResponse>(
            int routerId,
            IMikroTikOperation<TRequest, TResponse> operation,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var attemptCount = 0;
            
            try
            {
                var result = await _retryPolicy.ExecuteAsync(async (context) =>
                {
                    attemptCount = context.Count;
                    
                    var client = await GetOrCreateClientAsync(routerId, cancellationToken);
                    if (client == null)
                    {
                        throw new InvalidOperationException($"No se pudo establecer conexi�n con el router {routerId}");
                    }

                    var parameters = operation.BuildParameters(request);
                    var response = await client.ExecuteCommandAsync<dynamic>(operation.Command, parameters);
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                        await routerRepository.UpdateLastSeenAsync(routerId, DateTime.UtcNow);
                    }
                    
                    var parsedResponse = operation.ParseResponse(response);
                    return parsedResponse;
                }, 
                new Polly.Context { { "routerId", routerId.ToString() } });

                _logger.LogInformation(
                    "Operaci�n exitosa en router {RouterId}. Comando: {Command}. Intentos: {Attempts}",
                    routerId, operation.Command, attemptCount
                );

                return MikroTikResult<TResponse>.Success(result, routerId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Conexi�n perdida"))
            {
                _logger.LogWarning(
                    "Conexi�n perdida con router {RouterId} durante operaci�n {Command}. Limpiando pool.",
                    routerId, operation.Command
                );
                
                await DisconnectRouterAsync(routerId);
                
                return MikroTikResult<TResponse>.Failure(
                    "La conexi�n con el router se ha perdido",
                    MikroTikErrorType.ConnectionFailed,
                    routerId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error ejecutando operaci�n en router {RouterId} despu�s de {Attempts} intentos. Comando: {Command}",
                    routerId, attemptCount, operation.Command
                );

                var errorType = ClassifyError(ex);
                
                // Si es error de conexi�n, remover el cliente del pool
                if (errorType == MikroTikErrorType.ConnectionFailed || 
                    errorType == MikroTikErrorType.AuthenticationFailed)
                {
                    await DisconnectRouterAsync(routerId);
                }

                return MikroTikResult<TResponse>.Failure(ex.Message, errorType, routerId);
            }
        }

        public async Task<MikroTikResult<TResponse>> ExecuteMutationAsync<TRequest, TResponse>(
            int routerId,
            IMikroTikMutation<TRequest, TResponse> operation,
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var attemptCount = 0;
            try
            {
                var result = await _retryPolicy.ExecuteAsync(async (context) =>
                {
                    attemptCount = context.Count;
                    var client = await GetOrCreateClientAsync(routerId, cancellationToken);
                    if (client == null)
                        throw new InvalidOperationException($"No se pudo establecer conexion con el router {routerId}");


                    var parameters = operation.BuildParameters(request);

                    string? rawId = null;
                    if (operation.Command.EndsWith("/set") ||
                        operation.Command.EndsWith("/remove") ||
                        operation.Command.EndsWith("/disable") ||
                        operation.Command.EndsWith("/enable"))
                    {
                        // Para los comandos silenciosos que no retornan ID
                        await client.ExecuteNonQueryAsync(operation.Command, parameters);
                    }
                    else
                    {
                        // MikroTik /add retorna el ID ("*9")
                        var response = await client.ExecuteCommandAsync<dynamic>(operation.Command, parameters);
                        rawId = response?.ToString();
                    }


                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repo = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                        await repo.UpdateLastSeenAsync(routerId, DateTime.UtcNow);
                    }

                    return operation.ParseResponse(rawId);
                }, new Polly.Context { { "routerId", routerId.ToString() } });

                _logger.LogInformation(
                    "Mutacion exitosa en router {RouterId}. Comando: {Command}. Intentos: {Attempts}",
                    routerId, operation.Command, attemptCount);

                return MikroTikResult<TResponse>.Success(result, routerId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("conexion") || ex.Message.Contains("conexi"))
            {
                await DisconnectRouterAsync(routerId);
                return MikroTikResult<TResponse>.Failure("La conexion con el router se ha perdido", MikroTikErrorType.ConnectionFailed, routerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en mutacion router {RouterId}. Comando: {Command}", routerId, operation.Command);
                var errorType = ClassifyError(ex);
                if (errorType == MikroTikErrorType.ConnectionFailed || errorType == MikroTikErrorType.AuthenticationFailed)
                    await DisconnectRouterAsync(routerId);
                return MikroTikResult<TResponse>.Failure(ex.Message, errorType, routerId);
            }
        }

        public async Task<MikroTikResult<TResponse>> ExecuteQueryAsync<TResponse>(
            int routerId,
            IMikroTikQuery<TResponse> query,
            CancellationToken cancellationToken = default)
        {
            var attemptCount = 0;

            try
            {
                var result = await _retryPolicy.ExecuteAsync(async (context) =>
                {
                    attemptCount = context.Count;
                    
                    var client = await GetOrCreateClientAsync(routerId, cancellationToken);
                    if (client == null)
                    {
                        throw new InvalidOperationException($"No se pudo establecer conexi�n con el router {routerId}");
                    }

                    var responses = await client.ExecuteQueryAsync(query.Command, sentence => sentence);
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                        await routerRepository.UpdateLastSeenAsync(routerId, DateTime.UtcNow);
                    }
                    
                    var parsedResponse = query.ParseResponse(responses);
                    return parsedResponse;
                }, 
                new Polly.Context { { "routerId", routerId.ToString() } });

                return MikroTikResult<TResponse>.Success(result, routerId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Conexi�n perdida"))
            {
                _logger.LogWarning(
                    "Conexi�n perdida con router {RouterId} durante query. Limpiando pool.",
                    routerId
                );
                
                await DisconnectRouterAsync(routerId);
                
                return MikroTikResult<TResponse>.Failure(
                    "La conexi�n con el router se ha perdido",
                    MikroTikErrorType.ConnectionFailed,
                    routerId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error ejecutando query en router {RouterId} despu�s de {Attempts} intentos",
                    routerId, attemptCount
                );

                var errorType = ClassifyError(ex);
                
                if (errorType == MikroTikErrorType.ConnectionFailed || 
                    errorType == MikroTikErrorType.AuthenticationFailed)
                {
                    await DisconnectRouterAsync(routerId);
                }

                return MikroTikResult<TResponse>.Failure(ex.Message, errorType, routerId);
            }
        }

        public async Task<RouterConnectionStatus> GetConnectionStatusAsync(int routerId)
        {
            var cacheKey = $"router_status_{routerId}";
            
            if (_cache.TryGetValue<RouterConnectionStatus>(cacheKey, out var cachedStatus))
            {
                return cachedStatus!;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                var router = await routerRepository.GetByIdAsync(routerId);
                if (router == null)
                {
                    return new RouterConnectionStatus
                    {
                        RouterId = routerId,
                        IsConnected = false,
                        LastError = "Router no encontrado"
                    };
                }

                var pool = GetPoolForRouter(router.OrganizationId);
                var isInPool = pool?.GetActiveRouterIds().Contains(routerId) ?? false;

                var status = new RouterConnectionStatus
                {
                    RouterId = routerId,
                    IsConnected = isInPool,
                    LastConnected = router.LastSeen ?? DateTime.MinValue,
                    FailedAttempts = 0
                };

                _cache.Set(cacheKey, status, TimeSpan.FromSeconds(30));
                return status;
            }
        }

        public async Task<bool> TestConnectionAsync(int routerId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                    var router = await routerRepository.GetByIdAsync(routerId);
                    if (router == null || !router.IsActive)
                        return false;

                    // Intentar obtener o crear una conexi�n
                    var client = await GetOrCreateClientAsync(routerId, CancellationToken.None);
                    if (client == null || !client.IsConnected)
                        return false;

                    // Probar la conexi�n con un comando simple (system identity)
                    try
                    {
                        var testCommand = "/system/identity/print";
                        var result = await client.ExecuteQueryAsync(testCommand, sentence => sentence);
                        
                        // Si llegamos aqu�, la conexi�n es v�lida
                        await routerRepository.UpdateLastSeenAsync(routerId, DateTime.UtcNow);
                        return true;
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Conexi�n perdida"))
                    {
                        // La conexi�n se perdi� durante la ejecuci�n - ya fue limpiada por MikroTikClient
                        _logger.LogWarning("Conexi�n perdida durante test con router {RouterId}: {Error}", routerId, ex.Message);
                        await DisconnectRouterAsync(routerId);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        // Cualquier otro error indica que la conexi�n no es v�lida
                        _logger.LogWarning(ex, "Error en test de conexi�n con router {RouterId}", routerId);
                        await DisconnectRouterAsync(routerId);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error probando conexi�n con router {RouterId}", routerId);
                return false;
            }
        }

        public async Task<MikroTikResult<bool>> RebootRouterAsync(int routerId)
        {
            try
            {
                var client = await GetOrCreateClientAsync(routerId, CancellationToken.None);
                if (client == null)
                {
                    return MikroTikResult<bool>.Failure(
                        $"No se pudo establecer conexi�n con el router {routerId}",
                        MikroTikErrorType.ConnectionFailed,
                        routerId
                    );
                }

                // Ejecutar comando de reboot sin esperar respuesta (el router se desconecta inmediatamente)
                var parameters = new Dictionary<string, string>();
                await client.ExecuteNonQueryAsync("/system/reboot", parameters);

                // Cerrar la conexi�n ya que el router se va a reiniciar
                await DisconnectRouterAsync(routerId);

                _logger.LogInformation("Router {RouterId} reiniciado exitosamente", routerId);

                return MikroTikResult<bool>.Success(true, routerId, "Router reiniciado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reiniciando router {RouterId}", routerId);
                return MikroTikResult<bool>.Failure(
                    $"Error al reiniciar router: {ex.Message}",
                    ClassifyError(ex),
                    routerId
                );
            }
        }

        public async Task DisconnectOrganizationRoutersAsync(int organizationId)
        {
            if (_organizationPools.TryRemove(organizationId, out var pool))
            {
                _logger.LogInformation(
                    "Cerrando todas las conexiones de la organizaci�n {OrganizationId}. Routers activos: {Count}",
                    organizationId, pool.ActiveConnectionCount
                );
                
                pool.DisconnectAll();
                pool.Dispose();
            }

            await Task.CompletedTask;
        }

        public async Task DisconnectRouterAsync(int routerId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                var router = await routerRepository.GetByIdAsync(routerId);
                if (router == null)
                    return;

                var pool = GetPoolForRouter(router.OrganizationId);
                pool?.ReleaseConnection(routerId);

                _logger.LogInformation("Conexi�n cerrada para router {RouterId}", routerId);
            }
        }

        public async Task WarmUpConnectionsAsync(int organizationId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                var routers = await routerRepository.GetAvailableRoutersAsync(organizationId);
                
                _logger.LogInformation(
                    "Pre-calentando conexiones para organizaci�n {OrganizationId}. Routers: {Count}",
                    organizationId, routers.Count()
                );

                var tasks = routers.Select(router => TestConnectionAsync(router.Id));
                await Task.WhenAll(tasks);
            }
        }

        private async Task<IMikroTikClient?> GetOrCreateClientAsync(int routerId, CancellationToken cancellationToken)
        {
            var router = await GetRouterWithDecryptedPasswordAsync(routerId);
            if (router == null || !router.IsActive || router.DeletedAt != null)
            {
                _logger.LogWarning("Router {RouterId} no disponible o inactivo", routerId);
                return null;
            }

            var pool = _organizationPools.GetOrAdd(
                router.OrganizationId,
                orgId => new RouterConnectionPool(orgId, maxConnections: 20)
            );

            var client = await pool.GetOrCreateConnectionAsync(
                routerId,
                async () =>
                {
                    var newClient = _clientFactory.CreateClient(routerId);
                    
                    var connectionInfo = new RouterConnectionInfo
                    {
                        RouterId = router.Id,
                        Ip = router.Ip,
                        Port = 8728, // Puerto por defecto de la API de MikroTik
                        Username = router.User,
                        Password = router.EncryptedPassword, // TODO: Descifrar password
                        OrganizationId = router.OrganizationId
                    };

                    var connected = await newClient.ConnectAsync(connectionInfo, cancellationToken);
                    
                    if (!connected)
                    {
                        newClient.Dispose();
                        throw new InvalidOperationException($"No se pudo conectar al router {routerId}");
                    }

                    _logger.LogInformation(
                        "Nueva conexi�n establecida con router {RouterId} de organizaci�n {OrganizationId}",
                        routerId, router.OrganizationId
                    );

                    return newClient;
                },
                cancellationToken
            );

            return client;
        }

        private async Task<Router?> GetRouterWithDecryptedPasswordAsync(int routerId)
        {
            var cacheKey = $"router_info_{routerId}";
            
            if (_cache.TryGetValue<Router>(cacheKey, out var cachedRouter))
            {
                return cachedRouter;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var routerRepository = scope.ServiceProvider.GetRequiredService<IRouterRepository>();
                var router = await routerRepository.GetByIdAsync(routerId);
                if (router != null)
                {
                    try
                    {
                        router.EncryptedPassword = _encryptionService.Decrypt(router.EncryptedPassword);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error desencriptando password del router {RouterId}", routerId);
                        throw new InvalidOperationException("No se pudo desencriptar la contrase�a del router", ex);
                    }
                    
                    _cache.Set(cacheKey, router, TimeSpan.FromMinutes(5));
                }

                return router;
            }
        }

        private RouterConnectionPool? GetPoolForRouter(int organizationId)
        {
            _organizationPools.TryGetValue(organizationId, out var pool);
            return pool;
        }

        private bool ShouldRetry(Exception ex)
        {
            var errorType = ClassifyError(ex);
            return _retryConfig.RetriableErrors.Contains(errorType);
        }

        private MikroTikErrorType ClassifyError(Exception ex)
        {
            var message = ex.Message.ToLowerInvariant();

            if (message.Contains("timeout") || ex is TimeoutException)
                return MikroTikErrorType.Timeout;
            
            if (message.Contains("authentication") || message.Contains("login") || message.Contains("password"))
                return MikroTikErrorType.AuthenticationFailed;
            
            if (message.Contains("connection") || message.Contains("connect") || ex is System.Net.Sockets.SocketException)
                return MikroTikErrorType.ConnectionFailed;
            
            if (message.Contains("permission") || message.Contains("denied"))
                return MikroTikErrorType.PermissionDenied;
            
            if (message.Contains("unavailable") || message.Contains("not reachable"))
                return MikroTikErrorType.RouterUnavailable;

            return MikroTikErrorType.Unknown;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("Cerrando todas las conexiones MikroTik...");

            foreach (var pool in _organizationPools.Values)
            {
                pool.Dispose();
            }

            _organizationPools.Clear();
            _disposed = true;
        }
    }
}
