using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface IPollService
    {
        Task<ApiResponse<PollDto>> AddPollAsync(CreatePollRequest request);
        Task<ApiResponse<bool>> DeletePollAsync(Guid pollId);
        Task<ApiResponse<PollDto>> GetPollByIdAsync(Guid pollId);
        Task<ApiResponse<List<PollDto>>> GetPollsBySessionIdAsync(Guid sessionId);
        Task<ApiResponse<PollDto>> UpdatePollAsync(Guid pollId, UpdatePollRequest request);
    }
}
