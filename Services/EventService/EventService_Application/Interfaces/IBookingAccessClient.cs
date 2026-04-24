namespace EventService_Application.Interfaces;

public interface IBookingAccessClient
{
    Task<bool> HasStreamAccessAsync(Guid eventId, Guid userId);
}
