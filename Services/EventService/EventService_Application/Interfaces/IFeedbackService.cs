using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackDto>> AddFeedbackAsync(FeedbackDto request);
        Task<ApiResponse<FeedbackDto>> GetFeedbackByIdAsync(Guid id);
        Task<ApiResponse<List<FeedbackDto>>> GetAllFeedbacksAsync();
        Task<ApiResponse<List<FeedbackDto>>> GetFeedbacksByEventIdAsync(Guid eventId);
        Task<ApiResponse<List<FeedbackDto>>> GetFeedbacksBySessionIdAsync(Guid sessionId);
        Task<ApiResponse<FeedbackDto>> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto request);
        Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id);
    }
}
