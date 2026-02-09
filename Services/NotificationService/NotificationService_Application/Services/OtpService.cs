using NotificationService_Application.Interfaces;
using StackExchange.Redis;

namespace NotificationService_Application.Services;

public class OtpService(IConnectionMultiplexer redis) : IOtpService
{
    private readonly IDatabase _redisDb = redis.GetDatabase();
    private readonly TimeSpan _expiry = TimeSpan.FromMinutes(5);

    public async Task<string> GenerateOtpAsync(string email)
    {
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var key = $"otp:{email}";
        await _redisDb.StringSetAsync(key, otp, _expiry);
        return otp;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
        var key = $"otp:{email}";
        var storedOtp = await _redisDb.StringGetAsync(key);
        if (storedOtp.HasValue && storedOtp.ToString() == otp)
        {
            await _redisDb.KeyDeleteAsync(key);
            return true;
        }
        return false;
    }
}