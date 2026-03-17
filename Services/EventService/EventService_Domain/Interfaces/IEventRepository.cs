using Common;
using EventService_Domain.Entities;
using EventService_Domain.ModelExtension.EventExtension;

namespace EventService_Domain.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(Guid eventId);
    Event AddEventAsync(Event @event);
    Event UpdateEventAsync(Event @event);
    Task<bool> DeleteEventAsync(Guid eventId);
    Task<bool> EventExistsAsync(Guid eventId);
    IQueryable<Event> GetQueryable();
    Task<PagedResult<Event>> GetEventWithPaging(EventExtensionDto? request);
    Task SaveChangesAsync();
}