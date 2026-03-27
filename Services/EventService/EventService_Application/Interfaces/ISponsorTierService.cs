using Common;
using EventService_Application.DTOs;
using EventService_Domain.Entities;

namespace EventService_Application.Interfaces;

public interface ISponsorTierService
{
    Task<ApiResponse<SponsorTierDto>> CreateAsync(CreateSponsorTierDto dto);
    Task<ApiResponse<SponsorTierDto>> GetByIdAsync(Guid id);
    Task<IReadOnlyList<SponsorTier>> GetAllSponsorTiersAsync();
    Task<ApiResponse<IEnumerable<SponsorTierDto>>> GetByEventIdAsync(Guid eventId);
    Task<ApiResponse<SponsorTierDto>> UpdateAsync(Guid id, UpdateSponsorTierDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
