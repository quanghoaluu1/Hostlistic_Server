using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using IdentityService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Domain.Repositories;

public class UserPlanRepository(IdentityServiceDbContext dbContext) : IUserPlanRepository
{
    public async Task<UserPlan?> GetByIdAsync(Guid id)
    {
        return await dbContext.UserPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<UserPlan>> GetByUserIdAsync(Guid userId, bool onlyActive)
    {
        var query = dbContext.UserPlans
            .AsNoTracking()
            .Include(x => x.SubscriptionPlan)
            .Where(x => x.UserId == userId);

        if (onlyActive)
        {
            query = query.Where(x => x.IsActive == true);
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(UserPlan entity)
    {
        await dbContext.UserPlans.AddAsync(entity);
    }

    public Task UpdateAsync(UserPlan entity)
    {
        dbContext.UserPlans.Update(entity);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
