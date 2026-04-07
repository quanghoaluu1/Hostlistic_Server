using BookingService_Application.Interfaces;
using Common;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class EventCompletedConsumer(ISettlementService settlementService, ILogger<EventCompletedConsumer> logger) : IConsumer<EventCompletedMessage>
{
    public async Task Consume(ConsumeContext<EventCompletedMessage> context)
    {
        var message = context.Message;
        logger.LogInformation("Received EventCompleted for event {EventId}, organizer {OrganizerId}",
            message.EventId, message.OrganizerId);

        var result = await settlementService.SettleEventAsync(message.EventId, message.OrganizerId);

        if (result.IsSuccess)
        {
            logger.LogInformation("Settlement successful for event {EventId}. Net revenue: {Net}",
                message.EventId, result.Data?.NetRevenue);
        }
        else
        {
            logger.LogError("Settlement failed for event {EventId}: {Message}",
                message.EventId, result.Message);
            // MassTransit sẽ retry theo policy configured
            throw new InvalidOperationException($"Settlement failed: {result.Message}");
        }
    }
}