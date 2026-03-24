using Common;
using EventService_Domain.Entities;
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

    public async Task<PagedResult<Session>> GetSessionsByEventIdAsync(Guid eventId, BaseQueryParams request)
    {
        var query = _context.Sessions
            .Include(s => s.Venue)
            .Include(s => s.Track)
            .Where(s => s.EventId == eventId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<PagedResult<Session>> GetSessionsByTrackIdAsync(Guid trackId, BaseQueryParams request)
    {
        var query = _context.Sessions
            .Include(s => s.Event)
            .Include(s => s.Venue)
            .Where(s => s.TrackId == trackId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<PagedResult<Session>> GetSessionsByVenueIdAsync(Guid venueId, BaseQueryParams request)
    {
        var query = _context.Sessions
            .Include(s => s.Event)
            .Include(s => s.Track)
            .Where(s => s.VenueId == venueId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<Session> AddSessionAsync(Session session)
    {
        session.Id = Guid.NewGuid();
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