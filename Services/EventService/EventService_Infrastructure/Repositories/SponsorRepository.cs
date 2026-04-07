using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SponsorRepository(EventServiceDbContext dbContext) : ISponsorRepository
{
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

    public async Task<IEnumerable<Sponsor>> GetByEventIdAsync(Guid eventId)
    {
        return await dbContext.Sponsors
            .Where(s => s.EventId == eventId)
            .Include(s => s.Tier)
            .ToListAsync();
    }

    public async Task<Sponsor?> GetByIdWithInteractionsAsync(Guid sponsorId)
    {
        return await dbContext.Sponsors
            .Include(s => s.SponsorInteractions)
            .FirstOrDefaultAsync(s => s.Id == sponsorId);
    }

    public async Task<bool> ExistsAsync(Guid sponsorId)
    {
        return await dbContext.Sponsors.AnyAsync(s => s.Id == sponsorId);
    }
}
