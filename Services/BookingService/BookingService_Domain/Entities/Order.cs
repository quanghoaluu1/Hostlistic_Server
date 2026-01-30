using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; } 
    
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}