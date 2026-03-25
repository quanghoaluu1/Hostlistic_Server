using Common;
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

    public async Task<PagedResult<SponsorInteraction>> GetBySponsorIdAsync(Guid sponsorId, BaseQueryParams request)
    {
        var query = dbContext.SponsorInteractions
            .Where(x => x.SponsorId == sponsorId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<PagedResult<SponsorInteraction>> GetByUserIdAsync(Guid userId, BaseQueryParams request)
    {
        var query = dbContext.SponsorInteractions
            .Where(x => x.UserId == userId)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
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
