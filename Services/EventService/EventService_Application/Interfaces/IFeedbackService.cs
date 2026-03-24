using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackDto>> AddFeedbackAsync(FeedbackDto request);
        Task<ApiResponse<FeedbackDto>> GetFeedbackByIdAsync(Guid id);
        Task<ApiResponse<PagedResult<FeedbackDto>>> GetAllFeedback(BaseQueryParams request);
        Task<ApiResponse<PagedResult<FeedbackDto>>> GetFeedbacksByEventIdAsync(Guid eventId, BaseQueryParams request);
        Task<ApiResponse<PagedResult<FeedbackDto>>> GetFeedbacksBySessionIdAsync(Guid sessionId, BaseQueryParams request);
        Task<ApiResponse<FeedbackDto>> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto request);
        Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id);
    }
}
