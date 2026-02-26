using Common;
using IdentityService_Application.DTOs;

namespace IdentityService_Application.Interfaces;

public interface IUserPlanService
{
    Task<ApiResponse<UserPlanDto>> CreateAsync(CreateUserPlanDto dto);
    Task<ApiResponse<UserPlanDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<UserPlanDto>>> GetByUserIdAsync(Guid userId, bool onlyActive);
    Task<ApiResponse<UserPlanDto>> UpdateAsync(Guid id, UpdateUserPlanDto dto);
    Task<ApiResponse<bool>> CancelAsync(Guid id);
}
