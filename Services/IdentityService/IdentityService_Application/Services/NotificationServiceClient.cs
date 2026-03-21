using System.Net.Http.Json;
using IdentityService_Application.Interfaces;

namespace IdentityService_Application.Services;

public class NotificationServiceClient(IHttpClientFactory httpClientFactory) : INotificationServiceClient
{
    public async Task SendOtpEmailAsync(string email, string otp)
    {
        var client = httpClientFactory.CreateClient("NotificationService");
        var request = new { Email = email, Otp = otp };
        await client.PostAsJsonAsync("api/Email/send-email-otp", request);
    }
}
