using Common;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventRepository(EventServiceDbContext dbContext) : IEventRepository
{
    public async Task<PagedResult<Event>> GetAllEventsAsync(BaseQueryParams request)
    {
        var query = dbContext.Events.Include(e => e.Tracks)
            .ThenInclude(t => t.Sessions)
            .ThenInclude(s => s.Lineups)
            .ThenInclude(l => l.Talent)
            .Include(e => e.EventType)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId)
    {
        return await dbContext.Events
            .Include(e => e.Tracks)
                .ThenInclude(t => t.Sessions)
            .ThenInclude(s => s.Lineups)
            .ThenInclude(l => l.Talent)
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

    public async Task<object> GetDashboardAsync(int? year = null, int? month = null)
    {
        IQueryable<Event> query = dbContext.Events;

        if (year.HasValue && month.HasValue)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12.");

            var start = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            query = query.Where(e =>
                e.StartDate.HasValue &&
                e.StartDate >= start &&
                e.StartDate < end
            );
        }

        // ✅ chạy tuần tự (an toàn với DbContext)
        var total = await query.CountAsync();

        var byStatus = await query
            .GroupBy(e => e.EventStatus)
            .Select(g => new
            {
                status = g.Key.ToString(),
                count = g.Count()
            })
            .ToListAsync();

        var byDate = await query
            .Where(e => e.StartDate.HasValue)
            .GroupBy(e => e.StartDate.Value.Date)
            .Select(g => new
            {
                date = g.Key,
                count = g.Count()
            })
            .OrderBy(x => x.date)
            .ToListAsync();

        return new
        {
            total,
            byStatus,
            byDate
        };
    }

    public async Task<Event> UpdateEventStatus(Event @event)
    {
        @event.EventStatus = EventStatus.Unpublished;
        dbContext.Events.Update(@event);
        await SaveChangesAsync();
        return @event;
    }

}