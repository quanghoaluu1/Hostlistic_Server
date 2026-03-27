using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;
using Resend;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace NotificationService_Application.Services;

public class EmailService(IResend resend, IConfiguration configuration) : IEmailService
{
    public async Task SendOtpEmailAsync(EmailOtpRequest request)
    {
        var message = new EmailMessage()
        {
            From = "Hostlistic <noreply@hostlistic.tech>",
            To = request.Email,
            Subject = $"[Hostlistic] Your OTP Code",
            HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                        <h2 style='color: #2c3e50;'>Xác thực tài khoản</h2>
                        <p>Xin chào,</p>
                        <p>Mã xác thực (OTP) để đăng nhập vào <strong>Hostlistic</strong> của bạn là:</p>
                        <h1 style='color: #e74c3c; letter-spacing: 5px;'>{request.Otp}</h1>
                        <p>Mã này sẽ hết hạn sau <strong>5 phút</strong>.</p>
                        <p style='font-size: 12px; color: #7f8c8d;'>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                    </div>"
        };
        await resend.EmailSendAsync(message);
    }

    public async Task SendTicketPurchaseConfirmationAsync(TicketPurchaseEmailRequest request)
    {
        var ticketsHtml = BuildTicketsHtml(request.Tickets);

        var message = new EmailMessage()
        {
            From = "Hostlistic <noreply@hostlistic.tech>",
            To = request.Email,
            Subject = $"[Hostlistic] Xác nhận mua vé - {request.EventName}",
            HtmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Xác nhận mua vé</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 0;'>
                        <!-- Header -->
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px;'>🎉 Mua vé thành công!</h1>
                            <p style='color: #ffffff; margin: 10px 0 0 0; opacity: 0.9;'>Cảm ơn bạn đã tin tương Hostlistic</p>
                        </div>
                        
                        <!-- Content -->
                        <div style='padding: 30px;'>
                            <p style='font-size: 16px; margin-bottom: 20px;'>Xin chào <strong>{request.CustomerName}</strong>,</p>
                            
                            <p style='font-size: 14px; line-height: 1.6; color: #666;'>
                                Đơn hàng của bạn đã được xác nhận thành công. Dưới đây là thông tin chi tiết về vé sự kiện của bạn.
                            </p>

                            <!-- Event Info -->
                            <div style='background-color: #f8f9fa; border-left: 4px solid #667eea; padding: 20px; margin: 20px 0; border-radius: 0 8px 8px 0;'>
                                <h3 style='margin: 0 0 10px 0; color: #333; font-size: 18px;'>📅 Thông tin sự kiện</h3>
                                <p style='margin: 5px 0; color: #666;'><strong>Tên sự kiện:</strong> {request.EventName}</p>
                                <p style='margin: 5px 0; color: #666;'><strong>Thời gian:</strong> {request.EventDate:dd/MM/yyyy HH:mm}</p>
                                <p style='margin: 5px 0; color: #666;'><strong>Địa điểm:</strong> {request.EventLocation}</p>
                            </div>

                            <!-- Order Info -->
                            <div style='background-color: #f8f9fa; border-left: 4px solid #28a745; padding: 20px; margin: 20px 0; border-radius: 0 8px 8px 0;'>
                                <h3 style='margin: 0 0 10px 0; color: #333; font-size: 18px;'>🛒 Thông tin đơn hàng</h3>
                                <p style='margin: 5px 0; color: #666;'><strong>Mã đơn hàng:</strong> {request.OrderId}</p>
                                <p style='margin: 5px 0; color: #666;'><strong>Ngày mua:</strong> {request.PurchaseDate:dd/MM/yyyy HH:mm}</p>
                                <p style='margin: 5px 0; color: #666;'><strong>Tổng tiền:</strong> <span style='color: #e74c3c; font-weight: bold;'>{request.TotalAmount:N0} VNĐ</span></p>
                            </div>

                            <!-- Tickets -->
                            <div style='margin: 30px 0;'>
                                <h3 style='color: #333; font-size: 18px; margin-bottom: 20px;'>🎫 Vé của bạn</h3>
                                {ticketsHtml}
                            </div>

                            <!-- Instructions -->
                            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                <h4 style='margin: 0 0 10px 0; color: #856404;'>📱 Hướng dẫn sử dụng vé</h4>
                                <ul style='color: #856404; margin: 0; padding-left: 20px;'>
                                    <li>Vui lòng mang theo mã vé hoặc mã QR khi tham dự sự kiện</li>
                                    <li>Bạn có thể lưu ảnh QR code hoặc screenshot email này</li>
                                    <li>Mỗi vé chỉ được sử dụng một lần duy nhất</li>
                                    <li>Liên hệ với chúng tôi nếu bạn cần hỗ trợ</li>
                                </ul>
                            </div>
                        </div>
                        
                        <!-- Footer -->
                        <div style='background-color: #343a40; padding: 20px; text-align: center;'>
                            <p style='color: #ffffff; margin: 0; font-size: 14px;'>
                                Cảm ơn bạn đã sử dụng dịch vụ của Hostlistic
                            </p>
                            <p style='color: #adb5bd; margin: 10px 0 0 0; font-size: 12px;'>
                                Nếu bạn có thắc mắc, vui lòng liên hệ: support@hostlistic.tech
                            </p>
                        </div>
                    </div>
                </body>
                </html>"
        };

        await resend.EmailSendAsync(message);
    }
    

    public async Task SendTeamMemberInviteEmailAsync(InviteMemberEmailRequest request)
    {
        var baseUrl = "https://hostlistic.tech";
        var acceptUrl = $"{baseUrl}/invitations/{request.InviteToken}/accept?eventId={request.EventId}";
        var declineUrl = $"{baseUrl}/invitations/{request.InviteToken}/decline?eventId={request.EventId}";

        var roleDisplay = request.CustomTitle ?? request.Role;
        var expiryFormatted = request.InviteTokenExpiry.ToString("MMMM dd, yyyy 'at' HH:mm 'UTC'");

        var htmlBody = $$"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8"/>
            <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background-color: #f8f9fa; }
                .container { max-width: 600px; margin: 0 auto; padding: 40px 20px; }
                .card { background: #ffffff; border-radius: 12px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
                .header { text-align: center; margin-bottom: 24px; }
                .header h1 { color: #6D28D9; font-size: 24px; margin: 0; }
                .content { color: #374151; line-height: 1.6; font-size: 16px; }
                .role-badge { display: inline-block; background: #EDE9FE; color: #6D28D9; padding: 4px 12px; border-radius: 6px; font-weight: 600; font-size: 14px; }
                .event-title { font-weight: 700; color: #1F2937; }
                .buttons { text-align: center; margin: 32px 0 16px; }
                .btn { display: inline-block; padding: 12px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 16px; margin: 0 8px; }
                .btn-accept { background: #6D28D9; color: #ffffff; }
                .btn-decline { background: #F3F4F6; color: #6B7280; border: 1px solid #D1D5DB; }
                .footer { text-align: center; color: #9CA3AF; font-size: 13px; margin-top: 24px; }
                .expiry { background: #FEF3C7; color: #92400E; padding: 8px 16px; border-radius: 6px; font-size: 14px; text-align: center; margin-top: 16px; }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="card">
                    <div class="header">
                        <h1>Hostlistic</h1>
                    </div>
                    <div class="content">
                        <p>Hi <strong>{{request.InvitedUserName}}</strong>,</p>
                        <p><strong>{{request.InvitedByUserName}}</strong> has invited you to join the event
                           <span class="event-title">"{{request.EventTitle}}"</span> as
                           <span class="role-badge">{{roleDisplay}}</span>.</p>
                        <p>You can accept or decline this invitation using the buttons below:</p>
                    </div>
                    <div class="buttons">
                        <a href="{{acceptUrl}}" class="btn btn-accept">Accept Invitation</a>
                        <a href="{{declineUrl}}" class="btn btn-decline">Decline</a>
                    </div>
                    <div class="expiry">
                        This invitation expires on {{expiryFormatted}}
                    </div>
                </div>
                <div class="footer">
                    <p>Hostlistic — Event Management Platform</p>
                </div>
            </div>
        </body>
        </html>
        """;

        var message = new EmailMessage
        {
            From = "Hostlistic <noreply@hostlistic.tech>",
            To = request.InvitedUserEmail,
            Subject = $"You're invited to join \"{request.EventTitle}\" as {roleDisplay}",
            HtmlBody = htmlBody
        };
        await resend.EmailSendAsync(message);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new EmailMessage
        {
            From = "Hostlistic <noreply@hostlistic.tech>",
            To = toEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        await resend.EmailSendAsync(message);
    }

    private static string BuildTicketsHtml(List<TicketEmailInfo> tickets)
    {
        var ticketsHtml = new StringBuilder();

        foreach (var (ticket, index) in tickets.Select((t, i) => (t, i)))
        {
            ticketsHtml.Append($@"
                <div style='border: 2px dashed #667eea; border-radius: 12px; padding: 20px; margin-bottom: 15px; background: linear-gradient(135deg, #f8f9ff 0%, #f0f2ff 100%);'>
                    <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%' style='border-collapse: collapse;'>
                      <tr>
                        <td valign='top' style='padding-right: 15px;'>
                            <h4 style='margin: 0 0 10px 0; color: #333; font-size: 16px;'>🎟️ Vé #{index + 1}</h4>
                            <p style='margin: 5px 0; color: #666; font-size: 14px;'><strong>Loại vé:</strong> {ticket.TicketTypeName}</p>
                            <p style='margin: 5px 0; color: #666; font-size: 14px;'><strong>Giá:</strong> {ticket.Price:N0} VNĐ</p>
                            <p style='margin: 5px 0; color: #667eea; font-size: 14px; font-weight: bold;'><strong>Mã vé:</strong> {ticket.TicketCode}</p>
                        </td>
                        <td valign='top' align='center' style='width: 220px;'>
                          {(string.IsNullOrEmpty(ticket.QrCodeUrl) ? "" : $@"
                          <p style='margin: 0 0 10px 0; color: #666; font-size: 12px;'>Mã QR:</p>
                          <img src='{ticket.QrCodeUrl}' alt='QR Code' style='display:block; width: 200px; max-width: 200px; height: auto; border: 2px solid #667eea; border-radius: 8px; background-color: #ffffff; padding: 6px;' />
                          <p style='margin: 10px 0 0 0; color: #666; font-size: 11px;'>Quét mã này tại cổng vào</p>")}
                        </td>
                      </tr>
                    </table>
                </div>");
        }

        return ticketsHtml.ToString();
    }
}