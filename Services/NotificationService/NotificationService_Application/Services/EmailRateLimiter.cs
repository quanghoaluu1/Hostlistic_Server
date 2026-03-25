using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService_Application.Interfaces;
using StackExchange.Redis;

namespace NotificationService_Application.Services;

public class EmailRateLimiter(IConnectionMultiplexer redis, IConfiguration configuration, ILogger<EmailRateLimiter> logger) : IEmailRateLimiter
{
    private const string KeyPrefix = "email:daily:";
    private int DailyLimit => configuration.GetValue("Email:DailyLimit", 100);
    private readonly IDatabase _redis = redis.GetDatabase();

    public async Task<int> TryConsumeAsync(int requested)
    {
        var key = GetTodayKey();
        var script = @"
              local current = tonumber(redis.call('GET', KEYS[1]) or '0')
            local limit = tonumber(ARGV[1])
            local requested = tonumber(ARGV[2])
            local available = limit - current
            if available <= 0 then
                return 0
            end
            local consume = math.min(requested, available)
            redis.call('INCRBY', KEYS[1], consume)
            redis.call('EXPIRE', KEYS[1], 90000)  -- 25 hours TTL
            return consume
";
        var consumed =  (int)await _redis.ScriptEvaluateAsync(
            script,
            keys: [key],
            values: [DailyLimit, requested]);
        if (consumed < requested)
        {
            logger.LogWarning(
                "Rate limit: requested {Requested}, consumed {Consumed}, daily limit {Limit}",
                requested, consumed, DailyLimit);
        }
 
        return consumed;
    }

    public async Task<int> GetRemainingQuotaAsync()
    {
        var current = (int)await _redis.StringGetAsync(GetTodayKey());
        return Math.Max(0, DailyLimit - current);
    }

    public async Task<int> GetCurrentUsageAsync()
    {
        return (int)await _redis.StringGetAsync(GetTodayKey());
    }
    
    private static RedisKey GetTodayKey()
        => new($"{KeyPrefix}{DateTime.UtcNow:yyyy-MM-dd}");
}