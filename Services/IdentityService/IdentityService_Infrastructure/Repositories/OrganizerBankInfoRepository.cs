using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using IdentityService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Domain.Repositories;

public class OrganizerBankInfoRepository(IdentityServiceDbContext dbContext) : IOrganizerBankInfoRepository
{
    public async Task<OrganizerBankInfo?> GetByIdAsync(Guid id)
    {
        return await dbContext.OrganizerBankInfos.FindAsync(id);
    }

    public async Task<IReadOnlyList<OrganizerBankInfo>> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.OrganizerBankInfos
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<OrganizerBankInfo>> GetByOrganizationIdAsync(Guid organizationId)
    {
        return await dbContext.OrganizerBankInfos
            .Where(x => x.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task AddAsync(OrganizerBankInfo entity)
    {
        await dbContext.OrganizerBankInfos.AddAsync(entity);
    }

    public Task UpdateAsync(OrganizerBankInfo entity)
    {
        dbContext.OrganizerBankInfos.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbContext.OrganizerBankInfos.FindAsync(id);
        if (entity == null) return false;
        dbContext.OrganizerBankInfos.Remove(entity);
        return true;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
