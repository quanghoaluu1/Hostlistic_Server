using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventDayRepository(EventServiceDbContext dbContext) : IEventDayRepository
{
    public async Task<EventDay?> GetByIdAsync(Guid eventId, Guid dayId)
    {
        return await dbContext.EventDays
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dayId && d.EventId == eventId);
    }

    public async Task<IReadOnlyList<EventDay>> GetByEventIdAsync(Guid eventId)
    {
        return await dbContext.EventDays
            .AsNoTracking()
            .Where(d => d.EventId == eventId)
            .OrderBy(d => d.DayNumber)
            .ToListAsync();
    }

    public async Task<EventDay?> GetByEventAndDateAsync(Guid eventId, DateOnly date)
    {
        return await dbContext.EventDays
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EventId == eventId && d.Date == date);
    }

    public async Task<EventDay?> GetByEventAndDayNumberAsync(Guid eventId, int dayNumber)
    {
        return await dbContext.EventDays
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.EventId == eventId && d.DayNumber == dayNumber);
    }

    public async Task<bool> ExistsAsync(Guid eventId, DateOnly date)
    {
        return await dbContext.EventDays
            .AnyAsync(d => d.EventId == eventId && d.Date == date);
    }

    public async Task<bool> AnyExistAsync(Guid eventId)
    {
        return await dbContext.EventDays
            .AnyAsync(d => d.EventId == eventId);
    }

    public async Task AddAsync(EventDay entity)
    {
        await dbContext.EventDays.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<EventDay> entities)
    {
        await dbContext.EventDays.AddRangeAsync(entities);
    }

    public void Update(EventDay entity)
    {
        dbContext.EventDays.Update(entity);
    }

    public void Remove(EventDay entity)
    {
        dbContext.EventDays.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}
