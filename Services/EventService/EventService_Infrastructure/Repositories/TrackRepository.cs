using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class TrackRepository : ITrackRepository
{
    private readonly EventServiceDbContext _context;

    public TrackRepository(EventServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Track?> GetTrackByIdAsync(Guid trackId)
    {
        return await _context.Tracks
            .Include(t => t.Event)
            .Include(t => t.Sessions)
            .ThenInclude(s => s.Lineups)
            .FirstOrDefaultAsync(t => t.Id == trackId);
    }

    public async Task<Track?> GetByIdWithinEventAsync(Guid eventId, Guid trackId)
    {
        return await _context.Tracks
            .Include(t => t.Event)
            .Include(t => t.Sessions)
            .FirstOrDefaultAsync(t => t.Id == trackId && t.EventId == eventId);
    }
    

    public async Task<List<Track>> GetByEventIdAsync(Guid eventId)
    {
        return await _context.Tracks
            .Include(t => t.Sessions)
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }
    public async Task<List<Track>> GetByEventIdWithSessionsAsync(Guid eventId)
    {
        return await _context.Tracks
            .Include(t => t.Sessions)
            .ThenInclude(s => s.Venue)
            .Include(t => t.Sessions)
            .ThenInclude(s => s.Lineups)
            .ThenInclude(l => l.Talent)
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .AsSplitQuery() // Avoid cartesian explosion with multiple Includes
            .ToListAsync();
    }
    public async Task<bool> ExistsAsync(Guid trackId)
    {
        return await _context.Tracks.AnyAsync(t => t.Id == trackId);
    }
    public async Task<bool> ExistsWithinEventAsync(Guid eventId, Guid trackId)
    {
        return await _context.Tracks
            .AnyAsync(t => t.Id == trackId && t.EventId == eventId);
    }
    public async Task<bool> HasSessionsAsync(Guid trackId)
    {
        return await _context.Sessions
            .AnyAsync(s => s.TrackId == trackId);
    }

    public async Task<int> GetMaxSortOrderAsync(Guid eventId)
    {
        return await _context.Tracks
            .Where(t => t.EventId == eventId)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync() ?? 0;
    }

    public async Task<IEnumerable<Track>> GetTracksByEventIdAsync(Guid eventId)
    {
        return await _context.Tracks
            .Include(t => t.Sessions)
            .Where(t => t.EventId == eventId)
            .ToListAsync();
    }

    public async Task<Track> AddTrackAsync(Track track)
    {
        track.Id = Guid.CreateVersion7();
        await _context.Tracks.AddAsync(track);
        return track;
    }

    public async Task<Track> UpdateTrackAsync(Track track)
    {
        _context.Tracks.Update(track);
        return track;
    }

    public async Task<bool> DeleteTrackAsync(Guid trackId)
    {
        var track = await _context.Tracks.FindAsync(trackId);
        if (track == null)
            return false;

        _context.Tracks.Remove(track);
        return true;
    }

    public async Task<bool> TrackExistsAsync(Guid trackId)
    {
        return await _context.Tracks.AnyAsync(t => t.Id == trackId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}