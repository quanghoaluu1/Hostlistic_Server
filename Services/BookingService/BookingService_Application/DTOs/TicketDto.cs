namespace BookingService_Application.DTOs;

public class TicketDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public bool IsUsed { get; set; }

    // Optional enrichments for email/UI (not persisted)
    public string TicketTypeName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class CreateTicketRequest
{
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public Guid EventId { get; set; }  // required for HMAC QR payload generation
}

public class UpdateTicketRequest
{
    public bool IsUsed { get; set; }
}