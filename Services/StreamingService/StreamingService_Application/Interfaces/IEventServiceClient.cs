namespace StreamingService_Application.Interfaces;

public class StreamAuthResponseDto
{
    public bool IsAllowed { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class TicketTypeStreamingAccessDto
{
    public Guid Id { get; set; }
    public List<Guid> AllowedTrackIds { get; set; } = [];
    public int? MaxQaQuestions { get; set; }
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public interface IEventServiceClient
{
    Task<StreamAuthResponseDto> VerifyStreamAccessAsync(Guid eventId, Guid userId, Guid? trackId = null, CancellationToken cancellationToken = default);
    Task<TicketTypeStreamingAccessDto?> GetTicketTypeStreamingAccessAsync(Guid ticketTypeId, CancellationToken cancellationToken = default);
    Task<EventChatAccessResponseDto> GetEventChatAccessAsync(Guid eventId, Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
}

public class EventChatAccessResponseDto
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool CanSendChat { get; set; }
    public DateTime? ChatBlockedUntil { get; set; }
}
