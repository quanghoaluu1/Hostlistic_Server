namespace Common;

public record BookingMessage
(
    Guid OrderId,
    Guid EventId,
    Guid UserId,
    DateTime ConfirmedAt
    );