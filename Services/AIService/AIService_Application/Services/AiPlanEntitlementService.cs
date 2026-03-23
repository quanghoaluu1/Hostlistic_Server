using AIService_Application.DTOs;
using AIService_Application.Interface;

namespace AIService_Application.Services;

public class AiPlanEntitlementService(IUserPlanServiceClient userPlanServiceClient) : IAiPlanEntitlementService
{
    public async Task<AiEntitlementResult> EnsureCanUseAiAsync(Guid userId, CancellationToken ct = default)
    {
        var plans = await userPlanServiceClient.GetByUserIdAsync(userId, onlyActive: true, ct);
        var active = plans
            .Where(p => p.SubscriptionPlan is not null)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefault();

        if (active?.SubscriptionPlan is null)
        {
            return new AiEntitlementResult(false, 403,
                "No active subscription plan found. AI features require an active plan.");
        }

        if (!active.SubscriptionPlan.HasAiAccess)
        {
            return new AiEntitlementResult(false, 403,
                "Your current subscription plan does not include AI features. Please upgrade your plan.");
        }

        return new AiEntitlementResult(true, 200, string.Empty);
    }
}
