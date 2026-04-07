using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface ILineupService
    {
        Task<ApiResponse<BatchLineupResultDto>> CreateLineupAsync(CreateLineupsRequest request);
        Task<ApiResponse<List<LineupDto>>> GetLineupsByEventIdAsync(Guid eventId);
        Task<ApiResponse<LineupDto>> GetLineupById(Guid lineupId);
        Task<ApiResponse<List<LineupDto>>> GetAllLineups();
        Task<ApiResponse<LineupDto>> UpdateLineupAsync(LineupDto request);
        Task<ApiResponse<bool>> DeleteLineupAsync(Guid lineupId);
        Task<ApiResponse<PublicLineupResponse>> GetPublicLineupAsync(Guid eventId);
    }
}
