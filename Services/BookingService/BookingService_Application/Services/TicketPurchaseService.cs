using Common;
using BookingService_Application.DTOs;
using BookingService_Application.DTOs.PayOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using PayOS.Models.V2.PaymentRequests;

namespace BookingService_Application.Services;

public class TicketPurchaseService : ITicketPurchaseService
{
    private readonly IOrderService _orderService;
    private readonly ITicketService _ticketService;
    private readonly IPaymentService _paymentService;
    private readonly IWalletService _walletService;
    private readonly IInventoryService _inventoryService;
    private readonly IEventServiceClient _eventServiceClient;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly IPayOsService _payOsService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TicketPurchaseService> _logger;


    public TicketPurchaseService(
        IOrderService orderService,
        ITicketService ticketService,
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IWalletService walletService,
        IEventServiceClient eventServiceClient,
        IUserServiceClient userServiceClient,
        INotificationServiceClient notificationServiceClient,
        IPayOsService payOsService,
        IPublishEndpoint publishEndpoint,
        ILogger<TicketPurchaseService> logger)
    {
        _orderService = orderService;
        _ticketService = ticketService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _walletService = walletService;
        _eventServiceClient = eventServiceClient;
        _userServiceClient = userServiceClient;
        _notificationServiceClient = notificationServiceClient;
        _payOsService = payOsService;
        _publishEndpoint = publishEndpoint;
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
                var message = availabilityCheck.Data?.Message ?? "Tickets not available";
                _logger.LogWarning("Ticket availability check failed for event {EventId}: {Message}", request.EventId, message);

                var perTicketErrors = availabilityCheck.Data?.TicketAvailability
                    .Where(t => !t.IsValid && !string.IsNullOrEmpty(t.ErrorMessage))
                    .Select(t => $"{t.TicketTypeName}: {t.ErrorMessage}")
                    .ToList();

                return perTicketErrors is { Count: > 0 }
                    ? ApiResponse<PurchaseTicketResponse>.FailWithErrors(400, message, perTicketErrors)
                    : ApiResponse<PurchaseTicketResponse>.Fail(400, message);
            }

            // 1b. Validate holder info if required
            var holderValidationError = await ValidateHolderInfoAsync(request.TicketItems);
            if (holderValidationError is not null)
                return ApiResponse<PurchaseTicketResponse>.Fail(400, holderValidationError);

            // Fetch event and buyer info early (needed for ticket denormalization and email)
            _logger.LogInformation("Fetching event info for EventId {EventId}", request.EventId);
            var eventInfo = await _eventServiceClient.GetEventInfoAsync(request.EventId);

            _logger.LogInformation("Fetching user info for UserId {UserId}", request.UserId);
            var userInfo = await _userServiceClient.GetUserInfoAsync(request.UserId);

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
                    BuyerName = userInfo?.FullName,
                    BuyerEmail = userInfo?.Email,
                    BuyerAvatarUrl = userInfo?.AvatarUrl,
                    OrderDetails = request.TicketItems.Select(item => new CreateOrderDetailRequest
                    {
                        TicketTypeId = item.TicketTypeId,
                        TicketTypeName = item.TicketTypeName,
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

                // 10. Generate tickets with QR codes (TicketTypeName and EventName are persisted at creation)
                var tickets = await GenerateTicketsWithQrCodesAsync(
                    orderResult.Data.Id, request.TicketItems, request.EventId,
                    eventName: eventInfo?.Title ?? string.Empty,
                    buyerName: userInfo?.FullName,
                    buyerEmail: userInfo?.Email);

                // Enrich tickets with price for email template (Price is DTO-only, not persisted)
                if (tickets.Count > 0)
                {
                    var unitPriceByType = request.TicketItems
                        .GroupBy(x => x.TicketTypeId)
                        .ToDictionary(g => g.Key, g => g.First().UnitPrice);

                    foreach (var t in tickets)
                    {
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

                // 12. Publish wallet purchase event — consumer handles SignalR, BookingConfirmedEvent, and email
                try
                {
                    await _publishEndpoint.Publish(new WalletPurchaseCompletedEvent(
                        OrderId: orderResult.Data.Id,
                        EventId: request.EventId,
                        UserId: request.UserId,
                        TotalAmount: totalAmount,
                        EventName: eventInfo?.Title ?? "Unknown Event",
                        EventLocation: eventInfo?.Location ?? "TBD",
                        EventDate: eventInfo?.StartDate ?? DateTime.UtcNow,
                        CustomerName: userInfo?.FullName ?? "Valued Customer",
                        CustomerEmail: userInfo?.Email ?? string.Empty,
                        Tickets: tickets.Select(t => new WalletTicketSummary(
                            Id: t.Id,
                            TicketTypeId: t.TicketTypeId,
                            TicketCode: t.TicketCode,
                            TicketTypeName: t.TicketTypeName,
                            QrCodeUrl: t.QrCodeUrl,
                            Price: t.Price
                        )).ToList(),
                        CompletedAt: DateTime.UtcNow
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to publish WalletPurchaseCompletedEvent for order {OrderId}. " +
                        "SignalR notification and confirmation email will not be sent.",
                        orderResult.Data.Id);
                }

                return ApiResponse<PurchaseTicketResponse>.Success(200, "Tickets purchased successfully", new PurchaseTicketResponse
                {
                    OrderId = orderResult.Data.Id,
                    Tickets = tickets,
                    PaymentId = paymentResult.Data.Id,
                    TotalAmount = totalAmount,
                    Message = "Purchase completed successfully."
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

    public async Task<ApiResponse<PayOsCheckoutResponse>> InitiatePayOsPurchaseAsync(PurchaseTicketRequest request)
    {
        try
        {
            _logger.LogInformation("Starting ticket purchase for user {UserId} and event {EventId}", request.UserId,
                request.EventId);

            // 1. Validate ticket availability
            var availabilityCheck = await _inventoryService.CheckAvailabilityAsync(request.TicketItems);
            if (!availabilityCheck.IsSuccess || !availabilityCheck.Data!.IsAvailable)
            {
                var message = availabilityCheck.Data?.Message ?? "Tickets not available";
                _logger.LogWarning("Ticket availability check failed for event {EventId}: {Message}", request.EventId, message);

                var perTicketErrors = availabilityCheck.Data?.TicketAvailability
                    .Where(t => !t.IsValid && !string.IsNullOrEmpty(t.ErrorMessage))
                    .Select(t => $"{t.TicketTypeName}: {t.ErrorMessage}")
                    .ToList();

                return perTicketErrors is { Count: > 0 }
                    ? ApiResponse<PayOsCheckoutResponse>.FailWithErrors(400, message, perTicketErrors)
                    : ApiResponse<PayOsCheckoutResponse>.Fail(400, message);
            }

            // 1b. Validate holder info if required
            var holderValidationError = await ValidateHolderInfoAsync(request.TicketItems);
            if (holderValidationError is not null)
                return ApiResponse<PayOsCheckoutResponse>.Fail(400, holderValidationError);

            // Fetch buyer info early for order denormalization
            var userInfo = await _userServiceClient.GetUserInfoAsync(request.UserId);

            // 2. Reserve inventory temporarily
            var reservationId = await _inventoryService.ReserveInventoryAsync(request.TicketItems);

            try
            {
                // 3. Calculate total amount
                var totalAmount = request.TicketItems.Sum(x => x.UnitPrice * x.Quantity);

                // 5. Create order
                var orderRequest = new CreateOrderRequest
                {
                    EventId = request.EventId,
                    UserId = request.UserId,
                    Notes = request.Notes,
                    BuyerName = userInfo?.FullName,
                    BuyerEmail = userInfo?.Email,
                    BuyerAvatarUrl = userInfo?.AvatarUrl,
                    OrderDetails = request.TicketItems.Select(item => new CreateOrderDetailRequest
                    {
                        TicketTypeId = item.TicketTypeId,
                        TicketTypeName = item.TicketTypeName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                var orderResult = await _orderService.CreateOrderAsync(orderRequest);
                if (!orderResult.IsSuccess)
                {
                    _logger.LogWarning("Order creation failed for user {UserId}: {Message}", request.UserId,
                        orderResult.Message);
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PayOsCheckoutResponse>.Fail(400, orderResult.Message);
                }

                // 6. Create payment record for wallet transaction
                var paymentResult = await _paymentService.CreatePaymentAsync(new CreatePaymentRequest
                {
                    OrderId = orderResult.Data!.Id,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = totalAmount,
                    Gateway = "PayOs"
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
                    return ApiResponse<PayOsCheckoutResponse>.Fail(400, paymentResult.Message);
                }

                var orderCode = GenerateOrderCode();
                await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest()
                {
                    Status = OrderStatus.Pending,
                    Notes = $"ReservationId:{reservationId} PayOsCode:{orderCode}",
                    OrderCode = orderCode
                });
                
                var payOsRequest = new CreatePayOsPaymentRequest()
                {
                    OrderCode = orderCode,
                    OrderId = orderResult.Data.Id,
                    Amount = (long)totalAmount,
                    Description = $"HOSTLISTIC {request.EventId.ToString()[..8].ToUpper()}",
                    Items = request.TicketItems.Select(ti => new PayOsItemDto
                    {
                        Name = ti.TicketTypeName ?? "Ticket",
                        Quantity = ti.Quantity,
                        Price = ti.UnitPrice
                    }).ToList()
                };

                var payOsResult = await _payOsService.CreatePaymentLinkAsync(payOsRequest);
                if (payOsResult is null)
                {
                    // Rollback
                    await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                    {
                        Status = OrderStatus.Cancelled,
                        Notes = "PayOS payment link creation failed"
                    });
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<PayOsCheckoutResponse>.Fail(502, "Failed to create payment link");
                }
                return ApiResponse<PayOsCheckoutResponse>.Success(200, "Payment link created", new PayOsCheckoutResponse
                {
                    CheckoutUrl = payOsResult.CheckoutUrl,
                    QrCode = payOsResult.QrCode,
                    OrderId = orderResult.Data.Id,
                    OrderCode = orderCode,
                    ExpiresInMinutes = 15
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Purchase failed for user {UserId} and event {EventId}", request.UserId, request.EventId);
                // Rollback on any error
                await _inventoryService.ReleaseReservationAsync(reservationId);
                return ApiResponse<PayOsCheckoutResponse>.Fail(500, $"Purchase failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PurchaseTicketsAsync");
            return ApiResponse<PayOsCheckoutResponse>.Fail(500, $"Purchase failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FreeTicketPurchaseResponse>> PurchaseFreeTicketsAsync(FreeTicketPurchaseRequest request)
    {
        try
        {
            _logger.LogInformation("Starting free ticket registration for user {UserId} and event {EventId}", request.UserId, request.EventId);

            // Step 1: Guard clause — ensure this is truly a free order
            var totalAmount = request.TicketItems.Sum(x => x.UnitPrice * x.Quantity);
            if (totalAmount != 0)
            {
                _logger.LogWarning("Free ticket endpoint called with non-zero total amount {TotalAmount} for user {UserId}", totalAmount, request.UserId);
                return ApiResponse<FreeTicketPurchaseResponse>.Fail(400, "This endpoint only handles free tickets. Total amount must be zero.");
            }

            // Step 2: Validate ticket availability
            var availabilityCheck = await _inventoryService.CheckAvailabilityAsync(request.TicketItems);
            if (!availabilityCheck.IsSuccess || !availabilityCheck.Data!.IsAvailable)
            {
                var message = availabilityCheck.Data?.Message ?? "Tickets not available";
                _logger.LogWarning("Ticket availability check failed for event {EventId}: {Message}", request.EventId, message);

                var perTicketErrors = availabilityCheck.Data?.TicketAvailability
                    .Where(t => !t.IsValid && !string.IsNullOrEmpty(t.ErrorMessage))
                    .Select(t => $"{t.TicketTypeName}: {t.ErrorMessage}")
                    .ToList();

                return perTicketErrors is { Count: > 0 }
                    ? ApiResponse<FreeTicketPurchaseResponse>.FailWithErrors(400, message, perTicketErrors)
                    : ApiResponse<FreeTicketPurchaseResponse>.Fail(400, message);
            }

            // Step 3: Validate holder info if required
            var holderValidationError = await ValidateHolderInfoAsync(request.TicketItems);
            if (holderValidationError is not null)
                return ApiResponse<FreeTicketPurchaseResponse>.Fail(400, holderValidationError);

            // Step 4: Fetch event and user info
            _logger.LogInformation("Fetching event info for EventId {EventId}", request.EventId);
            var eventInfo = await _eventServiceClient.GetEventInfoAsync(request.EventId);

            _logger.LogInformation("Fetching user info for UserId {UserId}", request.UserId);
            var userInfo = await _userServiceClient.GetUserInfoAsync(request.UserId);

            // Step 5: Reserve inventory temporarily
            var reservationId = await _inventoryService.ReserveInventoryAsync(request.TicketItems);

            try
            {
                // Step 6: Create order
                var orderRequest = new CreateOrderRequest
                {
                    EventId = request.EventId,
                    UserId = request.UserId,
                    Notes = request.Notes,
                    BuyerName = userInfo?.FullName,
                    BuyerEmail = userInfo?.Email,
                    BuyerAvatarUrl = userInfo?.AvatarUrl,
                    OrderDetails = request.TicketItems.Select(item => new CreateOrderDetailRequest
                    {
                        TicketTypeId = item.TicketTypeId,
                        TicketTypeName = item.TicketTypeName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
                };

                var orderResult = await _orderService.CreateOrderAsync(orderRequest);
                if (!orderResult.IsSuccess)
                {
                    _logger.LogWarning("Order creation failed for user {UserId}: {Message}", request.UserId, orderResult.Message);
                    await _inventoryService.ReleaseReservationAsync(reservationId);
                    return ApiResponse<FreeTicketPurchaseResponse>.Fail(400, orderResult.Message);
                }

                // Step 7: Skip payment creation — no Payment record for free tickets

                // Step 8: Confirm inventory reduction
                await _inventoryService.ConfirmReservationAsync(reservationId);

                // Step 9: Generate tickets with QR codes
                var tickets = await GenerateTicketsWithQrCodesAsync(
                    orderResult.Data!.Id, request.TicketItems, request.EventId,
                    eventName: eventInfo?.Title ?? string.Empty,
                    buyerName: userInfo?.FullName,
                    buyerEmail: userInfo?.Email);

                // Step 10: Update order status to Confirmed
                await _orderService.UpdateOrderAsync(orderResult.Data.Id, new UpdateOrderRequest
                {
                    Status = OrderStatus.Confirmed,
                    Notes = "Free registration completed"
                });

                // Step 11: Publish FreePurchaseCompletedEvent — consumer handles SignalR notification and confirmation email
                try
                {
                    await _publishEndpoint.Publish(new Common.Messages.FreePurchaseCompletedEvent(
                        OrderId: orderResult.Data.Id,
                        EventId: request.EventId,
                        UserId: request.UserId,
                        EventName: eventInfo?.Title ?? "Unknown Event",
                        EventLocation: eventInfo?.Location ?? "TBD",
                        EventDate: eventInfo?.StartDate ?? DateTime.UtcNow,
                        CustomerName: userInfo?.FullName ?? "Valued Customer",
                        CustomerEmail: userInfo?.Email ?? string.Empty,
                        Tickets: tickets.Select(t => new Common.Messages.FreeTicketSummary(
                            Id: t.Id,
                            TicketTypeId: t.TicketTypeId,
                            TicketCode: t.TicketCode,
                            TicketTypeName: t.TicketTypeName,
                            QrCodeUrl: t.QrCodeUrl
                        )).ToList(),
                        CompletedAt: DateTime.UtcNow
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to publish FreePurchaseCompletedEvent for order {OrderId}. " +
                        "SignalR notification and confirmation email will not be sent.",
                        orderResult.Data.Id);
                }

                // Step 12: Return success response
                return ApiResponse<FreeTicketPurchaseResponse>.Success(200, "Free registration completed successfully", new FreeTicketPurchaseResponse
                {
                    OrderId = orderResult.Data.Id,
                    Tickets = tickets,
                    TotalAmount = 0,
                    Message = "Free registration completed successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Free ticket registration failed for user {UserId} and event {EventId}", request.UserId, request.EventId);
                await _inventoryService.ReleaseReservationAsync(reservationId);
                return ApiResponse<FreeTicketPurchaseResponse>.Fail(500, $"Registration failed: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PurchaseFreeTicketsAsync");
            return ApiResponse<FreeTicketPurchaseResponse>.Fail(500, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<List<TicketDto>> GenerateTicketsWithQrCodesAsync(
        Guid orderId,
        List<TicketItemRequest> ticketItems,
        Guid eventId,
        string eventName = "",
        string? buyerName = null,
        string? buyerEmail = null)
    {
        var tickets = new List<TicketDto>();

        // Pre-fetch all ticket type names in one pass to avoid N+1 calls inside the loop
        var ticketTypeNameMap = new Dictionary<Guid, string>();
        foreach (var item in ticketItems)
        {
            if (!string.IsNullOrEmpty(item.TicketTypeName))
            {
                ticketTypeNameMap[item.TicketTypeId] = item.TicketTypeName;
            }
            else
            {
                var info = await _eventServiceClient.GetTicketTypeInfoAsync(item.TicketTypeId);
                ticketTypeNameMap[item.TicketTypeId] = info?.Name ?? string.Empty;
            }
        }

        foreach (var item in ticketItems)
        {
            for (int i = 0; i < item.Quantity; i++)
            {
                string? holderName;
                string? holderEmail;
                string? holderPhone;

                if (item.Holders is not null && item.Holders.Count > i)
                {
                    holderName = item.Holders[i].Name;
                    holderEmail = item.Holders[i].Email;
                    holderPhone = item.Holders[i].Phone;
                }
                else
                {
                    // No explicit holder data — buyer is the holder
                    holderName = buyerName;
                    holderEmail = buyerEmail;
                    holderPhone = null;
                }

                var ticketResult = await _ticketService.CreateTicketAsync(new CreateTicketRequest
                {
                    OrderId = orderId,
                    TicketTypeId = item.TicketTypeId,
                    EventId = eventId,
                    TicketTypeName = ticketTypeNameMap.GetValueOrDefault(item.TicketTypeId, string.Empty),
                    EventName = eventName,
                    HolderName = holderName,
                    HolderEmail = holderEmail,
                    HolderPhone = holderPhone,
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

                tickets.Add(ticketResult.Data!);
            }
        }

        return tickets;
    }

    private async Task<string?> ValidateHolderInfoAsync(List<TicketItemRequest> ticketItems)
    {
        foreach (var item in ticketItems)
        {
            var ticketTypeInfo = await _eventServiceClient.GetTicketTypeInfoAsync(item.TicketTypeId);
            if (ticketTypeInfo is null || !ticketTypeInfo.IsRequireHolderInfo)
                continue;

            if (item.Quantity >= 2)
            {
                if (item.Holders is null || item.Holders.Count == 0)
                    return $"Holder information is required for ticket type '{item.TicketTypeName}' when purchasing multiple tickets.";

                if (item.Holders.Count != item.Quantity)
                    return $"Holder count ({item.Holders.Count}) must match quantity ({item.Quantity}) for ticket type '{item.TicketTypeName}'.";

                for (int i = 0; i < item.Holders.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(item.Holders[i].Name))
                        return $"Holder name is required for ticket {i + 1} of type '{item.TicketTypeName}'.";
                }
            }
            // Quantity == 1: buyer is the holder — no holder data required
        }

        return null;
    }

    private static long GenerateOrderCode()
    {
        // PayOS orderCode phải là số nguyên dương, max 9007199254740991 
        // Dùng timestamp (ms) + random 3 digits
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = Random.Shared.Next(100, 999);
        return timestamp * 1000 + random;
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
    public string? AvatarUrl { get; set; }
}

public class TicketTypeInfoDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsRequireHolderInfo { get; set; }
}
public class EventSettlementInfoDto
{
    public Guid EventId { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int EventStatus { get; set; }
}
