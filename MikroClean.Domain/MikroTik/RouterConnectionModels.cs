namespace MikroClean.Domain.MikroTik
{
    /// <summary>
    /// Credenciales para conectarse a un router MikroTik
    /// </summary>
    public class RouterConnectionInfo
    {
        public int RouterId { get; set; }
        public string Ip { get; set; } = string.Empty;
        public int Port { get; set; } = 8728; // Puerto API por defecto
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Estado de una conexión a un router
    /// </summary>
    public class RouterConnectionStatus
    {
        public int RouterId { get; set; }
        public bool IsConnected { get; set; }
        public DateTime LastConnected { get; set; }
        public DateTime? LastDisconnected { get; set; }
        public int FailedAttempts { get; set; }
        public string? LastError { get; set; }
        public TimeSpan? Latency { get; set; }
    }

    /// <summary>
    /// Configuración de reintentos para operaciones MikroTik
    /// </summary>
    public class MikroTikRetryPolicy
    {
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(10);
        public double BackoffMultiplier { get; set; } = 2.0; // Exponential backoff
        
        /// <summary>
        /// Tipos de errores que deben reintentar
        /// </summary>
        public HashSet<MikroTikErrorType> RetriableErrors { get; set; } = new()
        {
            MikroTikErrorType.ConnectionFailed,
            MikroTikErrorType.Timeout,
            MikroTikErrorType.RouterUnavailable
        };
    }
}
