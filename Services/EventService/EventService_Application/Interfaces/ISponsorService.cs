using Common;
using EventService_Application.DTOs;
using EventService_Domain.Enums;

namespace EventService_Application.Interfaces;

public interface ISponsorService
{
    Task<ApiResponse<SponsorDto>> CreateAsync(CreateSponsorDto dto);
    Task<ApiResponse<SponsorDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<SponsorDto>>> GetByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<SponsorDto>>> GetByTierIdAsync(Guid tierId);
    Task<ApiResponse<SponsorDto>> UpdateAsync(Guid id, UpdateSponsorDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);

    Task<IEnumerable<SponsorPublicDto>> GetSponsorsByEventAsync(Guid eventId);
}
