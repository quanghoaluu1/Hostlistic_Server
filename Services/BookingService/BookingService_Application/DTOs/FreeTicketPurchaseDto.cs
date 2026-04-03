namespace BookingService_Application.DTOs;

public class FreeTicketPurchaseRequest
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
    public List<TicketItemRequest> TicketItems { get; set; } = new();
}

public class FreeTicketPurchaseResponse
{
    public Guid OrderId { get; set; }
    public List<TicketDto> Tickets { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}
