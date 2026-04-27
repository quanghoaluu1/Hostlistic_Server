using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using BookingService_Domain.Entities;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Consumers;

public class EventCancelledConsumer(
    IOrderRepository orderRepository,
    ITicketRepository ticketRepository,
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    ILogger<EventCancelledConsumer> logger) : IConsumer<EventCancelledIntegrationEvent>
{
    public async Task Consume(ConsumeContext<EventCancelledIntegrationEvent> context)
    {
        var msg = context.Message;
        logger.LogInformation("Received EventCancelledIntegrationEvent for event {EventId}", msg.EventId);

        var orders = await orderRepository.GetConfirmedOrdersByEventIdAsync(msg.EventId);
        
        int processedOrders = 0;
        int skippedOrders = 0;

        foreach (var order in orders)
        {
            if (order.Status == OrderStatus.Refunded || order.Status == OrderStatus.Cancelled)
            {
                skippedOrders++;
                continue;
            }

            // Calculate total refund amount
            decimal refundAmount = 0m;
            if (order.OrderDetails != null)
            {
                refundAmount = order.OrderDetails.Sum(od => od.UnitPrice * od.Quantity);
            }

            await using var tx = await unitOfWork.BeginTransactionAsync();
            try
            {
                var tickets = await ticketRepository.GetTicketsByOrderIdAsync(order.Id);
                foreach (var ticket in tickets)
                {
                    ticket.IsCancelled = true;
                    ticket.PostponementStatus = PostponementStatus.Refunded;
                    await ticketRepository.UpdateTicketAsync(ticket);
                }

                if (refundAmount > 0)
                {
                    var wallet = await walletRepository.GetWalletByUserIdAsync(order.UserId);
                    if (wallet == null)
                    {
                        throw new InvalidOperationException($"Wallet not found for user {order.UserId} while refunding order {order.Id}");
                    }

                    wallet.Balance += refundAmount;
                    await walletRepository.UpdateWalletAsync(wallet);

                    var ledgerEntry = new Transaction
                    {
                        WalletId = wallet.Id,
                        Type = TransactionType.Refund,
                        Amount = refundAmount,
                        PlatformFee = 0m,
                        NetAmount = refundAmount,
                        BalanceAfter = wallet.Balance,
                        ReferenceId = order.Id,
                        ReferenceType = "Order",
                        Status = TransactionStatus.Completed,
                        Description = $"Refund due to event cancellation: {msg.Title}"
                    };
                    await transactionRepository.AddAsync(ledgerEntry);
                }

                order.Status = OrderStatus.Refunded;
                await orderRepository.UpdateOrderAsync(order);

                await unitOfWork.SaveChangesAsync();
                await tx.CommitAsync();

                processedOrders++;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                logger.LogError(ex, "Failed to process refund for order {OrderId}", order.Id);
                skippedOrders++;
            }
        }

        logger.LogInformation("Completed EventCancelledIntegrationEvent for event {EventId}. Processed: {ProcessedCount}, Skipped/Failed: {SkippedCount}", 
            msg.EventId, processedOrders, skippedOrders);
    }
}
