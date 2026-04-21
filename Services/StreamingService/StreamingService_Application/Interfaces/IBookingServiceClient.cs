namespace StreamingService_Application.Interfaces;

public class GuestLiveTicketValidationDto
{
    public Guid TicketId { get; set; }
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string? HolderName { get; set; }
    public string? HolderEmail { get; set; }
    public bool IsUsed { get; set; }
}

public interface IBookingServiceClient
{
    Task<GuestLiveTicketValidationDto?> ValidateGuestLiveTicketAsync(Guid eventId, string ticketCode, CancellationToken cancellationToken = default);
}
