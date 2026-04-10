using Common;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class EventTemplateRepository(EventServiceDbContext dbContext) : IEventTemplateRepository
{
    public async Task<PagedResult<EventTemplate>> GetByCreatorAsync(Guid createdBy, BaseQueryParams request)
    {
        var query = dbContext.EventTemplates
            .Where(x => x.CreatedBy == createdBy)
            .AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
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
