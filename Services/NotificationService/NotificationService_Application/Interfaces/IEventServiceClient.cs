using NotificationService_Application.Dtos.ServiceClientDtos;

namespace NotificationService_Application.Interfaces;

public interface IEventServiceClient
{
    Task<EventDetailDto?> GetEventAsync(Guid eventId, CancellationToken ct = default);
}