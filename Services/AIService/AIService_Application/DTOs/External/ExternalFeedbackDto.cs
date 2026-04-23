namespace AIService_Application.DTOs.External;

/// <summary>
/// Mirrors EventService FeedbackDto.
/// Source: GET /api/feedback/event/{eventId}
/// </summary>
public class ExternalFeedbackDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
