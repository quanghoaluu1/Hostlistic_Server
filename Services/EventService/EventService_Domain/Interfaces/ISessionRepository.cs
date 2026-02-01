using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<IEnumerable<Session>> GetSessionsByEventIdAsync(Guid eventId);
    Task<IEnumerable<Session>> GetSessionsByTrackIdAsync(Guid trackId);
    Task<IEnumerable<Session>> GetSessionsByVenueIdAsync(Guid venueId);
    Task<Session> AddSessionAsync(Session session);
    Task<Session> UpdateSessionAsync(Session session);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> SessionExistsAsync(Guid sessionId);
    Task SaveChangesAsync();
}