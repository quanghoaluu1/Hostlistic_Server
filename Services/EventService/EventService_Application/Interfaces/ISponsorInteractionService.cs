using Common;
using EventService_Application.DTOs;
using EventService_Domain.Enums;

namespace EventService_Application.Interfaces;

public interface ISponsorInteractionService
{
    Task<ApiResponse<SponsorInteractionDto>> CreateAsync(CreateSponsorInteractionDto dto);
    Task<ApiResponse<SponsorInteractionDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<SponsorInteractionDto>>> GetBySponsorIdAsync(Guid sponsorId);
    Task<ApiResponse<IEnumerable<SponsorInteractionDto>>> GetByUserIdAsync(Guid userId);

    Task TrackInteractionAsync(Guid sponsorId, Guid userId, InteractionType type);
    Task<SponsorInteractionStatsDto> GetInteractionStatsAsync(Guid sponsorId);
}
