using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class EventPostponedIntegrationEventConsumer(
    IOrderRepository orderRepository,
    ITicketRepository ticketRepository,
    ILogger<EventPostponedIntegrationEventConsumer> logger) : IConsumer<EventPostponedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EventPostponedIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Received EventPostponedIntegrationEvent for event {EventId}", msg.EventId);

        // Find all confirmed orders for this event
        var orders = await orderRepository.GetConfirmedOrdersByEventIdAsync(msg.EventId);
        
        var orderIds = orders.Select(o => o.Id).ToList();
        
        int updatedTickets = 0;
        
        // Find tickets belonging to these orders and set status
        foreach(var orderId in orderIds)
        {
            var tickets = await ticketRepository.GetTicketsByOrderIdAsync(orderId);
            foreach(var ticket in tickets)
            {
                // We only care about tickets that haven't been used yet
                if(!ticket.IsUsed)
                {
                    ticket.PostponementStatus = PostponementStatus.PendingDecision;
                    await ticketRepository.UpdateTicketAsync(ticket);
                    updatedTickets++;
                }
            }
        }
        
        await ticketRepository.SaveChangesAsync();
        logger.LogInformation("Updated {Count} tickets to PendingDecision for postponed event {EventId}", updatedTickets, msg.EventId);
    }
}
