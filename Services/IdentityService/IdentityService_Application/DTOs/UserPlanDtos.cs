namespace IdentityService_Application.DTOs
{
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
     
    public class CreateUserPlanDto
    {
        public Guid UserId { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        // public DateTime StartDate { get; set; }
        // public DateTime? EndDate { get; set; }
    }

    public class UpdateUserPlanDto
    {
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
