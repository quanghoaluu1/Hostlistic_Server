namespace NotificationService_Application.Interfaces;

public interface IEmailRateLimiter
{
    /// <summary>Try to consume N quota slots. Returns actual number consumed (may be less if near limit).</summary>
    Task<int> TryConsumeAsync(int requested);
 
    /// <summary>Get remaining quota for today.</summary>
    Task<int> GetRemainingQuotaAsync();
 
    /// <summary>Get current usage count for today.</summary>
    Task<int> GetCurrentUsageAsync();
}