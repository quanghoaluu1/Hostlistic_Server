namespace EventService_Application.IntegrationEvents;

public record EventStartedIntegrationEvent(
    Guid EventId,
    string Title,
    Guid OrganizerId,
    string EventMode,
    DateTime StartDate,
    DateTime? EndDate
);