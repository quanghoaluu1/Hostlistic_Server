namespace AIService_Application.DTOs.External;

/// <summary>
/// Mirrors BookingService OrderDto (with status as int for JSON-agnostic deserialization).
/// Source: GET /api/orders/event/{eventId}
/// </summary>
public class ExternalOrderDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// 0 = Pending, 1 = Confirmed, 2 = Cancelled, 3 = Refunded
    /// </summary>
    public int Status { get; set; }

    public string? Notes { get; set; }
    public List<ExternalOrderDetailDto> OrderDetails { get; set; } = [];
}

public class ExternalOrderDetailDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
