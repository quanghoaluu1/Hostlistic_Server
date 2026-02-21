using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Gateway { get; set; }
    public PaymentMethodDto? PaymentMethod { get; set; }
}

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Gateway { get; set; }
}

public class UpdatePaymentRequest
{
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
}