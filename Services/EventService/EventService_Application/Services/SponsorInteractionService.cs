using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class SponsorInteractionService(ISponsorInteractionRepository repository, ISponsorRepository sponsorRepository) : ISponsorInteractionService
{
    public async Task<ApiResponse<SponsorInteractionDto>> CreateAsync(CreateSponsorInteractionDto dto)
    {
        if (dto.SponsorId == Guid.Empty || dto.UserId == Guid.Empty)
            return ApiResponse<SponsorInteractionDto>.Fail(400, "Dữ liệu interaction không hợp lệ");

        var sponsor = await sponsorRepository.GetByIdAsync(dto.SponsorId);
        if (sponsor == null)
            return ApiResponse<SponsorInteractionDto>.Fail(400, "Sponsor không tồn tại");

        var entity = new SponsorInteraction
        {
            Id = Guid.NewGuid(),
            SponsorId = dto.SponsorId,
            UserId = dto.UserId,
            InteractionType = dto.InteractionType,
            InteractionDate = DateTime.UtcNow
        };

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SponsorInteractionDto>();
        return ApiResponse<SponsorInteractionDto>.Success(201, "Tạo interaction thành công", result);
    }

    public async Task<ApiResponse<SponsorInteractionDto>> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SponsorInteractionDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<SponsorInteractionDto>();
        return ApiResponse<SponsorInteractionDto>.Success(200, "OK", dto);
    }

    public async Task<ApiResponse<PagedResult<SponsorInteractionDto>>> GetBySponsorIdAsync(Guid sponsorId, BaseQueryParams request)
    {
        var list = await repository.GetBySponsorIdAsync(sponsorId, request);
        var dtos = list.Adapt<List<SponsorInteractionDto>>();
        var result = new PagedResult<SponsorInteractionDto>
            (
                dtos,
                list.TotalItems,
                list.CurrentPage,
                list.PageSize
            );
        return ApiResponse<PagedResult<SponsorInteractionDto>>.Success(200, "OK", result);
    }

    public async Task<ApiResponse<PagedResult<SponsorInteractionDto>>> GetByUserIdAsync(Guid userId, BaseQueryParams request)
    {
        var list = await repository.GetByUserIdAsync(userId, request);
        var dtos = list.Adapt<List<SponsorInteractionDto>>();
        var result = new PagedResult<SponsorInteractionDto>
            (
                dtos,
                list.TotalItems,
                list.CurrentPage,
                list.PageSize
            );
        return ApiResponse<IEnumerable<SponsorInteractionDto>>.Success(200, "OK", request);
    }
}
