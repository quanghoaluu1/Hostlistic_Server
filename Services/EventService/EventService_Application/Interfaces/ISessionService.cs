using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISessionService
{
    Task<ApiResponse<SessionDto>> GetSessionByIdAsync(Guid sessionId);
    Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByTrackIdAsync(Guid trackId);
    Task<ApiResponse<IEnumerable<SessionDto>>> GetSessionsByVenueIdAsync(Guid venueId);
    Task<ApiResponse<SessionDto>> CreateSessionAsync(CreateSessionRequest request);
    Task<ApiResponse<SessionDto>> UpdateSessionAsync(Guid sessionId, UpdateSessionRequest request);
    Task<ApiResponse<bool>> DeleteSessionAsync(Guid sessionId);
}