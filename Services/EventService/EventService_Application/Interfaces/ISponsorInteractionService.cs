using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISponsorInteractionService
{
    Task<ApiResponse<SponsorInteractionDto>> CreateAsync(CreateSponsorInteractionDto dto);
    Task<ApiResponse<SponsorInteractionDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<SponsorInteractionDto>>> GetBySponsorIdAsync(Guid sponsorId);
    Task<ApiResponse<IEnumerable<SponsorInteractionDto>>> GetByUserIdAsync(Guid userId);
}
