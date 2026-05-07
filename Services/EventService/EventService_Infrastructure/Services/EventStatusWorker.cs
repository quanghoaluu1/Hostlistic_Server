using Common.Messages;
using EventService_Application.Interfaces;
using EventService_Application.IntegrationEvents;
using EventService_Domain.Enums;
using EventService_Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventService_Infrastructure.Services;

public class EventStatusWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventStatusWorker> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(60);

    public EventStatusWorker(IServiceScopeFactory scopeFactory, ILogger<EventStatusWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventStatusWorker is starting. Polling interval: {Interval}s.",
            _pollingInterval.TotalSeconds);

        // Initial delay so the service fully starts before first poll
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessStatusTransitionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "EventStatusWorker: unhandled exception during status transition poll.");
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("EventStatusWorker is stopping.");
    }

    private async Task ProcessStatusTransitionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventServiceDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var notificationClient = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();
        var now = DateTime.UtcNow;

        // 1. Process Events
        // Published -> OnGoing (Start - 15m)
        var eventsToOngoing = await dbContext.Events
            .Where(e => e.EventStatus == EventStatus.Published && e.StartDate != null && e.StartDate.Value.AddMinutes(-15) <= now)
            .AsNoTracking()
            .Select(e => new { e.Id, e.Title, e.OrganizerId, e.EventMode, e.StartDate, e.EndDate })
            .ToListAsync(ct);

        // foreach (var @event in eventsToOngoing)
        // {
        //     @event.EventStatus = EventStatus.OnGoing;
        //     _logger.LogInformation("Event {EventId} transitioned to OnGoing.", @event.Id);
        // }

        if (eventsToOngoing.Count > 0)
        {
            await dbContext.Events
                .Where(e => e.EventStatus == EventStatus.Published && e.StartDate != null && e.StartDate.Value.AddMinutes(-15) <= now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.EventStatus, EventStatus.OnGoing)
                    .SetProperty(e => e.UpdatedAt, now), ct);
            foreach (var e in eventsToOngoing)
            {
                await publishEndpoint.Publish(new EventStartedIntegrationEvent(
                    EventId: e.Id,
                    Title: e.Title ?? string.Empty,
                    OrganizerId: e.OrganizerId,
                    EventMode: e.EventMode.ToString(),
                    StartDate: e.StartDate!.Value,
                    EndDate: e.EndDate
                ), ct);
                _logger.LogInformation("Event {EventId} transitioned to OnGoing.", e.Id);
            }
        }

        // OnGoing -> Completed (End + 15m)
        var eventsToCompleted = await dbContext.Events
            .Where(e => e.EventStatus == EventStatus.OnGoing
                        && e.EndDate != null
                        && e.EndDate.Value.AddMinutes(15) <= now)
            .AsNoTracking()
            .Select(e => new { e.Id, e.Title, e.OrganizerId })
            .ToListAsync(ct);

        _logger.LogDebug("EventStatusWorker: found {Count} OnGoing event(s) past end+15m threshold.",
            eventsToCompleted.Count);

        if (eventsToCompleted.Count > 0)
        {
            // ── BUG FIX: Use the SAME filter as the SELECT above (EndDate + 15m <= now)
            // The previous UPDATE used EndDate <= now (missing +15m), so events found by the
            // SELECT were NOT updated — causing the EventCompletedMessage to be published
            // but the event remaining OnGoing, triggering the message again on every poll.
            var updatedCount = await dbContext.Events
                .Where(e => e.EventStatus == EventStatus.OnGoing
                            && e.EndDate.HasValue
                            && e.EndDate.Value.AddMinutes(15) <= now)   // ← must match SELECT
                .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.EventStatus, EventStatus.Completed)
                        .SetProperty(e => e.UpdatedAt, now),
                    ct);

            _logger.LogInformation(
                "EventStatusWorker: {UpdatedCount} event(s) marked Completed in DB (selected {SelectedCount}).",
                updatedCount, eventsToCompleted.Count);

            foreach (var e in eventsToCompleted)
            {
                _logger.LogInformation(
                    "EventStatusWorker: publishing EventCompletedMessage for Event {EventId} '{Title}' (Organizer {OrganizerId}).",
                    e.Id, e.Title, e.OrganizerId);

                try
                {
                    // Direct HTTP call to NotificationService — bypasses RabbitMQ for Thank-You email.
                    await notificationClient.TriggerThankYouEmailAsync(
                        e.Id,
                        e.Title ?? string.Empty,
                        e.OrganizerId,
                        now,
                        ct);

                    // Also publish RabbitMQ messages (best-effort, for other consumers).
                    await publishEndpoint.Publish(new EventCompletedMessage
                    {
                        EventId = e.Id,
                        OrganizerId = e.OrganizerId,
                        EventTitle = e.Title ?? string.Empty,
                        CompletedAt = now
                    }, ct);

                    _logger.LogInformation(
                        "EventStatusWorker: EventCompletedMessage published for Event {EventId}.", e.Id);

                    // Publish local integration event — consumed by StreamingService.
                    await publishEndpoint.Publish(new EventCompletedIntegrationEvent(
                        EventId: e.Id,
                        Title: e.Title ?? string.Empty,
                        OrganizerId: e.OrganizerId,
                        CompletedAt: now
                    ), ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "EventStatusWorker: failed to publish completion messages for Event {EventId}.", e.Id);
                }
            }

            _logger.LogInformation(
                "EventStatusWorker: {Count} event(s) transitioned OnGoing → Completed.",
                eventsToCompleted.Count);
        }

        // 2. Process Sessions
// Scheduled -> OnGoing (Start - 15m)
        var sessionsToOngoing = await dbContext.Sessions
            .Where(s => s.Status == SessionStatus.Scheduled
                        && s.StartTime != null
                        && s.StartTime.Value.AddMinutes(-15) <= now)
            .Select(s => new { s.Id })
            .ToListAsync(ct);

        if (sessionsToOngoing.Count > 0)
        {
            await dbContext.Sessions
                .Where(s => s.Status == SessionStatus.Scheduled
                            && s.StartTime != null
                            && s.StartTime.Value.AddMinutes(-15) <= now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, SessionStatus.OnGoing), ct);

            foreach (var session in sessionsToOngoing)
            {
                _logger.LogInformation("Session {SessionId} transitioned to OnGoing.", session.Id);
            }
        }

// OnGoing -> Completed (End + 15m)
        var sessionsToCompleted = await dbContext.Sessions
            .Where(s => s.Status == SessionStatus.OnGoing
                        && s.EndTime != null
                        && s.EndTime.Value.AddMinutes(15) <= now)
            .Select(s => new { s.Id, s.EventId, s.Title })
            .ToListAsync(ct);

        if (sessionsToCompleted.Count > 0)
        {
            await dbContext.Sessions
                .Where(s => s.Status == SessionStatus.OnGoing
                            && s.EndTime != null
                            && s.EndTime.Value.AddMinutes(15) <= now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Status, SessionStatus.Completed), ct);

            foreach (var session in sessionsToCompleted)
            {
                _logger.LogInformation("Session {SessionId} transitioned to Completed.", session.Id);

                await publishEndpoint.Publish(new SessionCompletedMessage
                {
                    SessionId = session.Id,
                    EventId = session.EventId,
                    SessionTitle = session.Title ?? string.Empty,
                    CompletedAt = now
                }, ct);
            }
        }

        // if (eventsToOngoing.Any() || eventsToCompleted.Any() || sessionsToOngoing.Any() || sessionsToCompleted.Any())
        // {
        //     await dbContext.SaveChangesAsync(ct);
        // }
    }
}
