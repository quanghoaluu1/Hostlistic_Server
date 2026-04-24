using AIService_Domain.Entities;
using AIService_Domain.Enum;

namespace AIService_Domain.Interfaces;

public interface IAiRequestRepository
{
    Task<AiRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AiRequest>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Returns the most recent <see cref="AiRequestStatus.Completed"/> request
    /// of the given <paramref name="requestType"/> for <paramref name="eventId"/>,
    /// with its <see cref="AiRequest.GeneratedContents"/> eagerly loaded.
    /// Returns <c>null</c> when no completed request exists.
    /// </summary>
    Task<AiRequest?> GetLatestCompletedByTypeAsync(
        Guid eventId,
        AiRequestType requestType,
        CancellationToken ct = default);

    AiRequest Add(AiRequest request);
    AiRequest Update(AiRequest request);
    Task SaveChangesAsync(CancellationToken ct = default);
}
