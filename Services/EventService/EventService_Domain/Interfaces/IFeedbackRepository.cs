using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<Feedback> AddFeddbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackByIdAsync(Guid id);
        Task<PagedResult<Feedback>> GetAllFeedbacksAsync(int pageNumber, int pageSize, string? sortBy = null);
        Task<PagedResult<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId, int pageNumber, int pageSize, string? sortBy = null);
        Task<PagedResult<Feedback>> GetFeedbacksBySessionAsync(Guid sessionId, int pageNumber, int pageSize, string? sortBy = null);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> DeleteFeedbackAsync(Guid id);
    }
}
