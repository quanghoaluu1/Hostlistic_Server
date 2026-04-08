using MassTransit;
using Microsoft.Extensions.Logging;

namespace BookingService_Test;

public class TicketPurchaseServiceTest
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
    private readonly TicketPurchaseService _sut;

    public TicketPurchaseServiceTest()
    {
        _orderService = Substitute.For<IOrderService>();
        _ticketService = Substitute.For<ITicketService>();
        _paymentService = Substitute.For<IPaymentService>();
        _walletService = Substitute.For<IWalletService>();
        _inventoryService = Substitute.For<IInventoryService>();
        _eventServiceClient = Substitute.For<IEventServiceClient>();
        _userServiceClient = Substitute.For<IUserServiceClient>();
        _notificationServiceClient = Substitute.For<INotificationServiceClient>();
        _payOsService = Substitute.For<IPayOsService>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<TicketPurchaseService>>();

        _sut = new TicketPurchaseService(
            _orderService,
            _ticketService,
            _paymentService,
            _inventoryService,
            _walletService,
            _eventServiceClient,
            _userServiceClient,
            _notificationServiceClient,
            _payOsService,
            _publishEndpoint,
            _logger);
    }

    [Fact]
    public async Task CheckTicketAvailabilityAsync_ReturnsInventoryResult()
    {
        var request = new InventoryCheckRequest
        {
            TicketItems =
            [
                new TicketItemRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "General",
                    Quantity = 1,
                    UnitPrice = 100_000
                }
            ]
        };

        var inventoryResponse = ApiResponse<InventoryCheckResponse>.Success(200, "OK", new InventoryCheckResponse
        {
            IsAvailable = true,
            Message = "Available"
        });
        _inventoryService.CheckAvailabilityAsync(request.TicketItems).Returns(inventoryResponse);

        var result = await _sut.CheckTicketAvailabilityAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task PurchaseFreeTicketsAsync_WhenTotalAmountNotZero_ReturnsFail400()
    {
        var request = new FreeTicketPurchaseRequest
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TicketItems =
            [
                new TicketItemRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "General",
                    Quantity = 1,
                    UnitPrice = 1
                }
            ]
        };

        var result = await _sut.PurchaseFreeTicketsAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("only handles free tickets");
    }

    [Fact]
    public async Task PurchaseTicketsAsync_WhenAvailabilityCheckFails_ReturnsFail400WithPerTicketErrors()
    {
        var request = new PurchaseTicketRequest
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PaymentMethodId = Guid.NewGuid(),
            TicketItems =
            [
                new TicketItemRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "VIP",
                    Quantity = 1,
                    UnitPrice = 200_000
                }
            ]
        };

        _inventoryService.CheckAvailabilityAsync(request.TicketItems)
            .Returns(ApiResponse<InventoryCheckResponse>.Success(200, "Not enough", new InventoryCheckResponse
            {
                IsAvailable = false,
                Message = "Not enough tickets",
                TicketAvailability =
                [
                    new TicketAvailabilityInfo
                    {
                        TicketTypeId = request.TicketItems[0].TicketTypeId,
                        TicketTypeName = "VIP",
                        RequestedQuantity = 1,
                        AvailableQuantity = 0,
                        IsValid = false,
                        ErrorMessage = "Sold out"
                    }
                ]
            }));

        var result = await _sut.PurchaseTicketsAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Errors.Should().NotBeNull();
        result.Errors!.Should().Contain(x => x.Contains("VIP") && x.Contains("Sold out"));
        await _inventoryService.DidNotReceive().ReserveInventoryAsync(Arg.Any<List<TicketItemRequest>>());
    }

    [Fact]
    public async Task PurchaseTicketsAsync_WhenWalletMissing_ReleasesReservationAndReturnsFail400()
    {
        var reservationId = Guid.NewGuid();
        var request = new PurchaseTicketRequest
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PaymentMethodId = Guid.NewGuid(),
            TicketItems =
            [
                new TicketItemRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "General",
                    Quantity = 1,
                    UnitPrice = 100_000
                }
            ]
        };

        _inventoryService.CheckAvailabilityAsync(request.TicketItems)
            .Returns(ApiResponse<InventoryCheckResponse>.Success(200, "OK", new InventoryCheckResponse
            {
                IsAvailable = true
            }));

        _eventServiceClient.GetTicketTypeInfoAsync(request.TicketItems[0].TicketTypeId)
            .Returns(new TicketTypeInfoDto { Name = "General", IsRequireHolderInfo = false });

        _eventServiceClient.GetEventInfoAsync(request.EventId)
            .Returns(new EventInfoDto { Title = "Event A", Location = "HCMC", StartDate = DateTime.UtcNow });

        _userServiceClient.GetUserInfoAsync(request.UserId)
            .Returns(new UserInfoDto { FullName = "Tester", Email = "tester@example.com" });

        _inventoryService.ReserveInventoryAsync(request.TicketItems).Returns(reservationId);
        _walletService.GetWalletByUserIdAsync(request.UserId)
            .Returns(ApiResponse<WalletDto>.Fail(404, "Wallet not found for this user."));

        var result = await _sut.PurchaseTicketsAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Wallet not found");
        await _inventoryService.Received(1).ReleaseReservationAsync(reservationId);
    }

    [Fact]
    public async Task GenerateTicketsWithQrCodesAsync_WhenNoHolders_UsesBuyerInfo()
    {
        var orderId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var ticketTypeId = Guid.NewGuid();
        var capturedRequests = new List<CreateTicketRequest>();

        _ticketService
            .When(x => x.CreateTicketAsync(Arg.Any<CreateTicketRequest>()))
            .Do(call => capturedRequests.Add(call.Arg<CreateTicketRequest>()!));

        _ticketService.CreateTicketAsync(Arg.Any<CreateTicketRequest>())
            .Returns(call =>
            {
                var req = call.Arg<CreateTicketRequest>()!;
                return Task.FromResult(ApiResponse<TicketDto>.Success(201, "Created", new TicketDto
                {
                    Id = Guid.NewGuid(),
                    OrderId = req.OrderId,
                    TicketTypeId = req.TicketTypeId,
                    TicketTypeName = req.TicketTypeName,
                    EventName = req.EventName,
                    TicketCode = "TCK",
                    QrCodeUrl = "QR",
                    HolderName = req.HolderName,
                    HolderEmail = req.HolderEmail,
                    HolderPhone = req.HolderPhone
                }));
            });

        var items = new List<TicketItemRequest>
        {
            new()
            {
                TicketTypeId = ticketTypeId,
                TicketTypeName = "VIP",
                Quantity = 2,
                UnitPrice = 100_000
            }
        };

        var result = await _sut.GenerateTicketsWithQrCodesAsync(
            orderId,
            items,
            eventId,
            eventName: "Event X",
            buyerName: "Buyer Name",
            buyerEmail: "buyer@example.com");

        result.Should().HaveCount(2);
        capturedRequests.Should().HaveCount(2);
        capturedRequests.All(r => r.HolderName == "Buyer Name").Should().BeTrue();
        capturedRequests.All(r => r.HolderEmail == "buyer@example.com").Should().BeTrue();
        await _eventServiceClient.DidNotReceive().GetTicketTypeInfoAsync(Arg.Any<Guid>());
    }
}