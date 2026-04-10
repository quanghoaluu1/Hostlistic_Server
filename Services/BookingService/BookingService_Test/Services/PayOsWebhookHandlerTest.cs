using Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Test;

public class PayOsWebhookHandlerTest
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ITicketService _ticketService;
    private readonly ITicketPurchaseService _ticketPurchaseService;
    private readonly IInventoryService _inventoryService;
    private readonly IEventServiceClient _eventServiceClient;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PayOsWebhookHandler> _logger;
    private readonly PayOsWebhookHandler _sut;

    public PayOsWebhookHandlerTest()
    {
        _orderService = Substitute.For<IOrderService>();
        _paymentService = Substitute.For<IPaymentService>();
        _ticketService = Substitute.For<ITicketService>();
        _ticketPurchaseService = Substitute.For<ITicketPurchaseService>();
        _inventoryService = Substitute.For<IInventoryService>();
        _eventServiceClient = Substitute.For<IEventServiceClient>();
        _userServiceClient = Substitute.For<IUserServiceClient>();
        _notificationServiceClient = Substitute.For<INotificationServiceClient>();
        _paymentNotifier = Substitute.For<IPaymentNotifier>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<PayOsWebhookHandler>>();

        _sut = new PayOsWebhookHandler(
            _orderService,
            _paymentService,
            _ticketService,
            _ticketPurchaseService,
            _inventoryService,
            _eventServiceClient,
            _userServiceClient,
            _notificationServiceClient,
            _paymentNotifier,
            _publishEndpoint,
            _logger);
    }

    [Fact]
    public async Task HandlePaymentSuccessAsync_WhenOrderNotFound_ReturnsFail404()
    {
        var data = new PayOsVerifiedPaymentData { OrderCode = 123, Amount = 100_000, Reference = "REF" };
        _orderService.GetOrderByPayOsCodeAsync(data.OrderCode)
            .Returns(ApiResponse<OrderDto>.Success(200, "OK", null));

        var result = await _sut.HandlePaymentSuccessAsync(data);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Order not found");
    }

    [Fact]
    public async Task HandlePaymentSuccessAsync_WhenOrderAlreadyConfirmed_ReturnsSuccess200WithoutFurtherProcessing()
    {
        var order = new OrderDto
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = OrderStatus.Confirmed,
            Notes = "ReservationId:" + Guid.NewGuid()
        };

        _orderService.GetOrderByPayOsCodeAsync(Arg.Any<long>())
            .Returns(ApiResponse<OrderDto>.Success(200, "OK", order));

        var result = await _sut.HandlePaymentSuccessAsync(new PayOsVerifiedPaymentData
        {
            OrderCode = 888,
            Amount = 50_000,
            Reference = "REF-888"
        });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("Already processed");
        await _paymentService.DidNotReceive().GetPaymentsByOrderIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task HandlePaymentSuccessAsync_WithPendingPaymentAndReservation_ProcessesAndReturnsSuccess()
    {
        var reservationId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var order = new OrderDto
        {
            Id = orderId,
            EventId = eventId,
            UserId = userId,
            Status = OrderStatus.Pending,
            Notes = $"ReservationId:{reservationId}",
            OrderDetails =
            [
                new OrderDetailDto
                {
                    TicketTypeId = ticketTypeId,
                    TicketTypeName = "VIP",
                    Quantity = 2,
                    UnitPrice = 100_000
                }
            ]
        };

        _orderService.GetOrderByPayOsCodeAsync(999)
            .Returns(ApiResponse<OrderDto>.Success(200, "OK", order));

        _paymentService.GetPaymentsByOrderIdAsync(orderId)
            .Returns(ApiResponse<IEnumerable<PaymentDto>>.Success(200, "OK",
            [
                new PaymentDto
                {
                    Id = paymentId,
                    OrderId = orderId,
                    PaymentMethodId = Guid.NewGuid(),
                    Amount = 200_000,
                    Status = PaymentStatus.Pending,
                    Gateway = "PayOs"
                }
            ]));

        _eventServiceClient.GetEventInfoAsync(eventId)
            .Returns(new EventInfoDto { Title = "Summit", Location = "HCMC", StartDate = DateTime.UtcNow });
        _userServiceClient.GetUserInfoAsync(userId)
            .Returns(new UserInfoDto { FullName = "Alice", Email = "alice@example.com" });

        _ticketPurchaseService.GenerateTicketsWithQrCodesAsync(
                orderId,
                Arg.Any<List<TicketItemRequest>>(),
                eventId,
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>())
            .Returns(
            [
                new TicketDto
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    TicketTypeId = ticketTypeId,
                    TicketTypeName = "VIP",
                    TicketCode = "VIP001",
                    QrCodeUrl = "QR1"
                }
            ]);

        _paymentNotifier.NotifyPaymentConfirmedAsync(Arg.Any<Guid>(), Arg.Any<PaymentConfirmedPayload>())
            .Returns(Task.CompletedTask);
        _publishEndpoint.Publish(Arg.Any<BookingConfirmedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _sut.HandlePaymentSuccessAsync(new PayOsVerifiedPaymentData
        {
            OrderCode = 999,
            Amount = 200_000,
            Reference = "PAYOS-OK-REF"
        });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        await _paymentService.Received(1).UpdatePaymentAsync(paymentId,
            Arg.Is<UpdatePaymentRequest>(x =>
                x!.Status == PaymentStatus.Completed &&
                x.TransactionId == "PAYOS-OK-REF"));

        await _inventoryService.Received(1).ConfirmReservationAsync(reservationId);
        await _orderService.Received(1).UpdateOrderAsync(orderId,
            Arg.Is<UpdateOrderRequest>(x => x!.Status == OrderStatus.Confirmed));
        await _paymentNotifier.Received(1).NotifyPaymentConfirmedAsync(orderId, Arg.Any<PaymentConfirmedPayload>());
        await _publishEndpoint.Received(1).Publish(Arg.Any<BookingConfirmedEvent>(), Arg.Any<CancellationToken>());
    }
}