namespace BookingService_Application.DTOs;

public class PaymentMethodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public decimal FeePercentage { get; set; }
    public decimal FixedFee { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePaymentMethodRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public decimal FeePercentage { get; set; }
    public decimal FixedFee { get; set; }
}

public class UpdatePaymentMethodRequest
{
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public decimal FeePercentage { get; set; }
    public decimal FixedFee { get; set; }
    public bool IsActive { get; set; }
}