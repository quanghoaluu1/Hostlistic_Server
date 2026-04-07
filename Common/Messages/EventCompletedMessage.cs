namespace Common.Messages;

public record EventCompletedMessage
{
    public Guid EventId { get; init; }
    public Guid OrganizerId { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
}