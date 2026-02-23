using AIService_Domain.Entities;
using AIService_Domain.Interfaces;
using AIService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIService_Infrastructure.Repositories;

public class AiGeneratedContentRepository(AIServiceDbContext dbContext) : IAiGeneratedContentRepository
{
    public async Task<AiGeneratedContent?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.AiGeneratedContents
            .Include(c => c.Request)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<AiGeneratedContent>> GetByRequestIdAsync(Guid requestId, CancellationToken ct = default)
    {
        return await dbContext.AiGeneratedContents
            .Where(c => c.RequestId == requestId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public AiGeneratedContent Add(AiGeneratedContent content)
    {
        return dbContext.AiGeneratedContents.Add(content).Entity;
    }

    public AiGeneratedContent Update(AiGeneratedContent content)
    {
        return dbContext.AiGeneratedContents.Update(content).Entity;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
