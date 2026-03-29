using Microsoft.AspNetCore.SignalR;

namespace EventService_Api.Hubs;

public class CheckInHub : Hub
{
    private readonly ILogger<CheckInHub> _logger;

    public CheckInHub(ILogger<CheckInHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinEventGroup(string eventId)
    {
        if (!Guid.TryParse(eventId, out _))
        {
            _logger.LogWarning("Invalid eventId received on JoinEventGroup: {EventId}", eventId);
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");
        _logger.LogInformation("Client {ConnectionId} joined check-in group event-{EventId}",
            Context.ConnectionId, eventId);
    }

    public async Task LeaveEventGroup(string eventId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");
        _logger.LogInformation("Client {ConnectionId} left check-in group event-{EventId}",
            Context.ConnectionId, eventId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected. Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "Normal");
        await base.OnDisconnectedAsync(exception);
    }
}
