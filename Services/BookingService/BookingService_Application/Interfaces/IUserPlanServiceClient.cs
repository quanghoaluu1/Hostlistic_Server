using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IUserPlanServiceClient
{
    Task<UserPlanDto?> GetByIdAsync(Guid userPlanId);
    Task<IEnumerable<UserPlanDto>> GetByUserIdAsync(Guid userId, bool onlyActive = false);
}
