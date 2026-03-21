using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using NotificationService_Application.Interfaces;
using StackExchange.Redis;

namespace IdentityService_Application.Services;

public class OtpService(IConnectionMultiplexer redis, INotificationServiceClient notificationServiceClient) : IOtpService
{
    private readonly IDatabase _redisDb = redis.GetDatabase();
    private readonly TimeSpan _expiry = TimeSpan.FromMinutes(5);

    public async Task<ApiResponse<string>> SendOtpAsync(SendOtpRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return ApiResponse<string>.Fail(400, "Email is required");
            }
            var otp = await GenerateOtpAsync(request.Email);
            await notificationServiceClient.SendOtpEmailAsync(request.Email, otp);
            return ApiResponse<string>.Success(200, "Otp sent successfully", null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ApiResponse<string>.Fail(500, "Failed to send otp");
        }
    }
    
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