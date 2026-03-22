namespace IdentityService_Application.DTOs
{
    public class SubscriptionPlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DurationInMonths { get; set; }
        public int MaxEvents { get; set; }
    public int MaxAttendeesPerEvent { get; set; }
        public float CommissionRate { get; set; }
        public bool HasAiAccess { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSubscriptionPlanDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DurationInMonths { get; set; }
        public int MaxEvents { get; set; }
    public int MaxAttendeesPerEvent { get; set; }
        public float CommissionRate { get; set; }
        public bool HasAiAccess { get; set; }
    }

    public class UpdateSubscriptionPlanDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public int? DurationInMonths { get; set; }
        public int? MaxEvents { get; set; }
    public int? MaxAttendeesPerEvent { get; set; }
        public float? CommissionRate { get; set; }
        public bool? HasAiAccess { get; set; }
        public bool? IsActive { get; set; }
    }
}
