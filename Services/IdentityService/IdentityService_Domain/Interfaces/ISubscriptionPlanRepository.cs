using IdentityService_Domain.Entities;

namespace IdentityService_Domain.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(bool includeInactive);
    Task<SubscriptionPlan?> GetByIdAsync(Guid id);
    Task AddAsync(SubscriptionPlan entity);
    Task UpdateAsync(SubscriptionPlan entity);
    Task<bool> DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
