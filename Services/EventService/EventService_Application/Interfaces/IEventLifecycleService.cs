using Common;

namespace EventService_Application.Interfaces;

public interface IEventLifecycleService
{
    Task<ApiResponse<bool>> StartEventAsync(Guid eventId, Guid requesterId);
    Task<ApiResponse<bool>> CompleteEventAsync(Guid eventId, Guid requesterId);
    Task<ApiResponse<bool>> CancelEventAsync(Guid eventId, Guid requesterId, string? reason);
    Task<ApiResponse<bool>> PostponeEventAsync(Guid eventId, Guid requesterId, DateTime? newStartTime, DateTime? newEndTime, string reason);
}