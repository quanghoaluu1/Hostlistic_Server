using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;
using Resend;
using System.Text;

namespace NotificationService_Application.Services;

public class EmailService(IResend resend) : IEmailService
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

    private static string BuildTicketsHtml(List<TicketEmailInfo> tickets)
    {
        var ticketsHtml = new StringBuilder();

        foreach (var (ticket, index) in tickets.Select((t, i) => (t, i)))
        {
            ticketsHtml.Append($@"
                <div style='border: 2px dashed #667eea; border-radius: 12px; padding: 20px; margin-bottom: 15px; background: linear-gradient(135deg, #f8f9ff 0%, #f0f2ff 100%);'>
                    <div style='display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap;'>
                        <div style='flex: 1; min-width: 250px;'>
                            <h4 style='margin: 0 0 10px 0; color: #333; font-size: 16px;'>🎟️ Vé #{index + 1}</h4>
                            <p style='margin: 5px 0; color: #666; font-size: 14px;'><strong>Loại vé:</strong> {ticket.TicketTypeName}</p>
                            <p style='margin: 5px 0; color: #666; font-size: 14px;'><strong>Giá:</strong> {ticket.Price:N0} VNĐ</p>
                            <p style='margin: 5px 0; color: #667eea; font-size: 14px; font-weight: bold;'><strong>Mã vé:</strong> {ticket.TicketCode}</p>
                        </div>
                        <div style='text-align: center; margin-top: 10px;'>
                            {(string.IsNullOrEmpty(ticket.QrCodeUrl) ? "" : $@"
                            <p style='margin: 0 0 10px 0; color: #666; font-size: 12px;'>Mã QR:</p>
                            <img src='{ticket.QrCodeUrl}' alt='QR Code' style='width: 120px; height: 120px; border: 2px solid #667eea; border-radius: 8px; background-color: white; padding: 5px;' />
                            <p style='margin: 10px 0 0 0; color: #666; font-size: 11px;'>Quét mã này tại cổng vào</p>")}
                        </div>
                    </div>
                </div>");
        }

        return ticketsHtml.ToString();
    }
}