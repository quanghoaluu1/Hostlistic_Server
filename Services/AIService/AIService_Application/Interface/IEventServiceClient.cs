using AIService_Application.DTOs.Responses;

namespace AIService_Application.Interface;

public interface IEventServiceClient
{
    Task<EventDetailDto?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default);
}
