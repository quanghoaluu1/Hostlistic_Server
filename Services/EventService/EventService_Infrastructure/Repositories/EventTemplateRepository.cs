using Common;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventTemplateRepository(EventServiceDbContext dbContext) : IEventTemplateRepository
{
    public async Task<IReadOnlyList<EventTemplate>> GetByCreatorAsync(Guid createdBy)
    {
        return await dbContext.EventTemplates
            .Where(x => x.CreatedBy == createdBy)
            .ToListAsync();
    }

    public async Task<PagedResult<EventTemplate>> GetEventTemplateByCreatorAsync(Guid createdBy, int pageNumber, int pageSize, string? sortBy = null)
    {
        var query = dbContext.EventTemplates
            .Where(x => x.CreatedBy == createdBy)
            .AsQueryable();
        query = query.ApplySorting(sortBy);
        return await query.ToPagedResultAsync(pageNumber, pageSize);
    }

    public async Task<EventTemplate?> GetByIdAsync(Guid id)
    {
        return await dbContext.EventTemplates
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(EventTemplate entity)
    {
        entity.Id = Guid.NewGuid();
        await dbContext.EventTemplates.AddAsync(entity);
    }

    public Task UpdateAsync(EventTemplate entity)
    {
        dbContext.EventTemplates.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await dbContext.EventTemplates.FindAsync(id);
        if (entity == null) return false;
        dbContext.EventTemplates.Remove(entity);
        return true;
    }

    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}
