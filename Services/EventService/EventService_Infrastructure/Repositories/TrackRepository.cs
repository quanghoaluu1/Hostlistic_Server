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
            .FirstOrDefaultAsync(t => t.Id == trackId);
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
        track.Id = Guid.NewGuid();
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