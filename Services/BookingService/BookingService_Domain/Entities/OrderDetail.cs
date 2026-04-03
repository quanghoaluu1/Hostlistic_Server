using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService_Domain.Entities;

public class OrderDetail
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Navigation property to parent
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;
}