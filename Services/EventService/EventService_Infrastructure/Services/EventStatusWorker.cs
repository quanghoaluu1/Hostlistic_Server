using Common.Messages;
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
        _logger.LogInformation("EventStatusWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var timer = new PeriodicTimer(_pollingInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessStatusTransitionsAsync(stoppingToken);
            }
        }

        _logger.LogInformation("EventStatusWorker is stopping.");
    }

    private async Task ProcessStatusTransitionsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EventServiceDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
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
            .Where(e => e.EventStatus == EventStatus.OnGoing && e.EndDate != null && e.EndDate.Value.AddMinutes(15) <= now)
            .AsNoTracking()
            .Select(e => new { e.Id, e.Title, e.OrganizerId })
            .ToListAsync(ct);

        // foreach (var @event in eventsToCompleted)
        // {
        //     @event.EventStatus = EventStatus.Completed;
        //     _logger.LogInformation("Event {EventId} transitioned to Completed.", @event.Id);
        //
        //     await publishEndpoint.Publish(new EventCompletedMessage
        //     {
        //         EventId = @event.Id,
        //         OrganizerId = @event.OrganizerId,
        //         EventTitle = @event.Title ?? string.Empty,
        //         CompletedAt = now
        //     }, ct);
        // }
        if (eventsToCompleted.Count > 0)
        {
            await dbContext.Events
                .Where(e => e.EventStatus == EventStatus.OnGoing
                            && e.EndDate.HasValue
                            && e.EndDate.Value <= now)
                .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.EventStatus, EventStatus.Completed)
                        .SetProperty(e => e.UpdatedAt, now),
                    ct);

            foreach (var e in eventsToCompleted)
            {
                _logger.LogInformation("Event {EventId} transitioned to Completed.", e.Id);
                await publishEndpoint.Publish(new EventCompletedIntegrationEvent(
                    EventId: e.Id,
                    Title: e.Title ?? string.Empty,
                    OrganizerId: e.OrganizerId,
                    CompletedAt: now
                ), ct);
            }

            _logger.LogInformation(
                "EventStatusSyncJob: {Count} event(s) transitioned OnGoing → Completed.",
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
