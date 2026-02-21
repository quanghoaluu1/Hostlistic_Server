using Common;
using IdentityService_Application.DTOs;

namespace NotificationService_Application.Interfaces;

public interface IOtpService
{
    Task<ApiResponse<string>> SendOtpAsync(SendOtpRequest request);
    Task<string> GenerateOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string email, string otp);
}