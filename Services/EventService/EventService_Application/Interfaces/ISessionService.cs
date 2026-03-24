using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionService
{
    Task<ApiResponse<SessionDto>> GetSessionByIdAsync(Guid sessionId);
    Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByTrackIdAsync(Guid trackId, BaseQueryParams request);
    Task<ApiResponse<PagedResult<SessionDto>>> GetSessionsByVenueIdAsync(Guid venueId, BaseQueryParams request);
    Task<ApiResponse<SessionDto>> CreateSessionAsync(CreateSessionRequest request);
    Task<ApiResponse<SessionDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request);
    Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId);
}