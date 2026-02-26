using Common;
using IdentityService_Application.DTOs;

namespace IdentityService_Application.Interfaces;

public interface ISubscriptionPlanService
{
    Task<ApiResponse<SubscriptionPlanDto>> CreateAsync(CreateSubscriptionPlanDto dto);
    Task<ApiResponse<IEnumerable<SubscriptionPlanDto>>> GetAllAsync(bool includeInactive);
    Task<ApiResponse<SubscriptionPlanDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<SubscriptionPlanDto>> UpdateAsync(Guid id, UpdateSubscriptionPlanDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
