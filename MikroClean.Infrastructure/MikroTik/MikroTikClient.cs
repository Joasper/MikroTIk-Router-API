using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MikroClean.Domain.MikroTik;
using System.Diagnostics;
using tik4net;
using DomainITikSentence = MikroClean.Domain.MikroTik.ITikSentence;

namespace MikroClean.Infrastructure.MikroTik
{
    /// <summary>
    /// Cliente wrapper para tik4net con manejo de conexión y estado
    /// </summary>
    public class MikroTikClient : IMikroTikClient
    {
        private ITikConnection? _connection;
        private readonly object _lock = new();
        private DateTime? _lastActivity;
        private bool _disposed;

        public bool IsConnected => _connection?.IsOpened ?? false;
        public int RouterId { get; private set; }
        public DateTime? LastActivity => _lastActivity;

        public async Task<bool> ConnectAsync(RouterConnectionInfo connectionInfo, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MikroTikClient));

            lock (_lock)
            {
                if (IsConnected)
                    return true;

                try
                {
                    RouterId = connectionInfo.RouterId;
                    
                    _connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
                    _connection.ReceiveTimeout = (int)connectionInfo.CommandTimeout.TotalMilliseconds;
                    _connection.SendTimeout = (int)connectionInfo.ConnectionTimeout.TotalMilliseconds;

                    // Conectar de forma síncrona (tik4net no soporta async nativamente)
                    _connection.Open(
                        connectionInfo.Ip,
                        connectionInfo.Port,
                        connectionInfo.Username,
                        connectionInfo.Password
                    );

                    _lastActivity = DateTime.UtcNow;
                    return true;
                }
                catch
                {
                    _connection?.Dispose();
                    _connection = null;
                    throw;
                }
            }
        }

        public Task DisconnectAsync()
        {
            lock (_lock)
            {
                if (_connection != null)
                {
                    try
                    {
                        _connection.Close();
                    }
                    catch { /* Ignorar errores al cerrar */ }
                    finally
                    {
                        _connection?.Dispose();
                        _connection = null;
                    }
                }
            }
            return Task.CompletedTask;
        }

        [DebuggerStepThrough]
        public async Task<TResponse> ExecuteCommandAsync<TResponse>(string command, Dictionary<string, string> parameters)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MikroTikClient));

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!IsConnected || _connection == null)
                        throw new InvalidOperationException("No hay conexión activa con el router");

                    try
                    {
                        var cmd = _connection.CreateCommand(command);
                        
                        foreach (var param in parameters)
                        {
                            cmd.AddParameter(param.Key, param.Value);
                        }

                        var result = cmd.ExecuteScalar();
                        _lastActivity = DateTime.UtcNow;
                        
                        return (TResponse)(object)result;
                    }
                    catch (System.IO.IOException ioEx) when (ioEx.InnerException is System.Net.Sockets.SocketException)
                    {
                        HandleConnectionLost(ioEx);
                        throw;
                    }
                    catch (System.Net.Sockets.SocketException socketEx)
                    {
                        HandleConnectionLost(socketEx);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error ejecutando comando en router {RouterId}: {ex.Message}", ex);
                    }
                }
            });
        }

        [DebuggerStepThrough]
        public async Task ExecuteNonQueryAsync(string command, Dictionary<string, string> parameters)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MikroTikClient));

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!IsConnected || _connection == null)
                        throw new InvalidOperationException("No hay conexión activa con el router");

                    try
                    {
                        var cmd = _connection.CreateCommand(command);
                        
                        foreach (var param in parameters)
                        {
                            cmd.AddParameter(param.Key, param.Value);
                        }

                        cmd.ExecuteNonQuery();
                        _lastActivity = DateTime.UtcNow;
                    }
                    catch (System.IO.IOException ioEx) when (ioEx.InnerException is System.Net.Sockets.SocketException)
                    {
                        HandleConnectionLost(ioEx);
                        throw;
                    }
                    catch (System.Net.Sockets.SocketException socketEx)
                    {
                        HandleConnectionLost(socketEx);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error ejecutando comando en router {RouterId}: {ex.Message}", ex);
                    }
                }
            });
        }

        [DebuggerStepThrough]
        public async Task<IEnumerable<TResponse>> ExecuteQueryAsync<TResponse>(string command, Func<DomainITikSentence, TResponse> parser)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MikroTikClient));

            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!IsConnected || _connection == null)
                        throw new InvalidOperationException("No hay conexión activa con el router");

                    try
                    {
                        var cmd = _connection.CreateCommand(command);
                        var results = cmd.ExecuteList();
                        
                        _lastActivity = DateTime.UtcNow;

                        return results.Select(sentence => parser(new TikSentenceWrapper(sentence))).ToList();
                    }
                    catch (System.IO.IOException ioEx) when (ioEx.InnerException is System.Net.Sockets.SocketException)
                    {
                        HandleConnectionLost(ioEx);
                        throw;
                    }
                    catch (System.Net.Sockets.SocketException socketEx)
                    {
                        HandleConnectionLost(socketEx);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Error ejecutando query en router {RouterId}: {ex.Message}", ex);
                    }
                }
            });
        }

        [DebuggerStepThrough]
        private void HandleConnectionLost(Exception innerException)
        {
            CleanupConnection();
            throw new InvalidOperationException($"Conexión perdida con router {RouterId}", innerException);
        }

        private void CleanupConnection()
        {
            try
            {
                _connection?.Close();
            }
            catch { }
            finally
            {
                _connection?.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            DisconnectAsync().GetAwaiter().GetResult();
            _disposed = true;
        }

        /// <summary>
        /// Wrapper para adaptar ITikReSentence de tik4net a nuestra interfaz
        /// </summary>
        private class TikSentenceWrapper : DomainITikSentence
        {
            private readonly ITikReSentence _sentence;

            public TikSentenceWrapper(ITikReSentence sentence)
            {
                _sentence = sentence;
            }

            public string GetResponseField(string fieldName)
            {
                return _sentence.GetResponseField(fieldName);
            }

            public IEnumerable<string> GetAllWords()
            {
                return _sentence.Words.Values;
            }

            public string GetOptionalField(string fieldName)
            {
                try
                {
                    return _sentence.GetResponseField(fieldName) ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }
    }

    /// <summary>
    /// Factory para crear instancias de MikroTikClient
    /// </summary>
    public class MikroTikClientFactory : IMikroTikClientFactory
    {
        public IMikroTikClient CreateClient(int routerId)
        {
            return new MikroTikClient();
        }
    }
}
