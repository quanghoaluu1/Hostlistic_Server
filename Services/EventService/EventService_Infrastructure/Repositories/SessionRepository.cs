using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly EventServiceDbContext _context;

    public SessionRepository(EventServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Event)
            .Include(s => s.Venue)
            .Include(s => s.Track)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<IEnumerable<Session>> GetSessionsByEventIdAsync(Guid eventId)
    {
        return await _context.Sessions
            .Include(s => s.Venue)
            .Include(s => s.Track)
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetSessionsByTrackIdAsync(Guid trackId)
    {
        return await _context.Sessions
            .Include(s => s.Venue)
            .Where(s => s.TrackId == trackId)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Session>> GetSessionsByVenueIdAsync(Guid venueId)
    {
        return await _context.Sessions
            .Include(s => s.Event)
            .Include(s => s.Track)
            .Where(s => s.VenueId == venueId)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdWithinEventAsync(Guid eventId, Guid sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Event)
            .Include(s => s.Track)
            .Include(s => s.Venue)
            .Include(s => s.Lineups)
            .ThenInclude(l => l.Talent)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.EventId == eventId);
    }
    
    public async Task<bool> HasOverlapInTrackAsync(Guid trackId, DateTime start, DateTime end, Guid? excludeSessionId = null)
    {
        var query = _context.Sessions
            .Where(s => s.TrackId == trackId
                        && s.Status != SessionStatus.Cancelled
                        && s.StartTime != null && s.EndTime != null
                        && s.StartTime < end
                        && s.EndTime > start);
 
        if (excludeSessionId.HasValue)
            query = query.Where(s => s.Id != excludeSessionId.Value);
 
        return await query.AnyAsync();
    }

    public async Task<bool> HasOverlapInVenueAsync(Guid venueId, DateTime start, DateTime end, Guid? excludeSessionId = null)
    {
        var query =  _context.Sessions
            .Where(s => s.VenueId == venueId
                        && s.Status != SessionStatus.Cancelled
                        && s.StartTime != null && s.EndTime != null
                        && s.StartTime < end
                        && s.EndTime > start);
 
        if (excludeSessionId.HasValue)
            query = query.Where(s => s.Id != excludeSessionId.Value);
 
        return await query.AnyAsync();
    }

    public async Task<int> GetBookedCountAsync(Guid sessionId)
    {
        return await _context.SessionBookings
            .CountAsync(sb => sb.SessionId == sessionId
                              && sb.Status == BookingStatus.Confirmed);
    }

    public async Task<bool> HasBookingsAsync(Guid sessionId)
    {
        return await _context.SessionBookings
            .AnyAsync(sb => sb.SessionId == sessionId
                            && sb.Status == BookingStatus.Confirmed);
    }

    public async Task<int> GetMaxSortOrderInTrackAsync(Guid trackId)
    {
        return await _context.Sessions
            .Where(s => s.TrackId == trackId)
            .Select(s => (int?)s.SortOrder)
            .MaxAsync() ?? 0;
    }

    public async Task<Session> AddSessionAsync(Session session)
    {
        session.Id = Guid.CreateVersion7();
        await _context.Sessions.AddAsync(session);
        return session;
    }

    public async Task<Session> UpdateSessionAsync(Session session)
    {
        _context.Sessions.Update(session);
        return session;
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null)
            return false;

        _context.Sessions.Remove(session);
        return true;
    }

    public async Task<bool> SessionExistsAsync(Guid sessionId)
    {
        return await _context.Sessions.AnyAsync(s => s.Id == sessionId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}