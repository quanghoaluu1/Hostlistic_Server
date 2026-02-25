using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface IQaQuestionRepository
    {
        Task<QaQuestion> AddQaQuestionAsync(QaQuestion qaQuestion);
        Task<QaQuestion?> GetQaQuestionByIdAsync(Guid id);
        Task<IEnumerable<QaQuestion>> GetQaQuestionsBySessionIdAsync(Guid sessionId);
        Task<IEnumerable<QaQuestion>> GetQaQuestionsByUserIdAsync(Guid userId);
        Task<QaQuestion> UpdateQaQuestionAsync(QaQuestion qaQuestion);
        Task<bool> DeleteQaQuestionAsync(QaQuestion qaQuestion);
        Task<bool> QaQuestionVote(QaVote qaVote);
        Task<int> GetQaQuestionVotes(Guid qaQuestionId);
    }
}
