using AIService_Application.DTOs;

namespace AIService_Application.Interface;

public interface IUserPlanServiceClient
{
    Task<IReadOnlyList<UserPlanDto>> GetByUserIdAsync(Guid userId, bool onlyActive = false, CancellationToken ct = default);
}
