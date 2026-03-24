using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<PagedResult<Session>> GetSessionsByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<PagedResult<Session>> GetSessionsByTrackIdAsync(Guid trackId, BaseQueryParams request);
    Task<PagedResult<Session>> GetSessionsByVenueIdAsync(Guid venueId, BaseQueryParams request);
    Task<Session> AddSessionAsync(Session session);
    Task<Session> UpdateSessionAsync(Session session);
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> SessionExistsAsync(Guid sessionId);
    Task SaveChangesAsync();
}