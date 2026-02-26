using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SponsorInteractionRepository(EventServiceDbContext dbContext) : ISponsorInteractionRepository
{
    public async Task<SponsorInteraction?> GetByIdAsync(Guid id)
    {
        return await dbContext.SponsorInteractions
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<SponsorInteraction>> GetBySponsorIdAsync(Guid sponsorId)
    {
        return await dbContext.SponsorInteractions
            .Where(x => x.SponsorId == sponsorId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SponsorInteraction>> GetByUserIdAsync(Guid userId)
    {
        return await dbContext.SponsorInteractions
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(SponsorInteraction entity)
    {
        entity.Id = Guid.NewGuid();
        await dbContext.SponsorInteractions.AddAsync(entity);
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
