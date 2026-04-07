using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class WalletBookingConfirmConsumer(
    IPaymentNotifier paymentNotifier,
    IPublishEndpoint publishEndpoint,
    INotificationServiceClient notificationServiceClient,
    ILogger<WalletBookingConfirmConsumer> logger
) : IConsumer<WalletPurchaseCompletedEvent>
{
    public async Task Consume(ConsumeContext<WalletPurchaseCompletedEvent> context)
    {
        var msg = context.Message;

        try
        {
            await paymentNotifier.NotifyPaymentConfirmedAsync(msg.OrderId, new PaymentConfirmedPayload
            {
                OrderId = msg.OrderId,
                OrderCode = 0,
                Status = "Confirmed",
                TotalAmount = msg.TotalAmount,
                Tickets = msg.Tickets.Select(t => new TicketSummaryDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    TicketTypeName = t.TicketTypeName,
                    QrCodeUrl = t.QrCodeUrl,
                    Price = t.Price
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to push SignalR notification for wallet order {OrderId}. " +
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
                "Published BookingConfirmedEvent for wallet order {OrderId}, Event {EventId}, User {UserId}",
                msg.OrderId, msg.EventId, msg.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish BookingConfirmedEvent for wallet order {OrderId}. " +
                "Recipient will be missing from NotificationService until manual sync.",
                msg.OrderId);
            throw;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await notificationServiceClient.SendTicketPurchaseConfirmationAsync(new PurchaseConfirmationRequest
                {
                    UserId = msg.UserId,
                    OrderId = msg.OrderId,
                    TotalAmount = msg.TotalAmount,
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
                        Price = t.Price
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email for wallet order {OrderId}", msg.OrderId);
            }
        });
    }
}
