using EventService_Domain.Enums;
using EventService_Infrastructure.Data;
using EventService_Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class AgendaRepository(EventServiceDbContext context) : IAgendaRepository
{
    public async Task<AgendaQueryResult?> GetAgendaAsync(Guid eventId, Guid? currentUserId)
    {
        // 1. Load event metadata
        var eventEntity = await context.Events
            .AsNoTracking()
            .Where(e => e.Id == eventId)
            .Select(e => new { e.Id, e.StartDate, e.EndDate, e.TimeZoneId })
            .FirstOrDefaultAsync();
 
        if (eventEntity is null)
            return null;
 
        // 2. Load tracks with sessions + speakers in a split query
        var tracks = await context.Tracks
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .Include(t => t.Sessions.OrderBy(s => s.StartTime).ThenBy(s => s.SortOrder))
                .ThenInclude(s => s.Venue)
            .Include(t => t.Sessions)
                .ThenInclude(s => s.Lineups)
                    .ThenInclude(l => l.Talent)
            .OrderBy(t => t.SortOrder)
            .AsSplitQuery()
            .ToListAsync();
 
        // 3. Batch load booking counts for all sessions in this event (single query)
        //    SQL: SELECT SessionId, COUNT(*) FROM SessionBookings
        //         WHERE SessionId IN (...) AND Status = Confirmed
        //         GROUP BY SessionId
        var allSessionIds = tracks
            .SelectMany(t => t.Sessions)
            .Select(s => s.Id)
            .ToList();
 
        var bookingCounts = await context.SessionBookings
            .AsNoTracking()
            .Where(sb => allSessionIds.Contains(sb.SessionId)
                && sb.Status == BookingStatus.Confirmed)
            .GroupBy(sb => sb.SessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count);
 
        // 4. If user is authenticated, batch load their bookings (single query)
        var userBookedSessionIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            userBookedSessionIds = (await context.SessionBookings
                .AsNoTracking()
                .Where(sb => sb.UserId == currentUserId.Value
                    && allSessionIds.Contains(sb.SessionId)
                    && sb.Status == BookingStatus.Confirmed)
                .Select(sb => sb.SessionId)
                .ToListAsync())
                .ToHashSet();
        }
 
        // 5. Load EventDays for this event (single query)
        var eventDays = await context.EventDays
            .AsNoTracking()
            .Where(d => d.EventId == eventId)
            .OrderBy(d => d.DayNumber)
            .Select(d => new AgendaEventDayData
            {
                Id = d.Id,
                DayNumber = d.DayNumber,
                Date = d.Date,
                Title = d.Title,
                Theme = d.Theme
            })
            .ToListAsync();

        // 6. Assemble result — O(1) lookups via dictionaries
        var result = new AgendaQueryResult
        {
            EventId = eventEntity.Id,
            EventStartDate = eventEntity.StartDate,
            EventEndDate = eventEntity.EndDate,
            TimeZoneId = eventEntity.TimeZoneId,
            EventDays = eventDays,
            Tracks = tracks.Select(t => new AgendaTrackData
            {
                Id = t.Id,
                Name = t.Name,
                ColorHex = t.ColorHex,
                SortOrder = t.SortOrder,
                Sessions = t.Sessions.Select(s => new AgendaSessionData
                {
                    Id = s.Id,
                    Title = s.Title,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    TotalCapacity = s.TotalCapacity,
                    BookedCount = bookingCounts.GetValueOrDefault(s.Id, 0),
                    VenueName = s.Venue?.Name,
                    Status = s.Status,
                    IsBookedByCurrentUser = userBookedSessionIds.Contains(s.Id),
                    Speakers = s.Lineups
                        .Where(l => l.Talent != null)
                        .Select(l => new AgendaSpeakerData
                        {
                            TalentId = l.Talent.Id,
                            Name = l.Talent.Name,
                            AvatarUrl = l.Talent.AvatarUrl
                        }).ToList()
                }).ToList()
            }).ToList()
        };
 
        return result;
    }
}