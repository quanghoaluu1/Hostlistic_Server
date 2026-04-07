using Common;
using EventService_Application.DTOs;
using EventService_Domain.Enums;

namespace EventService_Application.Interfaces;

public interface ISponsorInteractionService
{
    Task<ApiResponse<SponsorInteractionDto>> CreateAsync(CreateSponsorInteractionDto dto);
    Task<ApiResponse<SponsorInteractionDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<PagedResult<SponsorInteractionDto>>> GetBySponsorIdAsync(Guid sponsorId, BaseQueryParams request);
    Task<ApiResponse<PagedResult<SponsorInteractionDto>>> GetByUserIdAsync(Guid userId, BaseQueryParams request);
}
