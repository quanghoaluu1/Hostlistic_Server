namespace BookingService_Application.DTOs;

public class OrderDetailDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class CreateOrderDetailRequest
{
    public Guid TicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateOrderDetailRequest
{
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}