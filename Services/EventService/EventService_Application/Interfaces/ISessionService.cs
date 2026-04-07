using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionService
{
    Task<ApiResponse<SessionDto>> GetSessionByIdAsync(Guid eventId, Guid sessionId);
    Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByEventIdAsync(Guid eventId);
    Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByTrackIdAsync(Guid eventId, Guid trackId, BaseQueryParams request);
    Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByVenueIdAsync(Guid venueId, BaseQueryParams request);

    Task<ApiResponse<SessionDto>> UpdateSessionStatusAsync(
        Guid eventId, Guid sessionId, UpdateSessionStatusRequest request);
    Task<ApiResponse<SessionDto>> CreateSessionAsync(Guid eventId, CreateSessionRequest request);
    Task<ApiResponse<SessionDto>> UpdateSessionAsync(Guid eventId, Guid sessionId, UpdateSessionRequest request);
    Task<ApiResponse<bool>> DeleteSessionAsync(Guid eventId, Guid sessionId);
}