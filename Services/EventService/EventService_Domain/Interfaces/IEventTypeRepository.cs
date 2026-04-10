using Common;

namespace EventService_Domain.Interfaces;

public interface IEventTypeRepository
{
    Task<PagedResult<EventType>> GetAllEventTypesAsync(BaseQueryParams request);
    Task<EventType?> GetEventTypeByIdAsync(Guid eventTypeId);
    EventType AddEventTypeAsync(EventType eventType);
    Task<bool> EventTypeExistsAsync(Guid eventTypeId);
    EventType UpdateEventTypeAsync(EventType eventType);
    Task SaveChangesAsync();

}