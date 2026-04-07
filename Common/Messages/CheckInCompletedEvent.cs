namespace Common.Messages;

public record CheckInCompletedEvent(
    Guid EventId,
    Guid CheckInId,
    Guid TicketId,
    Guid? SessionId,
    string AttendeeName,
    string TicketCode,
    string TicketTypeName,
    string? SessionName,
    DateTime CheckInTime,
    int CheckInType);
