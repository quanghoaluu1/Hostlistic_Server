using NotificationService_Application.Interfaces;
using Resend;

namespace NotificationService_Application.Services;

public class EmailService(IResend resend) : IEmailService
{
    public async Task SendOtpEmailAsync(string email, string otp)
    {
        var message = new EmailMessage()
        {
            From = "Hostlistic <noreply@hostlistic.tech>",
            To = email,
            Subject = $"[Hostlistic] Your OTP Code",
            HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                        <h2 style='color: #2c3e50;'>Xác thực tài khoản</h2>
                        <p>Xin chào,</p>
                        <p>Mã xác thực (OTP) để đăng nhập vào <strong>Hostlistic</strong> của bạn là:</p>
                        <h1 style='color: #e74c3c; letter-spacing: 5px;'>{otp}</h1>
                        <p>Mã này sẽ hết hạn sau <strong>5 phút</strong>.</p>
                        <p style='font-size: 12px; color: #7f8c8d;'>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                    </div>"
        };
        await resend.EmailSendAsync(message);
    }
}