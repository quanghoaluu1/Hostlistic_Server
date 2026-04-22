namespace Common.Messages;

public record EventPostponedIntegrationEvent(
    Guid EventId,
    Guid OrganizerId,
    string EventName,
    string Reason,
    DateTime? NewStartTime,
    DateTime? NewEndTime,
    DateTime PostponedAt
);
