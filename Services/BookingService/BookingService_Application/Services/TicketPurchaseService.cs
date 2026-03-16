using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class TicketPurchaseService : ITicketPurchaseService
{
    private readonly IOrderService _orderService;
    private readonly ITicketService _ticketService;
    private readonly IPaymentService _paymentService;
    private readonly IWalletService _walletService;
    private readonly IInventoryService _inventoryService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IEventServiceClient _eventServiceClient;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<TicketPurchaseService> _logger;

    public TicketPurchaseService(
        IOrderService orderService,
        ITicketService ticketService,
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IQrCodeService qrCodeService,
        IWalletService walletService,
        IEventServiceClient eventServiceClient,
        IUserServiceClient userServiceClient,
        INotificationServiceClient notificationServiceClient,
        ILogger<TicketPurchaseService> logger)
    {
        _orderService = orderService;
        _ticketService = ticketService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _qrCodeService = qrCodeService;
        _walletService = walletService;
        _eventServiceClient = eventServiceClient;
        _userServiceClient = userServiceClient;
        _notificationServiceClient = notificationServiceClient;
        _logger = logger;
    }

    public async Task<ApiResponse<InventoryCheckResponse>> CheckTicketAvailabilityAsync(InventoryCheckRequest request)
    {
        return await _inventoryService.CheckAvailabilityAsync(request.TicketItems);
    }

    public async Task<ApiResponse<PurchaseTicketResponse>> PurchaseTicketsAsync(PurchaseTicketRequest request)
    {
        try
        {
            _logger.LogInformation("Starting ticket purchase for user {UserId} and event {EventId}", request.UserId, request.EventId);

            // 1. Validate ticket availability
            var availabilityCheck = await _inventoryService.CheckAvailabilityAsync(request.TicketItems);
            if (!availabilityCheck.IsSuccess || !availabilityCheck.Data!.IsAvailable)
            {
                _logger.LogWarning("Ticket availability check failed for event {EventId}: {Message}", request.EventId,
                    availabilityCheck.Data?.Message ?? "Tickets not available");
                return ApiResponse<PurchaseTicketResponse>.Fail(400,
                    availabilityCheck.Data?.Message ?? "Tickets not available");
            }

            // 2. Reserve inventory temporarily
            var reservationId = await _inventoryService.ReserveInventoryAsync(request.TicketItems);

            try
            {
                // 3. Calculate total amount
                var totalAmount = request.TicketItems.Sum(x => x.UnitPrice * x.Quantity);

                // 4. Validate wallet and balance
                var walletResult = await _walletService.GetWalletByUserIdAsync(request.UserId);
                if (!walletResult.IsSuccess || walletResult.Data is null)
                {
                    _logger.LogWarning("Wallet not found for user {UserId} when purchasing tickets", request.UserId);
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400,
                        walletResult.Message ?? "Wallet not found for this user.");
                }

                if (walletResult.Data.Balance < totalAmount)
                {
                    _logger.LogWarning("Insufficient wallet balance for user {UserId}. Balance={Balance}, Required={Required}",
                        request.UserId, walletResult.Data.Balance, totalAmount);
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400, "Insufficient wallet balance.");
                }

                // 5. Create order
                var orderRequest = new CreateOrderRequest
                {
                    EventId = request.EventId,
                    UserId = request.UserId,
                    Notes = request.Notes,
                    OrderDetails = request.TicketItems.Select(item => new CreateOrderDetailRequest
                    {
                        TicketTypeId = item.TicketTypeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                var orderResult = await _orderService.CreateOrderAsync(orderRequest);
                if (!orderResult.IsSuccess)
                {
                    _logger.LogWarning("Order creation failed for user {UserId}: {Message}", request.UserId, orderResult.Message);
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400, orderResult.Message);
                }

                // 6. Create payment record for wallet transaction
                var paymentResult = await _paymentService.CreatePaymentAsync(new CreatePaymentRequest
                {
                    OrderId = orderResult.Data!.Id,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = totalAmount,
                    Gateway = "Wallet"
                });

                if (!paymentResult.IsSuccess)
                {
                    _logger.LogWarning("Payment creation failed for order {OrderId}: {Message}", orderResult.Data.Id,
                        paymentResult.Message);
                    await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                    {
                        Status = OrderStatus.Cancelled,
                        Notes = "Payment failed"
                    });
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400, paymentResult.Message);
                }

                // 7. Debit user wallet
                var walletDebitResult = await _walletService.UpdateWalletBalanceAsync(
                    walletResult.Data.Id,
                    new UpdateWalletBalanceRequest
                    {
                        Amount = totalAmount,
                        TransactionType = "Withdraw"
                    });

                if (!walletDebitResult.IsSuccess)
                {
                    _logger.LogError("Wallet debit failed for wallet {WalletId}: {Message}", walletResult.Data.Id,
                        walletDebitResult.Message);
                    await _paymentService.UpdatePaymentAsync(paymentResult.Data!.Id, new UpdatePaymentRequest
                    {
                        Status = PaymentStatus.Failed,
                        TransactionId = $"WALLET-FAILED-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}"
                    });

                    await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                    {
                        Status = OrderStatus.Cancelled,
                        Notes = "Wallet payment failed"
                    });

                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PurchaseTicketResponse>.Fail(400,
                        walletDebitResult.Message ?? "Wallet payment failed.");
                }

                // 8. Mark payment as completed
                await _paymentService.UpdatePaymentAsync(paymentResult.Data!.Id, new UpdatePaymentRequest
                {
                    Status = PaymentStatus.Completed,
                    TransactionId = $"WALLET-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}"
                });

                // 9. Confirm inventory reduction
                await _inventoryService.ConfirmReservationAsync(reservationId);

                // 10. Generate tickets with QR codes
                var tickets = await GenerateTicketsWithQrCodesAsync(orderResult.Data.Id, request.TicketItems);

                // Enrich tickets with type name + price for email template
                if (tickets.Count > 0)
                {
                    var unitPriceByType = request.TicketItems
                        .GroupBy(x => x.TicketTypeId)
                        .ToDictionary(g => g.Key, g => g.First().UnitPrice);

                    var ticketTypeIds = tickets.Select(t => t.TicketTypeId).Distinct().ToList();
                    var ticketTypeInfoById = new Dictionary<Guid, TicketTypeInfoDto>();

                    foreach (var ticketTypeId in ticketTypeIds)
                    {
                        var info = await _eventServiceClient.GetTicketTypeInfoAsync(ticketTypeId);
                        if (info is not null)
                        {
                            ticketTypeInfoById[ticketTypeId] = info;
                        }
                    }

                    foreach (var t in tickets)
                    {
                        if (ticketTypeInfoById.TryGetValue(t.TicketTypeId, out var info))
                        {
                            t.TicketTypeName = info.Name;
                        }

                        if (unitPriceByType.TryGetValue(t.TicketTypeId, out var price))
                        {
                            t.Price = price;
                        }
                    }
                }

                // 11. Update order status
                await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                {
                    Status = OrderStatus.Confirmed,
                    Notes = "Payment completed and tickets generated"
                });

                // 12. Get event and user information for email
                _logger.LogInformation("Fetching event info for EventId {EventId}", request.EventId);
                var eventInfo = await _eventServiceClient.GetEventInfoAsync(request.EventId);

                _logger.LogInformation("Fetching user info for UserId {UserId}", request.UserId);
                var userInfo = await _userServiceClient.GetUserInfoAsync(request.UserId);

                // 13. Send confirmation email with tickets and QR codes
                var emailSent = await _notificationServiceClient.SendTicketPurchaseConfirmationAsync(new PurchaseConfirmationRequest
                {
                    UserId = request.UserId,
                    OrderId = orderResult.Data.Id,
                    Tickets = tickets,
                    TotalAmount = totalAmount,
                    EventName = eventInfo?.Title ?? "Unknown Event",
                    EventDate = eventInfo?.StartDate ?? DateTime.Now,
                    EventLocation = eventInfo?.Location ?? "TBD",
                    CustomerName = userInfo?.FullName ?? "Valued Customer",
                    CustomerEmail = userInfo?.Email ?? ""
                });

                var responseMessage = emailSent 
                    ? "Purchase completed successfully. Confirmation email with tickets and QR codes sent."
                    : "Purchase completed successfully. Email sending failed - please check your email settings.";

                return ApiResponse<PurchaseTicketResponse>.Success(200, "Tickets purchased successfully", new PurchaseTicketResponse
                {
                    OrderId = orderResult.Data.Id,
                    Tickets = tickets,
                    PaymentId = paymentResult.Data.Id,
                    TotalAmount = totalAmount,
                    Message = responseMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Purchase failed for user {UserId} and event {EventId}", request.UserId, request.EventId);
                // Rollback on any error
                await _inventoryService.ReleaseReservationAsync(reservationId);
                return ApiResponse<PurchaseTicketResponse>.Fail(500, $"Purchase failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PurchaseTicketsAsync");
            return ApiResponse<PurchaseTicketResponse>.Fail(500, $"Purchase failed: {ex.Message}");
        }
    }

    private async Task<List<TicketDto>> GenerateTicketsWithQrCodesAsync(
        Guid orderId,
        List<TicketItemRequest> ticketItems)
    {
        var tickets = new List<TicketDto>();

        foreach (var item in ticketItems)
        {
            for (int i = 0; i < item.Quantity; i++)
            {
                var ticketResult = await _ticketService.CreateTicketAsync(new CreateTicketRequest
                {
                    OrderId = orderId,
                    TicketTypeId = item.TicketTypeId
                });

                if (!ticketResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to create ticket for Order {OrderId} and TicketType {TicketTypeId}: {Message}",
                        orderId,
                        item.TicketTypeId,
                        ticketResult.Message);
                    continue;
                }

                var qrCodeUrl = await _qrCodeService.GenerateQrCodeAsync(ticketResult.Data!.TicketCode);

                if (!string.IsNullOrEmpty(qrCodeUrl))
                {
                    await _ticketService.UpdateTicketAsync(ticketResult.Data.Id, new UpdateTicketRequest
                    {
                        QrCodeUrl = qrCodeUrl,
                        IsUsed = false
                    });

                    ticketResult.Data.QrCodeUrl = qrCodeUrl;
                }

                tickets.Add(ticketResult.Data);
            }
        }

        return tickets;
    }

}

// DTOs for external service calls
public class EventInfoDto
{
    // EventService returns `EventResponseDto` with `Title`
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class TicketTypeInfoDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
