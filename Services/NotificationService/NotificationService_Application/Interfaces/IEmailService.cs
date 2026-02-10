using NotificationService_Application.Dtos;

namespace NotificationService_Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(EmailOtpRequest request);
}