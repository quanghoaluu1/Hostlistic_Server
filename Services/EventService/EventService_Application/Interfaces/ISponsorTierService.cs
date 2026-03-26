using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISponsorTierService
{
    Task<ApiResponse<SponsorTierDto>> CreateAsync(CreateSponsorTierDto dto);
    Task<ApiResponse<SponsorTierDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<PagedResult<SponsorTierDto>>> GetByEventIdAsync(Guid eventId, BaseQueryParams request);
    Task<ApiResponse<SponsorTierDto>> UpdateAsync(Guid id, UpdateSponsorTierDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
