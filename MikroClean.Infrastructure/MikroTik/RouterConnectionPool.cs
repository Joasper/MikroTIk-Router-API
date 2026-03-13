using MikroClean.Domain.MikroTik;
using System.Collections.Concurrent;

namespace MikroClean.Infrastructure.MikroTik
{
    /// <summary>
    /// Pool de conexiones para routers de una organización específica
    /// Maneja múltiples conexiones concurrentes y su ciclo de vida
    /// </summary>
    public class RouterConnectionPool : IDisposable
    {
        private readonly ConcurrentDictionary<int, IMikroTikClient> _connections;
        private readonly SemaphoreSlim _semaphore;
        private readonly int _maxConnectionsPerOrganization;
        private bool _disposed;

        public int OrganizationId { get; }

        public RouterConnectionPool(int organizationId, int maxConnections = 10)
        {
            OrganizationId = organizationId;
            _maxConnectionsPerOrganization = maxConnections;
            _connections = new ConcurrentDictionary<int, IMikroTikClient>();
            _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
        }

        public async Task<IMikroTikClient?> GetOrCreateConnectionAsync(
            int routerId,
            Func<Task<IMikroTikClient>> connectionFactory,
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RouterConnectionPool));

            // Si ya existe, verificar su estado antes de devolverlo
            if (_connections.TryGetValue(routerId, out var existingClient))
            {
                if (existingClient.IsConnected)
                    return existingClient;
                
                // Si no está conectado, removerlo y crear uno nuevo
                _connections.TryRemove(routerId, out _);
                try
                {
                    existingClient.Dispose();
                }
                catch { }
                
                _semaphore.Release();
            }

            // Esperar slot disponible
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                // Double-check después de obtener el semaphore
                if (_connections.TryGetValue(routerId, out var client))
                {
                    if (client.IsConnected)
                    {
                        _semaphore.Release();
                        return client;
                    }
                    
                    // Cliente existe pero no está conectado
                    _connections.TryRemove(routerId, out _);
                    try
                    {
                        client.Dispose();
                    }
                    catch { }
                }

                var newClient = await connectionFactory();
                _connections[routerId] = newClient;
                return newClient;
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        public void ReleaseConnection(int routerId)
        {
            if (_connections.TryRemove(routerId, out var client))
            {
                client.Dispose();
                _semaphore.Release();
            }
        }

        public void DisconnectAll()
        {
            foreach (var connection in _connections.Values)
            {
                try
                {
                    connection.Dispose();
                }
                catch { /* Ignorar errores al cerrar */ }
            }
            _connections.Clear();
            
            // Resetear el semaphore
            while (_semaphore.CurrentCount < _maxConnectionsPerOrganization)
            {
                try { _semaphore.Release(); }
                catch { break; }
            }
        }

        public int ActiveConnectionCount => _connections.Count;

        public IEnumerable<int> GetActiveRouterIds() => _connections.Keys;

        public void Dispose()
        {
            if (_disposed) return;
            
            DisconnectAll();
            _semaphore?.Dispose();
            _disposed = true;
        }
    }
}
