using Common;

namespace IdentityService_Domain.Entities;

public class SubscriptionPlan : BaseClass
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationInMonths { get; set; }
    public int MaxEvents { get; set; }
    public float CommissionRate { get; set; }
    public bool HasAiAccess { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<UserPlan> UserPlans { get; set; } = new List<UserPlan>();
}