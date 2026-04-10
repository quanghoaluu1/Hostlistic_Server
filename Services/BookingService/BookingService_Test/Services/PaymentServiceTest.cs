using BookingService_Test.Helpers.TestDataBuilders;

namespace BookingService_Test;

public class PaymentServiceTest
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly PaymentService _sut;

    public PaymentServiceTest()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _orderRepository = Substitute.For<IOrderRepository>();
        _paymentMethodRepository = Substitute.For<IPaymentMethodRepository>();
        _sut = new PaymentService(_paymentRepository, _orderRepository, _paymentMethodRepository);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenOrderDoesNotExist_ReturnsFail404()
    {
        var request = PaymentBuilder.CreateRequest();
        _orderRepository.OrderExistsAsync(request.OrderId).Returns(false);

        var result = await _sut.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Order not found");
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenNotFound_ReturnsFail404()
    {
        _paymentRepository.GetPaymentByIdAsync(Arg.Any<Guid>()).Returns((Payment?)null);

        var result = await _sut.GetPaymentByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenFound_ReturnsSuccess200()
    {
        var paymentId = Guid.NewGuid();
        _paymentRepository.GetPaymentByIdAsync(paymentId)
            .Returns(PaymentBuilder.CreateEntity(id: paymentId));

        var result = await _sut.GetPaymentByIdAsync(paymentId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(paymentId);
    }

    [Fact]
    public async Task GetPaymentsByOrderIdAsync_ReturnsSuccess200WithCollection()
    {
        var orderId = Guid.NewGuid();
        _paymentRepository.GetPaymentsByOrderIdAsync(orderId).Returns(
        [
            PaymentBuilder.CreateEntity(orderId: orderId),
            PaymentBuilder.CreateEntity(orderId: orderId)
        ]);

        var result = await _sut.GetPaymentsByOrderIdAsync(orderId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenPaymentMethodDoesNotExist_ReturnsFail404()
    {
        var request = PaymentBuilder.CreateRequest();
        _orderRepository.OrderExistsAsync(request.OrderId).Returns(true);
        _paymentMethodRepository.PaymentMethodExistsAsync(request.PaymentMethodId).Returns(false);

        var result = await _sut.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Payment method not found");
    }

    [Fact]
    public async Task CreatePaymentAsync_WhenAmountIsNotPositive_ReturnsFail400()
    {
        var request = PaymentBuilder.CreateRequest(amount: 0);
        _orderRepository.OrderExistsAsync(request.OrderId).Returns(true);
        _paymentMethodRepository.PaymentMethodExistsAsync(request.PaymentMethodId).Returns(true);

        var result = await _sut.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("greater than zero");
    }

    [Fact]
    public async Task CreatePaymentAsync_WithValidRequest_ReturnsSuccess201AndPendingStatus()
    {
        Payment? capturedPayment = null;
        _orderRepository.OrderExistsAsync(Arg.Any<Guid>()).Returns(true);
        _paymentMethodRepository.PaymentMethodExistsAsync(Arg.Any<Guid>()).Returns(true);

        _paymentRepository
            .When(x => x.AddPaymentAsync(Arg.Any<Payment>()))
            .Do(call => capturedPayment = call.Arg<Payment>()!);

        _paymentRepository
            .AddPaymentAsync(Arg.Any<Payment>())
            .Returns(Task.FromResult(new Payment()));

        var request = PaymentBuilder.CreateRequest(amount: 150_000);
        var result = await _sut.CreatePaymentAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        capturedPayment.Should().NotBeNull();
        capturedPayment!.Status.Should().Be(PaymentStatus.Pending);
        await _paymentRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdatePaymentAsync_WhenNotFound_ReturnsFail404()
    {
        _paymentRepository.GetPaymentByIdAsync(Arg.Any<Guid>()).Returns((Payment?)null);

        var result = await _sut.UpdatePaymentAsync(Guid.NewGuid(), PaymentBuilder.UpdateRequest());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdatePaymentAsync_WhenPaymentExists_UpdatesStatusAndTransactionId()
    {
        var paymentId = Guid.NewGuid();
        var payment = PaymentBuilder.CreateEntity(id: paymentId, status: PaymentStatus.Pending);
        _paymentRepository.GetPaymentByIdAsync(paymentId).Returns(payment);

        var request = PaymentBuilder.UpdateRequest(status: PaymentStatus.Completed, transactionId: "TX-OK-1");
        var result = await _sut.UpdatePaymentAsync(paymentId, request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be("TX-OK-1");
        await _paymentRepository.Received(1).UpdatePaymentAsync(payment);
        await _paymentRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeletePaymentAsync_WhenDeleteFails_ReturnsFail500()
    {
        var paymentId = Guid.NewGuid();
        _paymentRepository.PaymentExistsAsync(paymentId).Returns(true);
        _paymentRepository.DeletePaymentAsync(paymentId).Returns(false);

        var result = await _sut.DeletePaymentAsync(paymentId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        await _paymentRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeletePaymentAsync_WhenPaymentNotFound_ReturnsFail404()
    {
        var paymentId = Guid.NewGuid();
        _paymentRepository.PaymentExistsAsync(paymentId).Returns(false);

        var result = await _sut.DeletePaymentAsync(paymentId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeletePaymentAsync_WhenDeleteSucceeds_ReturnsSuccess200()
    {
        var paymentId = Guid.NewGuid();
        _paymentRepository.PaymentExistsAsync(paymentId).Returns(true);
        _paymentRepository.DeletePaymentAsync(paymentId).Returns(true);

        var result = await _sut.DeletePaymentAsync(paymentId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _paymentRepository.Received(1).SaveChangesAsync();
    }
}