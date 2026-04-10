using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface IEventDayRepository
{
    Task<EventDay?> GetByIdAsync(Guid eventId, Guid dayId);
    Task<IReadOnlyList<EventDay>> GetByEventIdAsync(Guid eventId);
    Task<EventDay?> GetByEventAndDateAsync(Guid eventId, DateOnly date);
    Task<EventDay?> GetByEventAndDayNumberAsync(Guid eventId, int dayNumber);
    Task<bool> ExistsAsync(Guid eventId, DateOnly date);
    Task<bool> AnyExistAsync(Guid eventId);
    Task AddAsync(EventDay entity);
    Task AddRangeAsync(IEnumerable<EventDay> entities);
    void Update(EventDay entity);
    void Remove(EventDay entity);
    Task SaveChangesAsync();
}
