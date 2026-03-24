using Common;

namespace EventService_Domain.Interfaces;

public interface IEventTypeRepository
{
    Task<IReadOnlyList<EventType>> GetAllEventTypesAsync();
    Task<EventType?> GetEventTypeByIdAsync(Guid eventTypeId);
    EventType AddEventTypeAsync(EventType eventType);
    Task<bool> EventTypeExistsAsync(Guid eventTypeId);
    EventType UpdateEventTypeAsync(EventType eventType);
    Task SaveChangesAsync();
    Task<PagedResult<EventType>> GetAllEventTypesAsync(string? name, int pageNumber, int pageSize, string? sortBy = null);


}