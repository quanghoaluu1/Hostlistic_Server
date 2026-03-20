using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using Common;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class PayOsWebhookHandler(
    IOrderService orderService,
    IPaymentService paymentService,
    ITicketService ticketService,
    ITicketPurchaseService ticketPurchaseService,
    IInventoryService inventoryService,
    IEventServiceClient eventServiceClient,
    IUserServiceClient userServiceClient,
    INotificationServiceClient notificationServiceClient,
    ILogger<PayOsWebhookHandler> logger
    ) : IPayOsWebhookHandler
{
    public async Task<ApiResponse<bool>> HandlePaymentSuccessAsync(PayOsWebhookDataDto webhookData)
    {
        var orderResponse = await orderService.GetOrderByPayOsCodeAsync(webhookData.OrderCode);
        var order = orderResponse.Data;
        if (order is null)
        {
            logger.LogError("Order not found for PayOS orderCode {OrderCode}", webhookData.OrderCode);
            return ApiResponse<bool>.Fail(404, "Order not found");
        }

        if (order.Status == OrderStatus.Confirmed)
        {
            logger.LogInformation("Order {OrderId} already confirmed, skipping", order.Id);
            return ApiResponse<bool>.Success(200, "Already processed", true);
        }
        var payments = await paymentService.GetPaymentsByOrderIdAsync(order.Id);
        var pendingPayment = payments.Data?.FirstOrDefault(p => p.Status == PaymentStatus.Pending);
        if (pendingPayment is not null)
        {
            await paymentService.UpdatePaymentAsync(pendingPayment.Id, new UpdatePaymentRequest()
            {
                Status = PaymentStatus.Completed,
                TransactionId = webhookData.Reference
            });
        }
        var reservationId = ExtractReservationId(order.Notes);
        if (reservationId.HasValue)
        {
            await inventoryService.ConfirmReservationAsync(reservationId.Value);
        }

        // 5. Generate tickets
        var orderDetails = order.OrderDetails.Select(od => new DTOs.TicketItemRequest
        {
            TicketTypeId = od.TicketTypeId,
            Quantity = od.Quantity,
            UnitPrice = od.UnitPrice
        }).ToList();
        var tickets = await ticketPurchaseService.GenerateTicketsWithQrCodesAsync(order.Id, orderDetails);
        await orderService.UpdateOrderAsync(order.Id, new DTOs.UpdateOrderRequest
        {
            Status = OrderStatus.Confirmed,
            Notes = $"PayOS payment confirmed. Ref: {webhookData.Reference}"
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var eventInfo = await eventServiceClient.GetEventInfoAsync(order.EventId);
                var userInfo = await userServiceClient.GetUserInfoAsync(order.UserId);
                await notificationServiceClient.SendTicketPurchaseConfirmationAsync(new PurchaseConfirmationRequest
                {
                    UserId = order.UserId,
                    OrderId = order.Id,
                    TotalAmount = (decimal)webhookData.Amount,
                    EventName = eventInfo?.Title ?? "Unknown Event",
                    EventDate = eventInfo?.StartDate ?? DateTime.Now,
                    EventLocation = eventInfo?.Location ?? "TBD",
                    CustomerName = userInfo?.FullName ?? "Valued Customer",
                    CustomerEmail = userInfo?.Email ?? ""
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email for order {OrderId}", order.Id);
            }
        });
        return ApiResponse<bool>.Success(200, "Payment processed successfully", true);
    }
    
    private static Guid? ExtractReservationId(string? notes)
    {
        if (string.IsNullOrEmpty(notes)) return null;
        // Format: "ReservationId:{guid}"
        var prefix = "ReservationId:";
        var idx = notes.IndexOf(prefix, StringComparison.Ordinal);
        if (idx < 0) return null;
        var guidStr = notes[(idx + prefix.Length)..].Split(' ', ',', ';').First();
        return Guid.TryParse(guidStr, out var guid) ? guid : null;
    }
}