namespace BookingService_Application.DTOs;

public class GetPaymentOptionsRequest
{
    public Guid EventId { get; set; }
    public List<TicketItemRequest> TicketItems { get; set; } = new();
}

public class PaymentOptionsResponse
{
    public decimal TotalAmount { get; set; }
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
}
