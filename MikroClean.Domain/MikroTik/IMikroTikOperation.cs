namespace MikroClean.Domain.MikroTik
{
    /// <summary>
    /// Define una operación tipada que se ejecutará en un router MikroTik
    /// </summary>
    /// <typeparam name="TRequest">Tipo de datos de entrada</typeparam>
    /// <typeparam name="TResponse">Tipo de datos de salida</typeparam>
    public interface IMikroTikOperation<TRequest, TResponse>
    {
        /// <summary>
        /// Comando de la API de MikroTik (ej: /interface/bridge/add)
        /// </summary>
        string Command { get; }
        
        /// <summary>
        /// Construye los parámetros de la operación desde el request
        /// </summary>
        Dictionary<string, string> BuildParameters(TRequest request);
        
        /// <summary>
        /// Convierte la respuesta del router al tipo esperado
        /// </summary>
        TResponse ParseResponse(ITikSentence response);
        
        /// <summary>
        /// Indica si la operación necesita privilegios administrativos
        /// </summary>
        bool RequiresAdminPrivileges => true;
    }

    /// <summary>
    /// Operación simple sin request (solo query)
    /// </summary>
    public interface IMikroTikQuery<TResponse>
    {
        string Command { get; }
        TResponse ParseResponse(IEnumerable<ITikSentence> responses);
    }

    /// <summary>
    /// Representa una sentencia del protocolo MikroTik API
    /// </summary>
    public interface ITikSentence
    {
        string GetResponseField(string fieldName);
        string GetOptionalField(string fieldName);
        IEnumerable<string> GetAllWords();
    }
}
