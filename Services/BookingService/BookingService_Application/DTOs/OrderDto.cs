using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public List<OrderDetailDto> OrderDetails { get; set; } = new();
    public List<TicketDto> Tickets { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class CreateOrderRequest
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerAvatarUrl { get; set; }
    public List<CreateOrderDetailRequest> OrderDetails { get; set; } = new();
}

public class UpdateOrderRequest
{
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public long? OrderCode { get; set; }
}