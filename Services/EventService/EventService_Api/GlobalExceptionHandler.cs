using Common;

namespace EventService_Api;

using Microsoft.AspNetCore.Diagnostics;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 1. Chuẩn bị dữ liệu trả về theo ApiResponse<T>
        var response = ApiResponse<object>.Fail(
            StatusCodes.Status500InternalServerError,
            "Hệ thống đang bận, vui lòng thử lại sau." // Message thân thiện cho User
        );

        // Nếu là môi trường Development, có thể thêm chi tiết lỗi vào danh sách Errors
        response.Errors = new List<string> { exception.Message };

        // 2. Thiết lập Response
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        // 3. Ghi dữ liệu JSON trực tiếp vào stream
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        // Trả về true để báo hiệu lỗi đã được xử lý xong
        return true;
    }
}