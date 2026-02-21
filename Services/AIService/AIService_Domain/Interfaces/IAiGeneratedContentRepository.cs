using AIService_Domain.Entities;

namespace AIService_Domain.Interfaces;

public interface IAiGeneratedContentRepository
{
    Task<AiGeneratedContent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AiGeneratedContent>> GetByRequestIdAsync(Guid requestId, CancellationToken ct = default);
    AiGeneratedContent Add(AiGeneratedContent content);
    AiGeneratedContent Update(AiGeneratedContent content);
    Task SaveChangesAsync(CancellationToken ct = default);
}
