namespace MikroClean.Domain.MikroTik
{
    /// <summary>
    /// Gestor de conexiones a routers MikroTik con pool de conexiones
    /// </summary>
    public interface IMikroTikConnectionManager
    {
        /// <summary>
        /// Ejecuta una operación en un router específico con retry policy
        /// </summary>
        Task<MikroTikResult<TResponse>> ExecuteOperationAsync<TRequest, TResponse>(
            int routerId,
            IMikroTikOperation<TRequest, TResponse> operation,
            TRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una mutación (add/set/remove) en un router.
        /// MikroTik retorna un string ID o null - nunca un ITikSentence.
        /// </summary>
        Task<MikroTikResult<TResponse>> ExecuteMutationAsync<TRequest, TResponse>(
            int routerId,
            IMikroTikMutation<TRequest, TResponse> operation,
            TRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ejecuta una query (solo lectura) en un router
        /// </summary>
        Task<MikroTikResult<TResponse>> ExecuteQueryAsync<TResponse>(
            int routerId,
            IMikroTikQuery<TResponse> query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el estado de conexión actual de un router
        /// </summary>
        Task<RouterConnectionStatus> GetConnectionStatusAsync(int routerId);

        /// <summary>
        /// Verifica la conectividad y actualiza el estado del router
        /// </summary>
        Task<bool> TestConnectionAsync(int routerId);

        /// <summary>
        /// Reinicia un router MikroTik
        /// </summary>
        Task<MikroTikResult<bool>> RebootRouterAsync(int routerId);

        /// <summary>
        /// Cierra todas las conexiones de una organización (ej: al desconectar usuario)
        /// </summary>
        Task DisconnectOrganizationRoutersAsync(int organizationId);

        /// <summary>
        /// Cierra conexión específica de un router
        /// </summary>
        Task DisconnectRouterAsync(int routerId);

        /// <summary>
        /// Pre-calienta las conexiones de los routers de una organización
        /// </summary>
        Task WarmUpConnectionsAsync(int organizationId);
    }

    /// <summary>
    /// Cliente de conexión individual a un router MikroTik
    /// </summary>
    public interface IMikroTikClient : IDisposable
    {
        bool IsConnected { get; }
        int RouterId { get; }
        DateTime? LastActivity { get; }
        
        Task<bool> ConnectAsync(RouterConnectionInfo connectionInfo, CancellationToken cancellationToken = default);
        Task DisconnectAsync();
        
        /// <summary>
        /// Ejecuta un comando que devuelve un valor escalar
        /// </summary>
        Task<TResponse> ExecuteCommandAsync<TResponse>(string command, Dictionary<string, string> parameters);
        
        /// <summary>
        /// Ejecuta un comando sin esperar respuesta (para acciones como reboot, shutdown, etc.)
        /// </summary>
        Task ExecuteNonQueryAsync(string command, Dictionary<string, string> parameters);
        
        /// <summary>
        /// Ejecuta una query que devuelve una lista de resultados
        /// </summary>
        Task<IEnumerable<TResponse>> ExecuteQueryAsync<TResponse>(string command, Func<ITikSentence, TResponse> parser);
    }

    /// <summary>
    /// Factory para crear clientes MikroTik
    /// </summary>
    public interface IMikroTikClientFactory
    {
        IMikroTikClient CreateClient(int routerId);
    }
}
