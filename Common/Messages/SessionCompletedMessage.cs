namespace Common.Messages;

public record SessionCompletedMessage
{
    public Guid SessionId { get; init; }
    public Guid EventId { get; init; }
    public string SessionTitle { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
}
