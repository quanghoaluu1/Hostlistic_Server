using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces
{
    public interface ILineupService
    {
        Task<ApiResponse<BatchLineupResultDto>> CreateLineupAsync(CreateLineupsRequest request);
        Task<ApiResponse<PagedResult<LineupDto>>> GetLineupsByEventIdAsync(Guid eventId, BaseQueryParams request);
        Task<ApiResponse<LineupDto>> GetLineupById(Guid lineupId);
        Task<ApiResponse<PagedResult<LineupDto>>> GetAllLineups(BaseQueryParams request);
        Task<ApiResponse<LineupDto>> UpdateLineupAsync(LineupDto request);
        Task<ApiResponse<bool>> DeleteLineupAsync(Guid lineupId);
    }
}
