using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ITrackRepository
{
    Task<Track?> GetTrackByIdAsync(Guid trackId);
    Task<IEnumerable<Track>> GetTracksByEventIdAsync(Guid eventId);
    Task<Track> AddTrackAsync(Track track);
    Task<Track> UpdateTrackAsync(Track track);
    Task<bool> DeleteTrackAsync(Guid trackId);
    Task<bool> TrackExistsAsync(Guid trackId);
    Task SaveChangesAsync();
}