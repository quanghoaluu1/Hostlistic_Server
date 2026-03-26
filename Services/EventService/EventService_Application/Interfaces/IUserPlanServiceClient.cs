using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IUserPlanServiceClient
{
    Task<UserPlanLookupResult> GetByUserIdAsync(Guid userId, bool onlyActive = false);
}
