using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class SponsorTierService(ISponsorTierRepository repository, IEventRepository eventRepository) : ISponsorTierService
{
    public async Task<ApiResponse<SponsorTierDto>> CreateAsync(CreateSponsorTierDto dto)
    {
        if (dto.EventId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<SponsorTierDto>.Fail(400, "Dữ liệu tier không hợp lệ");

        var ev = await eventRepository.GetEventByIdAsync(dto.EventId);
        if (ev == null)
            return ApiResponse<SponsorTierDto>.Fail(400, "Sự kiện không tồn tại");

        var entity = new SponsorTier
        {
            Id = Guid.NewGuid(),
            EventId = dto.EventId,
            Name = dto.Name,
            Priority = dto.Priority
        };

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SponsorTierDto>();
        return ApiResponse<SponsorTierDto>.Success(201, "Tạo tier thành công", result);
    }

    public async Task<ApiResponse<SponsorTierDto>> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SponsorTierDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<SponsorTierDto>();
        return ApiResponse<SponsorTierDto>.Success(200, "OK", dto);
    }

    public async Task<ApiResponse<PagedResult<SponsorTierDto>>> GetByEventIdAsync(Guid eventId, BaseQueryParams request)
    {
        var list = await repository.GetByEventIdAsync(eventId, request);
        var dtos = list.Adapt<List<SponsorTierDto>>();
        var result = new PagedResult<SponsorTierDto>
        (
            dtos,
            list.TotalItems,
            list.PageSize,
            list.CurrentPage
        );
        return ApiResponse<PagedResult<SponsorTierDto>>.Success(200, "OK", result);
    }

    public async Task<ApiResponse<SponsorTierDto>> UpdateAsync(Guid id, UpdateSponsorTierDto dto)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SponsorTierDto>.Fail(404, "Không tìm thấy");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;
        if (dto.Priority.HasValue)
            entity.Priority = dto.Priority.Value;

        await repository.UpdateAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SponsorTierDto>();
        return ApiResponse<SponsorTierDto>.Success(200, "Cập nhật thành công", result);
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
