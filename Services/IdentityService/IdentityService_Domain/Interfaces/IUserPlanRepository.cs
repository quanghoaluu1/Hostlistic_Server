using IdentityService_Domain.Entities;

namespace IdentityService_Domain.Interfaces;

public interface IUserPlanRepository
{
    Task<UserPlan?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<UserPlan>> GetByUserIdAsync(Guid userId, bool onlyActive);
    Task AddAsync(UserPlan entity);
    Task UpdateAsync(UserPlan entity);
    Task SaveChangesAsync();
}
