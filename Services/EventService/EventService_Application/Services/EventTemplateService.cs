using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class EventTemplateService(IEventTemplateRepository repository) : IEventTemplateService
{
    public async Task<ApiResponse<EventTemplateDto>> CreateAsync(CreateEventTemplateDto dto)
    {
        if (dto.CreatedBy == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name))
            return ApiResponse<EventTemplateDto>.Fail(400, "Dữ liệu template không hợp lệ");

        var entity = new EventTemplate
        {
            Id = Guid.NewGuid(),
            CreatedBy = dto.CreatedBy,
            Name = dto.Name,
            Config = dto.Config.Adapt<EventTemplateConfig>()
        };

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<EventTemplateDto>();
        return ApiResponse<EventTemplateDto>.Success(201, "Tạo template thành công", result);
    }

    public async Task<ApiResponse<EventTemplateDto>> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<EventTemplateDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<EventTemplateDto>();
        return ApiResponse<EventTemplateDto>.Success(200, "OK", dto);
    }

    public async Task<ApiResponse<PagedResult<EventTemplateDto>>> GetByCreatorAsync(Guid createdBy, BaseQueryParams request)
    {
        var list = await repository.GetByCreatorAsync(createdBy, request);
        var dtos = list.Adapt<List<EventTemplateDto>>();
        var result = new PagedResult<EventTemplateDto>(dtos, list.TotalItems, list.CurrentPage, list.PageSize);
        return ApiResponse<PagedResult<EventTemplateDto>>.Success(200, "OK", result);
    }

    public async Task<ApiResponse<EventTemplateDto>> UpdateAsync(Guid id, UpdateEventTemplateDto dto)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<EventTemplateDto>.Fail(404, "Không tìm thấy");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;
        if (dto.Config is not null)
            entity.Config = dto.Config.Adapt<EventTemplateConfig>();

        await repository.UpdateAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<EventTemplateDto>();
        return ApiResponse<EventTemplateDto>.Success(200, "Cập nhật thành công", result);
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
