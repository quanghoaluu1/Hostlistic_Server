using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface IEventRepository
{
    Task<PagedResult<Event>> GetAllEventsAsync(BaseQueryParams request);
    Task<Event?> GetEventByIdAsync(Guid eventId);
    Event AddEventAsync(Event @event);
    Event UpdateEventAsync(Event @event);
    Task<bool> DeleteEventAsync(Guid eventId);
    Task<bool> EventExistsAsync(Guid eventId);
    Task<bool> IsOwnerAsync(Guid eventId, Guid userId);

    IQueryable<Event> GetQueryable();
    Task SaveChangesAsync();
    Task<object> GetDashboardAsync(int? year = null, int? month = null);
    Task<bool> UpdateEventStatus(Event @event);
}