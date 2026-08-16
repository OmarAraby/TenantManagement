namespace TenantManagement.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? [] };
}

public static class ApiResponse
{
    public static ApiResponse<object> SuccessResponse(string message)
        => new() { Success = true, Message = message };

    public static ApiResponse<object> ErrorResponse(string message, List<string>? errors = null)
        => ApiResponse<object>.ErrorResponse(message, errors);
}
