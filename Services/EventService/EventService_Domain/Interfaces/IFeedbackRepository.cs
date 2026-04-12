using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<Feedback> AddFeedbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackByIdAsync(Guid id);
        Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
        Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId);
        Task<IEnumerable<Feedback>> GetFeedbacksBySessionAsync(Guid sessionId);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> DeleteFeedbackAsync(Guid id);
    }
}
