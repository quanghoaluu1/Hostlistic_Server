namespace NotificationService_Application.Dtos;

public class EmailOtpRequest
{
 public string Email { get; set; } = string.Empty;
 public string Otp { get; set; } = string.Empty;
}