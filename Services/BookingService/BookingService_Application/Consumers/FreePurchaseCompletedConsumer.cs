using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class FreePurchaseCompletedConsumer(
    IPaymentNotifier paymentNotifier,
    IPublishEndpoint publishEndpoint,
    INotificationServiceClient notificationServiceClient,
    ILogger<FreePurchaseCompletedConsumer> logger
) : IConsumer<FreePurchaseCompletedEvent>
{
    public async Task Consume(ConsumeContext<FreePurchaseCompletedEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Received FreePurchaseCompletedEvent for order {OrderId}, Event {EventId}, User {UserId}",
            msg.OrderId, msg.EventId, msg.UserId);

        try
        {
            await paymentNotifier.NotifyPaymentConfirmedAsync(msg.OrderId, new PaymentConfirmedPayload
            {
                OrderId = msg.OrderId,
                OrderCode = 0,
                Status = "Confirmed",
                TotalAmount = 0,
                Tickets = msg.Tickets.Select(t => new TicketSummaryDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    TicketTypeName = t.TicketTypeName,
                    QrCodeUrl = t.QrCodeUrl,
                    Price = 0
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to push SignalR notification for free order {OrderId}. " +
                "Client can use polling fallback.", msg.OrderId);
        }

        try
        {
            var ticketsByType = msg.Tickets
                .GroupBy(t => t.TicketTypeId)
                .Select(g => new BookingTicketInfo(
                    TicketTypeId: g.Key,
                    TicketTypeName: g.First().TicketTypeName,
                    Quantity: g.Count()))
                .ToList();

            await publishEndpoint.Publish(new BookingConfirmedEvent(
                EventId: msg.EventId,
                UserId: msg.UserId,
                Email: msg.CustomerEmail,
                FullName: msg.CustomerName,
                OrderId: msg.OrderId,
                Tickets: ticketsByType,
                ConfirmedAt: msg.CompletedAt
            ));

            logger.LogInformation(
                "Published BookingConfirmedEvent for free order {OrderId}, Event {EventId}, User {UserId}",
                msg.OrderId, msg.EventId, msg.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish BookingConfirmedEvent for free order {OrderId}. " +
                "Recipient will be missing from NotificationService until manual sync.",
                msg.OrderId);
            throw;
        }

        // Email template receives TotalAmount = 0 — the existing template renders this as free
        _ = Task.Run(async () =>
        {
            try
            {
                await notificationServiceClient.SendTicketPurchaseConfirmationAsync(new PurchaseConfirmationRequest
                {
                    UserId = msg.UserId,
                    OrderId = msg.OrderId,
                    TotalAmount = 0,
                    EventName = msg.EventName,
                    EventDate = msg.EventDate,
                    EventLocation = msg.EventLocation,
                    CustomerName = msg.CustomerName,
                    CustomerEmail = msg.CustomerEmail,
                    Tickets = msg.Tickets.Select(t => new TicketDto
                    {
                        Id = t.Id,
                        TicketTypeId = t.TicketTypeId,
                        TicketCode = t.TicketCode,
                        TicketTypeName = t.TicketTypeName,
                        QrCodeUrl = t.QrCodeUrl,
                        Price = 0
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email for free order {OrderId}", msg.OrderId);
            }
        });
    }
}
