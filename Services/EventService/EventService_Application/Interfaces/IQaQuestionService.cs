using Common;
using EventService_Application.DTOs;
using EventService_Domain.Entities;
using EventService_Domain.Enums;

namespace EventService_Application.Interfaces
{
    public interface IQaQuestionService
    {
        Task<ApiResponse<QaQuestionDto>> AddQaQuestionAsync(CreateQaQuestionDto request);
        Task<ApiResponse<QaQuestionDto>> UpdateQaQuestionStatusAsync(Guid qaQuestionId, QaStatus newStatus);
        Task<ApiResponse<bool>> DeleteQaQuestionAsync(Guid qaQuestionId);
        Task<ApiResponse<List<QaQuestionDto>>> GetQaQuestionsBySessionIdAsync(Guid sessionId);
        Task<ApiResponse<List<QaQuestionDto>>> GetQaQuestionsByUserIdAsync(Guid userId);
        Task<ApiResponse<QaQuestionDto>> QaQuestionVote(QaVoteDto request);
        Task<ApiResponse<int>> GetQaQuestionVotes(Guid qaQuestionId);
        Task<ApiResponse<QaQuestion>> GetQaQuestionByIdAsync(Guid qaQuestionId);
    }
}
