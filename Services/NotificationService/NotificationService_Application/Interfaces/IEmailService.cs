namespace NotificationService_Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
}