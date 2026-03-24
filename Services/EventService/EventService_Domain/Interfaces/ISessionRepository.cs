using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<IEnumerable<Session>> GetSessionsByEventIdAsync(Guid eventId);
    Task<IEnumerable<Session>> GetSessionsByTrackIdAsync(Guid trackId);
    Task<IEnumerable<Session>> GetSessionsByVenueIdAsync(Guid venueId);
    Task<Session?> GetByIdWithinEventAsync(Guid eventId, Guid sessionId);
    
    // === Overlap detection — pushed to database ===
    Task<bool> HasOverlapInTrackAsync(Guid trackId, DateTime start, DateTime end, Guid? excludeSessionId = null);
    Task<bool> HasOverlapInVenueAsync(Guid venueId, DateTime start, DateTime end, Guid? excludeSessionId = null);
    
    // === Booking queries ===
    Task<int> GetBookedCountAsync(Guid sessionId);
    Task<bool> HasBookingsAsync(Guid sessionId);
    
    Task<int> GetMaxSortOrderInTrackAsync(Guid trackId);
    Task<Session> AddSessionAsync(Session session);
    Task<Session> UpdateSessionAsync(Session session);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> SessionExistsAsync(Guid sessionId);
    Task SaveChangesAsync();
}