namespace MikroClean.Domain.MikroTik
{
    /// <summary>
    /// Define una operacion tipada que se ejecutara en un router MikroTik.
    /// Usar solo para comandos que retornan un ITikSentence completo.
    /// Para mutaciones (add/set/remove) usar IMikroTikMutation.
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
        /// Construye los parametros de la operacion desde el request
        /// </summary>
        Dictionary<string, string> BuildParameters(TRequest request);
        
        /// <summary>
        /// Convierte la respuesta del router al tipo esperado
        /// </summary>
        TResponse ParseResponse(ITikSentence response);
        
        /// <summary>
        /// Indica si la operacion necesita privilegios administrativos
        /// </summary>
        bool RequiresAdminPrivileges => true;
    }

    /// <summary>
    /// Para comandos que MUTAN estado: /add, /set, /remove.
    /// MikroTik retorna un string con el ID (en /add) o null/vacio (en /set y /remove),
    /// NUNCA un ITikSentence completo.
    /// </summary>
    public interface IMikroTikMutation<TRequest, TResponse>
    {
        string Command { get; }
        Dictionary<string, string> BuildParameters(TRequest request);

        /// <param name="rawResponse">
        /// El string retornado por MikroTik: "*9" en /add, null o "" en /set y /remove.
        /// </param>
        TResponse ParseResponse(string? rawResponse);
    }

    /// <summary>
    /// Operacion simple sin request (solo query)
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
