namespace StreamingService_Application.Interfaces;

public class StreamAuthResponseDto
{
    public bool IsAllowed { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public interface IEventServiceClient
{
    Task<StreamAuthResponseDto> VerifyStreamAccessAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
}
