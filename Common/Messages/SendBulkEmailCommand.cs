namespace Common.Messages;

public record SendBulkEmailCommand(
    Guid CampaignId,
    Guid RequestedBy
    );