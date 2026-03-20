using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class EventSettlement
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid OrganizerId { get; set; }
    public Guid WalletId { get; set; }
    
    public decimal GrossRevenue { get; set; }        // Tổng tiền vé bán được
    public decimal PlatformFeePercent { get; set; }   // % phí hệ thống (vd: 5%)
    public decimal PlatformFeeAmount { get; set; }    // Số tiền phí
    public decimal NetRevenue { get; set; }           // Tiền organizer nhận = Gross - Fee
    
    public int TotalTicketsSold { get; set; }
    public int TotalOrders { get; set; }
    
    public SettlementStatus Status { get; set; }
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}