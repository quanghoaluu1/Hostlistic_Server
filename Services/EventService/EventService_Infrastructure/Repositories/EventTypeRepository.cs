using Common;
using EventService_Domain;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventTypeRepository(EventServiceDbContext dbContext) : IEventTypeRepository
{

    public async Task<IReadOnlyList<EventType>> GetAllEventTypesAsync()
    {
        return await dbContext.EventTypes.ToListAsync();
    }

    public async Task<PagedResult<EventType>> GetAllEventTypesAsync(string? name, int pageNumber, int pageSize, string? sortBy = null)
    {
        var query = dbContext.EventTypes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => e.Name.Contains(name));
        }
        query = query.ApplySorting(sortBy);
        return await query.ToPagedResultAsync(pageNumber, pageSize);
    }

    public async Task<EventType?> GetEventTypeByIdAsync(Guid eventTypeId)
    {
        return await dbContext.EventTypes.FindAsync(eventTypeId);
    }

    public EventType AddEventTypeAsync(EventType eventType)
    {
        var newEventType = dbContext.EventTypes.Add(eventType);
        return newEventType.Entity;
    }

    public async Task<bool> EventTypeExistsAsync(Guid eventTypeId)
    {
        return await dbContext.EventTypes.AnyAsync(e => e.Id == eventTypeId);
    }

    public EventType UpdateEventTypeAsync(EventType eventType)
    {
        var updatedEventType = dbContext.EventTypes.Update(eventType);
        return updatedEventType.Entity;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}