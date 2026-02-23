using AIService_Domain.Entities;
using AIService_Domain.Interfaces;
using AIService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIService_Infrastructure.Repositories;

public class AiRequestRepository(AIServiceDbContext dbContext) : IAiRequestRepository
{
    public async Task<AiRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.AiRequests
            .Include(r => r.GeneratedContents)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<AiRequest>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default)
    {
        return await dbContext.AiRequests
            .Where(r => r.EventId == eventId)
            .Include(r => r.GeneratedContents)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public AiRequest Add(AiRequest request)
    {
        return dbContext.AiRequests.Add(request).Entity;
    }

    public AiRequest Update(AiRequest request)
    {
        return dbContext.AiRequests.Update(request).Entity;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
