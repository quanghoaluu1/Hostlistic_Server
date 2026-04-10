namespace EventService_Application.IntegrationEvents;


public record EventCompletedIntegrationEvent(
    Guid EventId,
    string Title,
    Guid OrganizerId,
    DateTime CompletedAt
);