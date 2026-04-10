using Common.Messages;
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
            try
            {
                await ProcessStatusTransitionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing event status transitions.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
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
            .ToListAsync(ct);

        foreach (var @event in eventsToOngoing)
        {
            @event.EventStatus = EventStatus.OnGoing;
            _logger.LogInformation("Event {EventId} transitioned to OnGoing.", @event.Id);
        }

        // OnGoing -> Completed (End + 15m)
        var eventsToCompleted = await dbContext.Events
            .Where(e => e.EventStatus == EventStatus.OnGoing && e.EndDate != null && e.EndDate.Value.AddMinutes(15) <= now)
            .ToListAsync(ct);

        foreach (var @event in eventsToCompleted)
        {
            @event.EventStatus = EventStatus.Completed;
            _logger.LogInformation("Event {EventId} transitioned to Completed.", @event.Id);

            await publishEndpoint.Publish(new EventCompletedMessage
            {
                EventId = @event.Id,
                OrganizerId = @event.OrganizerId,
                EventTitle = @event.Title ?? string.Empty,
                CompletedAt = now
            }, ct);
        }

        // 2. Process Sessions
        // Scheduled -> OnGoing (Start - 15m)
        var sessionsToOngoing = await dbContext.Sessions
            .Where(s => s.Status == SessionStatus.Scheduled && s.StartTime != null && s.StartTime.Value.AddMinutes(-15) <= now)
            .ToListAsync(ct);

        foreach (var session in sessionsToOngoing)
        {
            session.Status = SessionStatus.OnGoing;
            _logger.LogInformation("Session {SessionId} transitioned to OnGoing.", session.Id);
        }

        // OnGoing -> Completed (End + 15m)
        var sessionsToCompleted = await dbContext.Sessions
            .Where(s => s.Status == SessionStatus.OnGoing && s.EndTime != null && s.EndTime.Value.AddMinutes(15) <= now)
            .ToListAsync(ct);

        foreach (var session in sessionsToCompleted)
        {
            session.Status = SessionStatus.Completed;
            _logger.LogInformation("Session {SessionId} transitioned to Completed.", session.Id);

            await publishEndpoint.Publish(new SessionCompletedMessage
            {
                SessionId = session.Id,
                EventId = session.EventId,
                SessionTitle = session.Title ?? string.Empty,
                CompletedAt = now
            }, ct);
        }

        if (eventsToOngoing.Any() || eventsToCompleted.Any() || sessionsToOngoing.Any() || sessionsToCompleted.Any())
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }
}
