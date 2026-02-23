using AIService_Domain.Entities;
using AIService_Domain.Enum;

namespace AIService_Domain.Interfaces;

public interface IAiRequestRepository
{
    Task<AiRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AiRequest>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
    AiRequest Add(AiRequest request);
    AiRequest Update(AiRequest request);
    Task SaveChangesAsync(CancellationToken ct = default);
}
