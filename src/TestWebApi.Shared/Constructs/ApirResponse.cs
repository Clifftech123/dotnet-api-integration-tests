

namespace TestWebApi.Shared.Constructs
{
    public class ApiResponse<T>(bool Success, string Message, T? Data = default, string[]? Errors = null)
    {
        public static ApiResponse<T> SuccessResponse(string message, T? data = default)
            => new(true, message, data);

        public static ApiResponse<T> ErrorResponse(string message, string[]? errors = null)
            => new(false, message, default, errors);

        public static ApiResponse<T> ErrorResponse(string message, T? data = default, string[]? errors = null)
            => new(false, message, data, errors);
    }
}
