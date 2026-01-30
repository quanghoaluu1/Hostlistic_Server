namespace Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; } // Danh sách lỗi chi tiết (cho form validation)
    
    public static ApiResponse<T> Success(int statusCode, string message, T data) => new ApiResponse<T> {IsSuccess = true, StatusCode = statusCode, Message = message, Data = data};
    public static ApiResponse<T> Fail(int statusCode, string message) => new ApiResponse<T> {IsSuccess = false, StatusCode = statusCode, Message = message};
}