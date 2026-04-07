using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ITrackRepository
{
    Task<Track?> GetTrackByIdAsync(Guid trackId);
    Task<PagedResult<Track>> GetTracksByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<Track?> GetByIdWithinEventAsync(Guid eventId, Guid trackId); // scoped query
    Task<List<Track>> GetByEventIdAsync(Guid eventId);
    Task<List<Track>> GetByEventIdWithSessionsAsync(Guid eventId);
    Task<bool> ExistsWithinEventAsync(Guid eventId, Guid trackId);
    Task<bool> ExistsAsync(Guid trackId);
    Task<bool> HasSessionsAsync(Guid trackId);
    Task<int> GetMaxSortOrderAsync(Guid eventId);
    Task<IEnumerable<Track>> GetTracksByEventIdAsync(Guid eventId);
    Task<Track> AddTrackAsync(Track track);
    Task<Track> UpdateTrackAsync(Track track);
    Task<bool> DeleteTrackAsync(Guid trackId);
    Task<bool> TrackExistsAsync(Guid trackId);
    Task SaveChangesAsync();
}