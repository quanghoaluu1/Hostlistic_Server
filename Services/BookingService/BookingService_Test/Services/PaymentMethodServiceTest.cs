using BookingService_Test.Helpers.TestDataBuilders;

namespace BookingService_Test;

public class PaymentMethodServiceTest
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IPhotoService _photoService;
    private readonly PaymentMethodService _sut;

    public PaymentMethodServiceTest()
    {
        _paymentMethodRepository = Substitute.For<IPaymentMethodRepository>();
        _photoService = Substitute.For<IPhotoService>();
        _sut = new PaymentMethodService(_paymentMethodRepository, _photoService);
    }

    [Fact]
    public async Task GetPaymentMethodByIdAsync_WhenNotFound_ReturnsFail404()
    {
        _paymentMethodRepository.GetPaymentMethodByIdAsync(Arg.Any<Guid>())
            .Returns((PaymentMethod?)null);

        var result = await _sut.GetPaymentMethodByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPaymentMethodByIdAsync_WhenFound_ReturnsSuccess200()
    {
        var id = Guid.NewGuid();
        _paymentMethodRepository.GetPaymentMethodByIdAsync(id)
            .Returns(PaymentMethodBuilder.CreateEntity(id: id));

        var result = await _sut.GetPaymentMethodByIdAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetActivePaymentMethodsAsync_ReturnsSuccess200WithCollection()
    {
        _paymentMethodRepository.GetActivePaymentMethodsAsync().Returns(
        [
            PaymentMethodBuilder.CreateEntity(isActive: true),
            PaymentMethodBuilder.CreateEntity(isActive: true)
        ]);

        var result = await _sut.GetActivePaymentMethodsAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllPaymentMethodsAsync_ReturnsSuccess200WithCollection()
    {
        _paymentMethodRepository.GetAllPaymentMethodsAsync().Returns(
        [
            PaymentMethodBuilder.CreateEntity(isActive: true),
            PaymentMethodBuilder.CreateEntity(isActive: false)
        ]);

        var result = await _sut.GetAllPaymentMethodsAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaymentMethodByCodeAsync_WhenNotFound_ReturnsFail404()
    {
        _paymentMethodRepository.GetPaymentMethodByCodeAsync("UNKNOWN").Returns((PaymentMethod?)null);

        var result = await _sut.GetPaymentMethodByCodeAsync("UNKNOWN");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdatePaymentMethodWithIconAsync_WhenNotFound_ReturnsFail404()
    {
        _paymentMethodRepository.GetPaymentMethodByIdAsync(Arg.Any<Guid>()).Returns((PaymentMethod?)null);

        var result = await _sut.UpdatePaymentMethodWithIconAsync(
            Guid.NewGuid(),
            new UpdatePaymentMethodRequest
            {
                Name = "Updated",
                FeePercentage = 2,
                FixedFee = 1000,
                IsActive = true
            },
            iconFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdatePaymentMethodWithIconAsync_WhenNameMissing_ReturnsFail400()
    {
        var id = Guid.NewGuid();
        _paymentMethodRepository.GetPaymentMethodByIdAsync(id).Returns(PaymentMethodBuilder.CreateEntity(id: id));

        var result = await _sut.UpdatePaymentMethodWithIconAsync(
            id,
            new UpdatePaymentMethodRequest
            {
                Name = " ",
                FeePercentage = 2,
                FixedFee = 1000,
                IsActive = true
            },
            iconFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreatePaymentMethodWithIconAsync_WhenNameMissing_ReturnsFail400()
    {
        var request = PaymentMethodBuilder.CreateRequest(name: "   ");

        var result = await _sut.CreatePaymentMethodWithIconAsync(request, iconFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("name is required");
    }

    [Fact]
    public async Task CreatePaymentMethodWithIconAsync_WhenCodeMissing_ReturnsFail400()
    {
        var request = PaymentMethodBuilder.CreateRequest();
        request.Code = "   ";

        var result = await _sut.CreatePaymentMethodWithIconAsync(request, iconFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("code is required");
    }

    [Fact]
    public async Task CreatePaymentMethodWithIconAsync_WhenCodeExists_ReturnsFail400()
    {
        var request = PaymentMethodBuilder.CreateRequest(code: "VNPAY");
        _paymentMethodRepository.PaymentMethodCodeExistsAsync("VNPAY").Returns(true);

        var result = await _sut.CreatePaymentMethodWithIconAsync(request, iconFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("code already exists");
    }

    [Fact]
    public async Task GetPaymentOptionsAsync_WithZeroTotalAmount_ReturnsFreeMethodOnly()
    {
        var request = PaymentMethodBuilder.FreeOptionsRequest();

        var result = await _sut.GetPaymentOptionsAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(0);
        result.Data.PaymentMethods.Should().HaveCount(1);
        result.Data.PaymentMethods[0].Code.Should().Be("FREE");
    }

    [Fact]
    public async Task GetPaymentOptionsAsync_WithPositiveTotalAmount_ReturnsActivePaymentMethods()
    {
        var request = new GetPaymentOptionsRequest
        {
            EventId = Guid.NewGuid(),
            TicketItems =
            [
                new TicketItemRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "VIP",
                    Quantity = 2,
                    UnitPrice = 100_000
                }
            ]
        };

        _paymentMethodRepository.GetActivePaymentMethodsAsync().Returns(
        [
            PaymentMethodBuilder.CreateEntity(name: "VNPay", code: "VNPAY", isActive: true),
            PaymentMethodBuilder.CreateEntity(name: "PayOS", code: "PAYOS", isActive: true)
        ]);

        var result = await _sut.GetPaymentOptionsAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(200_000);
        result.Data.PaymentMethods.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeletePaymentMethodAsync_WhenNotFound_ReturnsFail404()
    {
        var paymentMethodId = Guid.NewGuid();
        _paymentMethodRepository.PaymentMethodExistsAsync(paymentMethodId).Returns(false);

        var result = await _sut.DeletePaymentMethodAsync(paymentMethodId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeletePaymentMethodAsync_WhenDeleteFails_ReturnsFail500()
    {
        var paymentMethodId = Guid.NewGuid();
        _paymentMethodRepository.PaymentMethodExistsAsync(paymentMethodId).Returns(true);
        _paymentMethodRepository.DeletePaymentMethodAsync(paymentMethodId).Returns(false);

        var result = await _sut.DeletePaymentMethodAsync(paymentMethodId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeletePaymentMethodAsync_WhenDeleteSucceeds_ReturnsSuccess200()
    {
        var paymentMethodId = Guid.NewGuid();
        _paymentMethodRepository.PaymentMethodExistsAsync(paymentMethodId).Returns(true);
        _paymentMethodRepository.DeletePaymentMethodAsync(paymentMethodId).Returns(true);

        var result = await _sut.DeletePaymentMethodAsync(paymentMethodId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _paymentMethodRepository.Received(1).SaveChangesAsync();
    }
}