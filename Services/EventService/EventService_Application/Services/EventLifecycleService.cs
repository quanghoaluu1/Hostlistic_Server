using Common;
using EventService_Application.IntegrationEvents;
using EventService_Application.Interfaces;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Common.Messages;

namespace EventService_Application.Services;

public class EventLifecycleService(IEventRepository eventRepository, IBus bus, ILogger<EventLifecycleService> logger) : IEventLifecycleService
{
    public async Task<ApiResponse<bool>> StartEventAsync(Guid eventId, Guid requesterId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<bool>.Fail(404, "Event not found.");
        if (eventEntity.OrganizerId != requesterId)
            return ApiResponse<bool>.Fail(403, "Only the event organizer can start the event.");
        if (eventEntity.EventStatus != EventStatus.Published)
            return ApiResponse<bool>.Fail(400,
                $"Cannot start event. Current status is '{eventEntity.EventStatus}'. Event must be Published.");
        if (eventEntity.StartDate.HasValue)
        {
            var twoHoursBefore = eventEntity.StartDate.Value.AddHours(-2);
            if (DateTime.UtcNow < twoHoursBefore)
                return ApiResponse<bool>.Fail(400,
                    "Event cannot be started more than 2 hours before its scheduled start time.");
        }
        eventEntity.EventStatus = EventStatus.OnGoing;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventRepository.UpdateEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        await bus.Publish(new EventStartedIntegrationEvent(
            EventId: eventEntity.Id,
            Title: eventEntity.Title ?? string.Empty,
            OrganizerId: requesterId,
            EventMode: eventEntity.EventMode.ToString(),
            StartDate: eventEntity.StartDate ?? DateTime.UtcNow,
            EndDate: eventEntity.EndDate));
        logger.LogInformation("Event {EventId} manually started by organizer {OrganizerId}.",
            eventId, requesterId);

        return ApiResponse<bool>.Success(200, "Event started successfully.", true);
    }
    
    public async Task<ApiResponse<bool>> CompleteEventAsync(Guid eventId, Guid requesterId)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<bool>.Fail(404, "Event not found.");

        if (eventEntity.OrganizerId != requesterId)
            return ApiResponse<bool>.Fail(403, "Only the event organizer can complete the event.");

        if (eventEntity.EventStatus != EventStatus.OnGoing)
            return ApiResponse<bool>.Fail(400,
                $"Cannot complete event. Current status is '{eventEntity.EventStatus}'. Event must be OnGoing.");

        eventEntity.EventStatus = EventStatus.Completed;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventRepository.UpdateEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        await bus.Publish(new EventCompletedIntegrationEvent(
            EventId: eventEntity.Id,
            Title: eventEntity.Title ?? string.Empty,
            OrganizerId: requesterId,
            CompletedAt: DateTime.UtcNow
        ));

        logger.LogInformation("Event {EventId} manually completed by organizer {OrganizerId}.",
            eventId, requesterId);

        return ApiResponse<bool>.Success(200, "Event completed successfully.", true);
    }
    
    public async Task<ApiResponse<bool>> CancelEventAsync(Guid eventId, Guid requesterId, string? reason)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<bool>.Fail(404, "Event not found.");

        if (eventEntity.OrganizerId != requesterId)
            return ApiResponse<bool>.Fail(403, "Only the event organizer can cancel the event.");

        var allowedStatuses = new[] { EventStatus.Published, EventStatus.OnGoing };
        if (!allowedStatuses.Contains(eventEntity.EventStatus))
            return ApiResponse<bool>.Fail(400,
                $"Cannot cancel event with status '{eventEntity.EventStatus}'. Only Published or OnGoing events can be cancelled.");

        eventEntity.EventStatus = EventStatus.Cancelled;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        eventRepository.UpdateEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        await bus.Publish(new EventCancelledIntegrationEvent(
            EventId: eventEntity.Id,
            Title: eventEntity.Title ?? string.Empty,
            OrganizerId: requesterId,
            Reason: reason ?? "No reason provided.",
            CancelledAt: DateTime.UtcNow
        ));

        logger.LogInformation("Event {EventId} cancelled by organizer {OrganizerId}. Reason: {Reason}",
            eventId, requesterId, reason);

        return ApiResponse<bool>.Success(200, "Event cancelled successfully.", true);
    }

    public async Task<ApiResponse<bool>> PostponeEventAsync(Guid eventId, Guid requesterId, DateTime? newStartTime, DateTime? newEndTime, string reason)
    {
        var eventEntity = await eventRepository.GetEventByIdAsync(eventId);
        if (eventEntity is null)
            return ApiResponse<bool>.Fail(404, "Event not found.");

        if (eventEntity.OrganizerId != requesterId)
            return ApiResponse<bool>.Fail(403, "Only the event organizer can postpone the event.");

        if (newStartTime is null || newEndTime is null)
            return ApiResponse<bool>.Fail(400, "NewStartTime and NewEndTime are required.");

        var postponeResult = eventEntity.Postpone(DateTime.UtcNow, DateTime.SpecifyKind(newStartTime.Value, DateTimeKind.Utc), DateTime.SpecifyKind(newEndTime.Value, DateTimeKind.Utc), reason);
        if (!postponeResult.IsSuccess)
            return postponeResult;

        eventRepository.UpdateEventAsync(eventEntity);
        await eventRepository.SaveChangesAsync();

        await bus.Publish(new EventPostponedIntegrationEvent(
            EventId: eventEntity.Id,
            OrganizerId: requesterId,
            EventName: eventEntity.Title ?? string.Empty,
            Reason: reason,
            NewStartTime: newStartTime,
            NewEndTime: newEndTime,
            PostponedAt: DateTime.UtcNow
        ));

        logger.LogInformation("Event {EventId} postponed by organizer {OrganizerId}. New start time: {NewStartTime}. New end time: {NewEndTime}. Reason: {Reason}",
            eventId, requesterId, newStartTime, newEndTime, reason);

        return ApiResponse<bool>.Success(200, "Event postponed successfully.", true);
    }
}
