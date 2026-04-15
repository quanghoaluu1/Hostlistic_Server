using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface IFeedbackService
    {
        Task<ApiResponse<FeedbackDto>> AddFeedbackAsync(CreateFeedbackDto request, Guid userId, string userFullName);
        Task<ApiResponse<FeedbackDto>> GetFeedbackByIdAsync(Guid id);
        Task<ApiResponse<List<FeedbackDto>>> GetAllFeedbacksAsync();
        Task<ApiResponse<List<FeedbackDto>>> GetFeedbacksByEventIdAsync(Guid eventId);
        Task<ApiResponse<FeedbackDto?>> GetMyFeedbackForEventAsync(Guid eventId, Guid userId);
        Task<ApiResponse<FeedbackDto>> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto request, Guid userId);
        Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id, Guid userId);
    }
}
