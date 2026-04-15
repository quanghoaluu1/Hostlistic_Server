using NotificationService_Application.Dtos.ServiceClientDtos;

namespace NotificationService_Application.Interfaces;

public interface IBookingServiceClient
{
    Task<List<EmailRecipientDto>> GetEmailRecipientsAsync(
        Guid eventId,
        int recipientGroup,
        List<Guid>? ticketTypeIds = null,
        List<Guid>? specificUserIds = null,
        CancellationToken ct = default);
}