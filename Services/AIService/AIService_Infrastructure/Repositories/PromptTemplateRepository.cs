using AIService_Domain.Entities;
using AIService_Domain.Enum;
using AIService_Domain.Interfaces;
using AIService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIService_Infrastructure.Repositories;

public class PromptTemplateRepository(AIServiceDbContext dbContext) : IPromptTemplateRepository
{
    public async Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<PromptTemplate?> GetByKeyAsync(PromptTemplateKey key, CancellationToken ct = default)
    {
        return await dbContext.PromptTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == key, ct);
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.PromptTemplates
            .OrderBy(t => t.Category)
            .ThenBy(t => t.DisplayName)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PromptTemplate>> GetByCategoryAsync(PromptCategory category, CancellationToken ct = default)
    {
        return await dbContext.PromptTemplates
            .Where(t => t.Category == category)
            .OrderBy(t => t.DisplayName)
            .ToListAsync(ct);
    }

    public PromptTemplate Add(PromptTemplate template)
    {
        return dbContext.PromptTemplates.Add(template).Entity;
    }

    public PromptTemplate Update(PromptTemplate template)
    {
        return dbContext.PromptTemplates.Update(template).Entity;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
