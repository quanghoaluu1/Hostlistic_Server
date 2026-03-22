using BookingService_Api.Hubs;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BookingService_Api.Services;

public class SignalRPaymentNotifier(
    IHubContext<PaymentHub> hubContext,
    ILogger<SignalRPaymentNotifier> logger
    ) : IPaymentNotifier
{
    public async Task NotifyPaymentConfirmedAsync(Guid orderId, PaymentConfirmedPayload payload)
    {
        var groupName = orderId.ToString();
 
        logger.LogInformation(
            "Pushing PaymentConfirmed to group {OrderId} — {TicketCount} tickets",
            groupName, payload.Tickets.Count);
 
        await hubContext.Clients
            .Group(groupName)
            .SendAsync("PaymentConfirmed", payload);
    }
 
    public async Task NotifyPaymentFailedAsync(Guid orderId, string reason)
    {
        var groupName = orderId.ToString();
 
        logger.LogWarning(
            "Pushing PaymentFailed to group {OrderId} — Reason: {Reason}",
            groupName, reason);
 
        await hubContext.Clients
            .Group(groupName)
            .SendAsync("PaymentFailed", new PaymentFailedPayload
            {
                OrderId = orderId,
                Reason = reason
            });
    }
}