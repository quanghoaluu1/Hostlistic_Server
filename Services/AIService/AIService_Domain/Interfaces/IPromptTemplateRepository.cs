using AIService_Domain.Entities;
using AIService_Domain.Enum;

namespace AIService_Domain.Interfaces;

public interface IPromptTemplateRepository
{
    Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PromptTemplate?> GetByKeyAsync(PromptTemplateKey key, CancellationToken ct = default);
    Task<IReadOnlyList<PromptTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PromptTemplate>> GetByCategoryAsync(PromptCategory category, CancellationToken ct = default);
    PromptTemplate Add(PromptTemplate template);
    PromptTemplate Update(PromptTemplate template);
    Task SaveChangesAsync(CancellationToken ct = default);
}
