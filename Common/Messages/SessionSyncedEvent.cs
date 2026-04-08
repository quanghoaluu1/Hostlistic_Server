namespace Common.Messages;

public record SessionSyncedEvent(
    Guid SessionId,
    Guid EventId,
    Guid TrackId,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string? Location,
    int SessionOrder);
