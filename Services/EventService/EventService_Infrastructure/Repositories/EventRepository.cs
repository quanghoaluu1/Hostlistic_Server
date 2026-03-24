using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventRepository(EventServiceDbContext dbContext) : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetAllEventsAsync()
    {
        return await dbContext.Events.Include(e => e.Tracks)
            .ThenInclude(t => t.Sessions)
            .ThenInclude(s => s.Lineups)
            .ThenInclude(l => l.Talent)
            .Include(e => e.EventType)
            .Include(e => e.Venue).ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId)
    {
        return await dbContext.Events
            .Include(e => e.Tracks)
                .ThenInclude(t => t.Sessions)
            .ThenInclude(s => s.Lineups)
            .ThenInclude(l => l.Talent)
            .Include(e => e.Venue)
            .Include(e => e.EventType)
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    public Event AddEventAsync(Event @event)
    {
        return dbContext.Events.Add(@event).Entity;
    }

    public Event UpdateEventAsync(Event @event)
    {
        return dbContext.Events.Update(@event).Entity;
    }

    

    public IQueryable<Event> GetQueryable()
    {
        return dbContext.Events.AsQueryable().AsNoTracking();
    }

    public Task<bool> DeleteEventAsync(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EventExistsAsync(Guid eventId)
    {
        return await dbContext.Events.AnyAsync(e => e.Id == eventId);
    }
    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
    
}