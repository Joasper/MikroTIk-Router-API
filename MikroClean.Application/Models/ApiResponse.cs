namespace MikroClean.Application.Models
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } = ResponseStatus.Success;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }
        public PaginationMetadata? Pagination { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Métodos estáticos para crear respuestas fácilmente
        public static ApiResponse<T> Success(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.Success,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> SuccessWithPagination(T data, string message, PaginationMetadata pagination)
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.Success,
                Message = message,
                Data = data,
                Pagination = pagination,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Error(string message = "An error occurred", object? errors = null)
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.Error,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.NotFound,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ValidationError(string message, object errors)
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.ValidationError,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.Unauthorized,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Access forbidden")
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.Forbidden,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Warning(string message, T? data = default)
        {
            return new ApiResponse<T>
            {
                Status = ResponseStatus.Warning,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PaginationMetadata(int currentPage, int pageSize, int totalRecords)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalRecords = totalRecords;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }
    }
}
