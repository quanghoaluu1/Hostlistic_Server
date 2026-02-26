using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using Mapster;

namespace IdentityService_Application.Services;

public class SubscriptionPlanService(ISubscriptionPlanRepository repository) : ISubscriptionPlanService
{
    public async Task<ApiResponse<SubscriptionPlanDto>> CreateAsync(CreateSubscriptionPlanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            dto.DurationInMonths <= 0 ||
            dto.MaxEvents < 0 ||
            dto.CommissionRate < 0 || dto.CommissionRate > 1)
        {
            return ApiResponse<SubscriptionPlanDto>.Fail(400, "Dữ liệu gói không hợp lệ");
        }

        var entity = dto.Adapt<SubscriptionPlan>();
        entity.Id = Guid.NewGuid();
        entity.IsActive = true;

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SubscriptionPlanDto>();
        return ApiResponse<SubscriptionPlanDto>.Success(201, "Tạo gói thành công", result);
    }

    public async Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetAllAsync(bool includeInactive)
    {
        var plans = await repository.GetAllAsync(includeInactive);
        var dtos = plans.Adapt<IEnumerable<SubscriptionPlanDto>>();
        return ApiResponse<IEnumerable<SubscriptionPlanDto>>.Success(200, "OK", dtos);
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SubscriptionPlanDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<SubscriptionPlanDto>();
        return ApiResponse<SubscriptionPlanDto>.Success(200, "OK", dto);
    }

    public async Task<ApiResponse<SubscriptionPlanDto>> UpdateAsync(Guid id, UpdateSubscriptionPlanDto dto)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<SubscriptionPlanDto>.Fail(404, "Không tìm thấy");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;
        if (dto.Price.HasValue)
            entity.Price = dto.Price.Value;
        if (!string.IsNullOrWhiteSpace(dto.Description))
            entity.Description = dto.Description;
        if (dto.DurationInMonths.HasValue && dto.DurationInMonths > 0)
            entity.DurationInMonths = dto.DurationInMonths.Value;
        if (dto.MaxEvents.HasValue && dto.MaxEvents >= 0)
            entity.MaxEvents = dto.MaxEvents.Value;
        if (dto.CommissionRate.HasValue && dto.CommissionRate is >= 0 and <= 1)
            entity.CommissionRate = dto.CommissionRate.Value;
        if (dto.HasAiAccess.HasValue)
            entity.HasAiAccess = dto.HasAiAccess.Value;
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        await repository.UpdateAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<SubscriptionPlanDto>();
        return ApiResponse<SubscriptionPlanDto>.Success(200, "Cập nhật thành công", result);
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
