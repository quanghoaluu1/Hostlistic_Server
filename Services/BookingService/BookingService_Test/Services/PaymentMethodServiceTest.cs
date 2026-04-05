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
    public async Task CreatePaymentMethodWithIconAsync_WhenNameMissing_ReturnsFail400()
    {
        var request = PaymentMethodBuilder.CreateRequest(name: "   ");

        var result = await _sut.CreatePaymentMethodWithIconAsync(request, iconFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("name is required");
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
    public async Task DeletePaymentMethodAsync_WhenNotFound_ReturnsFail404()
    {
        var paymentMethodId = Guid.NewGuid();
        _paymentMethodRepository.PaymentMethodExistsAsync(paymentMethodId).Returns(false);

        var result = await _sut.DeletePaymentMethodAsync(paymentMethodId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}