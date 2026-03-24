using Microsoft.AspNetCore.SignalR;

namespace BookingService_Api.Hubs;

public class PaymentHub : Hub
{
    private readonly ILogger<PaymentHub> _logger;

    public PaymentHub(ILogger<PaymentHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinOrderGroup(string orderId)
    {
        if (!Guid.TryParse(orderId, out var id))
        {
            _logger.LogError("Invalid order ID: {OrderId}", orderId);
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        _logger.LogInformation("Client {ConnectionId} joined payment group {OrderId}", Context.ConnectionId, orderId);
    }
    
    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        _logger.LogInformation(
            "Client {ConnectionId} left payment group {OrderId}",
            Context.ConnectionId, orderId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Client {ConnectionId} disconnected. Reason: {Reason}",
            Context.ConnectionId, exception?.Message ?? "Normal");
        await base.OnDisconnectedAsync(exception);
    }
}