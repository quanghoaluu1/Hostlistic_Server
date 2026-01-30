namespace BookingService_Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IconUrl { get; set; } = string.Empty;
    public decimal FeePercentage { get; set; }
    public decimal FixedFee { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}