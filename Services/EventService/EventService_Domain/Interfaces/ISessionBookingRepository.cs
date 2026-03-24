using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISessionBookingRepository
{
    Task<SessionBooking?> GetSessionBookingByIdAsync(Guid bookingId);
    Task<PagedResult<SessionBooking>> GetSessionBookingsBySessionIdAsync(Guid sessionId, int pageNumber, int pageSize, string? sortBy = null);
    Task<PagedResult<SessionBooking>> GetSessionBookingsByUserIdAsync(Guid userId, int pageNumber, int pageSize, string? sortBy = null);
    Task<SessionBooking?> GetSessionBookingByUserAndSessionAsync(Guid userId, Guid sessionId);
    Task<SessionBooking> AddSessionBookingAsync(SessionBooking sessionBooking);
    Task<SessionBooking> UpdateSessionBookingAsync(SessionBooking sessionBooking);
    Task<bool> DeleteSessionBookingAsync(Guid bookingId);
    Task<bool> SessionBookingExistsAsync(Guid bookingId);
    Task<bool> UserHasBookingForSessionAsync(Guid userId, Guid sessionId);
    Task SaveChangesAsync();
}