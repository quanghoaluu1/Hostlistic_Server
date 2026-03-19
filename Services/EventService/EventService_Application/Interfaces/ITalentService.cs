using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface ITalentService
    {
        Task<ApiResponse<TalentDto>> GetTalentByIdAsync(Guid talentId);
        Task<ApiResponse<List<TalentDto>>> GetAllTalentsAsync();
        Task<ApiResponse<TalentDto>> CreateTalentAsync(CreateTalentDto request);
        Task<ApiResponse<TalentDto>> UpdateTalentAsync(Guid talentId, UpdateTalentDto request);
        Task<ApiResponse<PagedResult<TalentDto>>> GetAllTalentsWPagingAsync(TalentSearchRequest? request);
        Task<ApiResponse<bool>> DeleteTalentAsync(Guid talentId);

    }
}
