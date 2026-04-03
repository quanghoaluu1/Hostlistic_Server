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
    public string? HolderName { get; set; }
    public string? HolderEmail { get; set; }
    public string? HolderPhone { get; set; }

    // Denormalized fields (persisted on entity)
    public string TicketTypeName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;

    // DTO-only enrichment (not persisted)
    public decimal Price { get; set; }
}

public class CreateTicketRequest
{
    public Guid OrderId { get; set; }
    public Guid TicketTypeId { get; set; }
    public Guid EventId { get; set; }  // required for HMAC QR payload generation
    public string TicketTypeName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string? HolderName { get; set; }
    public string? HolderEmail { get; set; }
    public string? HolderPhone { get; set; }
}

public class UpdateTicketRequest
{
    public bool IsUsed { get; set; }
}