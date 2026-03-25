using NotificationService_Domain.Entities;

namespace NotificationService_Application.Interfaces;

public interface IRecipientResolutionService
{
    Task<List<EventRecipient>> ResolveAsync(EmailCampaign campaign);
    Task<int> CountAsync(EmailCampaign campaign);

}