using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SponsorRepository(EventServiceDbContext dbContext) : ISponsorRepository
{
    public async Task<IReadOnlyList<Sponsor>> GetByEventIdAsync(Guid eventId)
    {
        return await dbContext.Sponsors
            .Include(s => s.Tier)
            .Where(x => x.EventId == eventId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Sponsor>> GetByTierIdAsync(Guid tierId)
    {
        return await dbContext.Sponsors
            .Where(x => x.TierId == tierId)
            .ToListAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(Guid id)
    {
        return await dbContext.Sponsors
            .Include(s => s.Tier)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Sponsor entity)
    {
        entity.Id = Guid.NewGuid();
        await dbContext.Sponsors.AddAsync(entity);
    }

    public Task UpdateAsync(Sponsor entity)
    {
        dbContext.Sponsors.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbContext.Sponsors.FindAsync(id);
        if (entity == null) return false;
        dbContext.Sponsors.Remove(entity);
        return true;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
