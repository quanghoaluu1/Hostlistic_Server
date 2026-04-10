namespace EventService_Test.Helpers.TestDataBuilders;

public class UserPlanBuilder
{
    public static UserPlanLookupResult ActivePlanResult(
        int maxEvents = 10,
        int maxAttendeesPerEvent = 100
    )
    {
        return new UserPlanLookupResult()
        {
            IsSuccess = true,
            StatusCode = 200,
            Message = "Success",
            Plans =
            [
                new UserPlanDto()
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    SubscriptionPlanId = Guid.NewGuid(),
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    SubscriptionPlan = new SubscriptionPlanDto()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Pro Plan",
                        Description = "Pro Plan Description Testing",
                        MaxEvents = maxEvents,
                        MaxAttendeesPerEvent = maxAttendeesPerEvent,
                        HasAiAccess = true,
                        IsActive = true,
                        Price = 1000,
                        DurationInDays = 30,
                        CommissionRate = 0.05f
                    }
                }
            ]
        };
    }

    public static UserPlanLookupResult FailedPlanResult(string message = "Service unavailable")
    {
        return new UserPlanLookupResult()
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = message,
            Plans = []
        };
    }
    
    public static UserPlanLookupResult NoPlanResult()
    {
        return new UserPlanLookupResult
        {
            IsSuccess = true,
            StatusCode = 200,
            Message = "OK",
            Plans = [] // No active plans
        };
    }
}