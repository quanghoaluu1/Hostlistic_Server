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

public record WalletPurchaseCompletedEvent(
    Guid OrderId,
    Guid EventId,
    Guid UserId,
    decimal TotalAmount,
    string EventName,
    string EventLocation,
    DateTime EventDate,
    string CustomerName,
    string CustomerEmail,
    List<WalletTicketSummary> Tickets,
    DateTime CompletedAt
);

public record WalletTicketSummary(
    Guid Id,
    Guid TicketTypeId,
    string TicketCode,
    string TicketTypeName,
    string QrCodeUrl,
    decimal Price
);

public record FreePurchaseCompletedEvent(
    Guid OrderId,
    Guid EventId,
    Guid UserId,
    string EventName,
    string EventLocation,
    DateTime EventDate,
    string CustomerName,
    string CustomerEmail,
    List<FreeTicketSummary> Tickets,
    DateTime CompletedAt
);

public record FreeTicketSummary(
    Guid Id,
    Guid TicketTypeId,
    string TicketCode,
    string TicketTypeName,
    string QrCodeUrl
);