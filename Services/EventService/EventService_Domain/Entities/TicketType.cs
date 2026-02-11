using System.ComponentModel.DataAnnotations.Schema;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class TicketType
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid? SessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public string? Description { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int QuantitySold { get; set; }
    public DateTime SaleStartDate { get; set; }
    public DateTime SaleEndTime { get; set; } //Thời gian kết thúc trước khi đóng vé
    public int MinPerOrder { get; set; }
    public int MaxPerOrder { get; set; }
    public bool IsRequireHolderInfo { get; set; }
    public TicketTypeStatus Status { get; set; } = TicketTypeStatus.Active;
    public SaleChannel SaleChannel { get; set; }
    
    // Navigation properties to parent
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;
    
    [ForeignKey("SessionId")]
    public virtual Session? Session { get; set; }
}