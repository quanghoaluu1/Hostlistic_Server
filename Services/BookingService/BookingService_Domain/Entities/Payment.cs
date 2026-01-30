using System.ComponentModel.DataAnnotations.Schema;
using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string Gateway { get; set; } = string.Empty;
    
    // Navigation properties to parent
    [ForeignKey("PaymentMethodId")]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}