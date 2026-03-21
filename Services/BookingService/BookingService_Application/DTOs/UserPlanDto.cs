namespace BookingService_Application.DTOs;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public int MaxEvents { get; set; }
    public float CommissionRate { get; set; }
    public bool HasAiAccess { get; set; }
    public bool IsActive { get; set; }
}

public class UserPlanDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public SubscriptionPlanDto? SubscriptionPlan { get; set; }
}

public class CreateUserPlanRequest
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
}

public class PurchaseSubscriptionWithWalletRequest
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
}

public class PurchaseSubscriptionWithWalletResponse
{
    public Guid UserPlanId { get; set; }
    public Guid WalletId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public string SubscriptionPlanName { get; set; } = string.Empty;
    public decimal ChargedAmount { get; set; }
    public decimal WalletBalanceAfter { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
