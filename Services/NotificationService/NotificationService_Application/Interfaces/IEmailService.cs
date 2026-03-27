using NotificationService_Application.Dtos;

namespace NotificationService_Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(EmailOtpRequest request);
    Task SendTicketPurchaseConfirmationAsync(TicketPurchaseEmailRequest request);
    Task SendTeamMemberInviteEmailAsync(InviteMemberEmailRequest request);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}