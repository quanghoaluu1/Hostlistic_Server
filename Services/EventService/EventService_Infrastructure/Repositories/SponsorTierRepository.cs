using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SponsorTierRepository(EventServiceDbContext dbContext) : ISponsorTierRepository
{
    public async Task<IReadOnlyList<SponsorTier>> GetByEventIdAsync(Guid eventId)
    {
        return await dbContext.SponsorTiers
            .Where(x => x.EventId == eventId)
            .ToListAsync();
    }

    public async Task<SponsorTier?> GetByIdAsync(Guid id)
    {
        return await dbContext.SponsorTiers.FindAsync(id);
    }

    public async Task AddAsync(SponsorTier entity)
    {
        entity.Id = Guid.NewGuid();
        await dbContext.SponsorTiers.AddAsync(entity);
    }

    public Task UpdateAsync(SponsorTier entity)
    {
        dbContext.SponsorTiers.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbContext.SponsorTiers.FindAsync(id);
        if (entity == null) return false;
        dbContext.SponsorTiers.Remove(entity);
        return true;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
