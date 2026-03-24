using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISessionBookingRepository
{
    Task<SessionBooking?> GetSessionBookingByIdAsync(Guid bookingId);
    Task<IEnumerable<SessionBooking>> GetSessionBookingsBySessionIdAsync(Guid sessionId);
    Task<IEnumerable<SessionBooking>> GetSessionBookingsByUserIdAsync(Guid userId);
    Task<SessionBooking?> GetSessionBookingByUserAndSessionAsync(Guid userId, Guid sessionId);
    
    Task<SessionBooking?> GetByUserAndSessionAsync(Guid userId, Guid sessionId);
    Task<List<SessionBooking>> GetByUserAndEventAsync(Guid userId, Guid eventId);
    
    // === Conflict detection — soft warning ===
    Task<List<Session>> GetConflictingSessionsAsync(
        Guid userId, Guid eventId, DateTime start, DateTime end, Guid? excludeSessionId = null);
    Task<SessionBooking> AddSessionBookingAsync(SessionBooking sessionBooking);
    Task<SessionBooking> UpdateSessionBookingAsync(SessionBooking sessionBooking);
    Task<bool> DeleteSessionBookingAsync(Guid bookingId);
    Task<bool> SessionBookingExistsAsync(Guid bookingId);
    Task<bool> UserHasBookingForSessionAsync(Guid userId, Guid sessionId);
    Task SaveChangesAsync();
}