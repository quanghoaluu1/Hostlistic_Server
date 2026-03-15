namespace Common;

public record Message
(
    Guid OrderId,
    Guid EventId,
    Guid UserId,
    DateTime ConfirmedAt
    );