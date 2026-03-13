namespace MikroClean.Domain.MikroTik
{
    /// <summary>
    /// Resultado de una operación MikroTik con manejo de errores
    /// </summary>
    public class MikroTikResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public MikroTikErrorType ErrorType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? RouterId { get; set; }
        public int AttemptCount { get; set; }

        public static MikroTikResult<T> Success(T data, int? routerId = null, string? message = null)
        {
            return new MikroTikResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message,
                RouterId = routerId,
                ErrorType = MikroTikErrorType.None
            };
        }

        public static MikroTikResult<T> Failure(string error, MikroTikErrorType errorType, int? routerId = null)
        {
            return new MikroTikResult<T>
            {
                IsSuccess = false,
                ErrorMessage = error,
                ErrorType = errorType,
                RouterId = routerId
            };
        }
    }

    public enum MikroTikErrorType
    {
        None,
        ConnectionFailed,
        AuthenticationFailed,
        CommandFailed,
        Timeout,
        RouterUnavailable,
        InvalidResponse,
        PermissionDenied,
        Unknown
    }
}
