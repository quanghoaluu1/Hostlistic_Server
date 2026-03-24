namespace IdentityService_Application.Interfaces;

public interface INotificationServiceClient
{
    Task SendOtpEmailAsync(string email, string otp);
}
