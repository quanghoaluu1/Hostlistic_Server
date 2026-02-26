using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using Mapster;

namespace IdentityService_Application.Services;

public class UserPlanService(IUserPlanRepository userPlanRepository, ISubscriptionPlanRepository subscriptionPlanRepository) : IUserPlanService
{
    public async Task<ApiResponse<UserPlanDto>> CreateAsync(CreateUserPlanDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.SubscriptionPlanId == Guid.Empty)
            return ApiResponse<UserPlanDto>.Fail(400, "Thiếu UserId hoặc SubscriptionPlanId");

        if (dto.StartDate == default)
            dto.StartDate = DateTime.UtcNow;

        var plan = await subscriptionPlanRepository.GetByIdAsync(dto.SubscriptionPlanId);
        if (plan == null || !plan.IsActive)
            return ApiResponse<UserPlanDto>.Fail(400, "Gói không tồn tại hoặc không hoạt động");

        var existingActives = await userPlanRepository.GetByUserIdAsync(dto.UserId, true);
        if (existingActives.Any(x => x.SubscriptionPlanId == dto.SubscriptionPlanId))
            return ApiResponse<UserPlanDto>.Fail(400, "User đã có gói này đang hoạt động");

        var entity = new UserPlan
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            SubscriptionPlanId = dto.SubscriptionPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = true
        };

        await userPlanRepository.AddAsync(entity);
        await userPlanRepository.SaveChangesAsync();

        var result = entity.Adapt<UserPlanDto>();
        return ApiResponse<UserPlanDto>.Success(201, "Gán gói cho user thành công", result);
    }

    public async Task<ApiResponse<UserPlanDto>> GetByIdAsync(Guid id)
    {
        var entity = await userPlanRepository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<UserPlanDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<UserPlanDto>();
        return ApiResponse<UserPlanDto>.Success(200, "OK", dto);
    }

    public async Task<ApiResponse<IEnumerable<UserPlanDto>>> GetByUserIdAsync(Guid userId, bool onlyActive)
    {
        var list = await userPlanRepository.GetByUserIdAsync(userId, onlyActive);
        var dtos = list.Adapt<IEnumerable<UserPlanDto>>();
        return ApiResponse<IEnumerable<UserPlanDto>>.Success(200, "OK", dtos);
    }

    public async Task<ApiResponse<UserPlanDto>> UpdateAsync(Guid id, UpdateUserPlanDto dto)
    {
        var entity = await userPlanRepository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<UserPlanDto>.Fail(404, "Không tìm thấy");

        if (dto.EndDate.HasValue)
            entity.EndDate = dto.EndDate;
        if (dto.IsActive.HasValue)
            entity.IsActive = dto.IsActive.Value;

        await userPlanRepository.UpdateAsync(entity);
        await userPlanRepository.SaveChangesAsync();

        var result = entity.Adapt<UserPlanDto>();
        return ApiResponse<UserPlanDto>.Success(200, "Cập nhật thành công", result);
    }

    public async Task<ApiResponse<bool>> CancelAsync(Guid id)
    {
        var entity = await userPlanRepository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail(404, "Không tìm thấy");

        entity.IsActive = false;
        entity.EndDate ??= DateTime.UtcNow;

        await userPlanRepository.UpdateAsync(entity);
        await userPlanRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Huỷ gói thành công", true);
    }
}
