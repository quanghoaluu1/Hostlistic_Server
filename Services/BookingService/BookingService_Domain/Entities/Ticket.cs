using System.ComponentModel.DataAnnotations.Schema;

namespace BookingService_Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public string? HolderName { get; set; }
    public string? HolderEmail { get; set; }
    public string? HolderPhone { get; set; }
    public bool IsUsed { get; set; } = false;
    
    // Navigation property to parent
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;
    
    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();
}