using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ITrackService
{
    Task<ApiResponse<TrackDto>> GetTrackByIdAsync(Guid eventId, Guid trackId);
    Task<ApiResponse<PagedResult<TrackDto>>> GetTracksByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<ApiResponse<TrackDto>> CreateTrackAsync(Guid eventId, CreateTrackRequest request);
    Task<ApiResponse<TrackDto>> UpdateTrackAsync(Guid eventId, Guid trackId, UpdateTrackRequest request);
    Task<ApiResponse<bool>> DeleteTrackAsync(Guid eventId, Guid trackId);
}