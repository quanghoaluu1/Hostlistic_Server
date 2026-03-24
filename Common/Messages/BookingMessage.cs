namespace Common.Messages;

public record BookingConfirmedEvent(
    Guid EventId,
    Guid UserId,
    string Email,
    string FullName,
    Guid OrderId,
    List<BookingTicketInfo> Tickets,
    DateTime ConfirmedAt
);
 
public record BookingTicketInfo(
    Guid TicketTypeId,
    string TicketTypeName,
    int Quantity
);