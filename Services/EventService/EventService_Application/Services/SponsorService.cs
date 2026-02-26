using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class SponsorService(ISponsorRepository repository, ISponsorTierRepository sponsorTierRepository, IEventRepository eventRepository) : ISponsorService
{
    public async Task<ApiResponse<SponsorDto>> CreateAsync(CreateSponsorDto dto)
    {
        if (dto.EventId == Guid.Empty || dto.TierId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<SponsorDto>.Fail(400, "Dữ liệu sponsor không hợp lệ");

        var ev = await eventRepository.GetEventByIdAsync(dto.EventId);
        if (ev == null)
            return ApiResponse<SponsorDto>.Fail(400, "Sự kiện không tồn tại");

        var tier = await sponsorTierRepository.GetByIdAsync(dto.TierId);
        if (tier == null || tier.EventId != dto.EventId)
            return ApiResponse<SponsorDto>.Fail(400, "Tier không hợp lệ cho sự kiện này");

        var entity = new Sponsor
        {
            Id = Guid.NewGuid(),
            EventId = dto.EventId,
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            Description = dto.Description,
            WebsiteUrl = dto.WebsiteUrl,
            TierId = dto.TierId
        };

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SponsorDto>();
        return ApiResponse<SponsorDto>.Success(201, "Tạo sponsor thành công", result);
    }

    public async Task<ApiResponse<SponsorDto>> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SponsorDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<SponsorDto>();
        return ApiResponse<SponsorDto>.Success(200, "OK", dto);
    }

    public async Task<ApiResponse<IEnumerable<SponsorDto>>> GetByEventIdAsync(Guid eventId)
    {
        var list = await repository.GetByEventIdAsync(eventId);
        var dtos = list.Adapt<IEnumerable<SponsorDto>>();
        return ApiResponse<IEnumerable<SponsorDto>>.Success(200, "OK", dtos);
    }

    public async Task<ApiResponse<IEnumerable<SponsorDto>>> GetByTierIdAsync(Guid tierId)
    {
        var list = await repository.GetByTierIdAsync(tierId);
        var dtos = list.Adapt<IEnumerable<SponsorDto>>();
        return ApiResponse<IEnumerable<SponsorDto>>.Success(200, "OK", dtos);
    }

    public async Task<ApiResponse<SponsorDto>> UpdateAsync(Guid id, UpdateSponsorDto dto)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SponsorDto>.Fail(404, "Không tìm thấy");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.LogoUrl))
            entity.LogoUrl = dto.LogoUrl;
        if (dto.Description is not null)
            entity.Description = dto.Description;
        if (dto.WebsiteUrl is not null)
            entity.WebsiteUrl = dto.WebsiteUrl;
        if (dto.TierId.HasValue)
        {
            var tier = await sponsorTierRepository.GetByIdAsync(dto.TierId.Value);
            if (tier == null || tier.EventId != entity.EventId)
                return ApiResponse<SponsorDto>.Fail(400, "Tier không hợp lệ cho sự kiện này");
            entity.TierId = dto.TierId.Value;
        }

        await repository.UpdateAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SponsorDto>();
        return ApiResponse<SponsorDto>.Success(200, "Cập nhật thành công", result);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var ok = await repository.DeleteAsync(id);
        if (!ok)
            return ApiResponse<bool>.Fail(404, "Không tìm thấy");

        await repository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Xoá thành công", true);
    }
}
