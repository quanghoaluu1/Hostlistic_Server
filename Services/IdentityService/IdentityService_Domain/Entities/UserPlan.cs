using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService_Domain.Entities;

public class UserPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties to parent
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    
    [ForeignKey("SubscriptionPlanId")]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}