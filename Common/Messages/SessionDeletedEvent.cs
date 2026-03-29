namespace Common.Messages;

public record SessionDeletedEvent(
    Guid SessionId,
    Guid EventId);
