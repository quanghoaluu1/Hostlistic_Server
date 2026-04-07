using NotificationService_Application.Interfaces;
using NotificationService_Domain.Entities;
using NotificationService_Domain.Interfaces;

namespace NotificationService_Application.Services;

public class RecipientResolutionService(IEventRecipientRepository recipientRepository) : IRecipientResolutionService
{
    public async Task<List<EventRecipient>> ResolveAsync(EmailCampaign campaign)
    {
        if (campaign.EventId is null)
            return [];
 
        return await recipientRepository.GetRecipientsAsync(
            campaign.EventId.Value,
            campaign.RecipientGroup,
            campaign.TargetFilter);
    }
 
    public async Task<int> CountAsync(EmailCampaign campaign)
    {
        if (campaign.EventId is null)
            return 0;
 
        return await recipientRepository.CountRecipientsAsync(
            campaign.EventId.Value,
            campaign.RecipientGroup,
            campaign.TargetFilter);
    }
}