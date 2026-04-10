namespace EventService_Application.IntegrationEvents;


public record EventCancelledIntegrationEvent(
    Guid EventId,
    string Title,
    Guid OrganizerId,
    string Reason,
    DateTime CancelledAt
);