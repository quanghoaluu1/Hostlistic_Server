using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventRepository(EventServiceDbContext dbContext) : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetAllEventsAsync()
    {
        return await dbContext.Events.ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId)
    {
        return await dbContext.Events.FindAsync(eventId);
    }

    public Event AddEventAsync(Event @event)
    {
        return dbContext.Events.Add(@event).Entity;
    }

    public Event UpdateEventAsync(Event @event)
    {
        return dbContext.Events.Update(@event).Entity;
    }

    public Task<bool> DeleteEventAsync(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EventExistsAsync(Guid eventId)
    {
        return await dbContext.Events.AnyAsync(e => e.Id == eventId);
    }
}