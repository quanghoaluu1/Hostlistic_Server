namespace Common.Messages;

public record UserSessionOverriddenEvent
{
    public Guid UserId { get; init; }
    public string NewSessionId { get; init; } = string.Empty;
}
