namespace AIService_Application.Interface;

public sealed record AiEntitlementResult(bool Success, int StatusCode, string Message);

public interface IAiPlanEntitlementService
{
    Task<AiEntitlementResult> EnsureCanUseAiAsync(Guid userId, CancellationToken ct = default);
}
