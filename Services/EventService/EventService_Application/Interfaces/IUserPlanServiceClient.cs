using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IUserPlanServiceClient
{
    Task<IEnumerable<UserPlanDto>> GetByUserIdAsync(Guid userId, bool onlyActive = false);
}
