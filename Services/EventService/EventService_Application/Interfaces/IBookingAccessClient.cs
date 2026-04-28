namespace EventService_Application.Interfaces;

public class BookingStreamAccessDto
{
    public bool HasAccess { get; set; }
    public List<Guid> TicketTypeIds { get; set; } = [];
}

public interface IBookingAccessClient
{
    Task<BookingStreamAccessDto> GetStreamAccessAsync(Guid eventId, Guid userId);
}
