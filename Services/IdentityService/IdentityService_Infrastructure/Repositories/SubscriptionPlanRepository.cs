using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using IdentityService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Domain.Repositories;

public class SubscriptionPlanRepository(IdentityServiceDbContext dbContext) : ISubscriptionPlanRepository
{
    public async Task<IReadOnlyList<SubscriptionPlan>> GetAllAsync(bool includeInactive)
    {
        var query = dbContext.SubscriptionPlans.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        return await query.ToListAsync();
    }

    public async Task<SubscriptionPlan?> GetByIdAsync(Guid id)
    {
        return await dbContext.SubscriptionPlans.FindAsync(id);
    }

    public async Task AddAsync(SubscriptionPlan entity)
    {
        await dbContext.SubscriptionPlans.AddAsync(entity);
    }

    public Task UpdateAsync(SubscriptionPlan entity)
    {
        dbContext.SubscriptionPlans.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbContext.SubscriptionPlans.FindAsync(id);
        if (entity == null) return false;
        dbContext.SubscriptionPlans.Remove(entity);
        return true;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
