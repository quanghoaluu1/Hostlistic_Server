namespace BookingService_Application.DTOs;


public class PaymentConfirmedPayload
{
    public Guid OrderId { get; set; }
    public long OrderCode { get; set; }
    public string Status { get; set; } = "Confirmed";
    public decimal TotalAmount { get; set; }
    public List<TicketSummaryDto> Tickets { get; set; } = [];
}

public class TicketSummaryDto
{
    public Guid Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string TicketTypeName { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class PaymentFailedPayload
{
    public Guid OrderId { get; set; }
    public long OrderCode { get; set; }
    public string Reason { get; set; } = string.Empty;
}