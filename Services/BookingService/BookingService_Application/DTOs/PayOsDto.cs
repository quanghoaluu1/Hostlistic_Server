// BookingService_Application/DTOs/PayOs/CreatePayOsPaymentRequest.cs
namespace BookingService_Application.DTOs.PayOs;

public class CreatePayOsPaymentRequest
{
    public long OrderCode { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<PayOsItemDto> Items { get; set; } = [];
}

public class PayOsItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

// Response DTOs (abstraction layer — không expose PayOS SDK types ra Application layer)
public class PayOsCheckoutResult
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string PaymentLinkId { get; set; } = string.Empty;
}

public class PayOsPaymentStatusResult
{
    public long OrderCode { get; set; }
    public long Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}

public class PayOsCheckoutResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public long OrderCode { get; set; } 
    public int ExpiresInMinutes { get; set; } = 15;
}