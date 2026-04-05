using BookingService_Test.Helpers.TestDataBuilders;

namespace BookingService_Test;

public class OrderServiceTest
{
    private readonly IOrderRepository _orderRepository;
    private readonly OrderService _sut;

    public OrderServiceTest()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _sut = new OrderService(_orderRepository);
    }

    [Fact]
    public async Task CreateOrderAsync_WithoutOrderDetails_ReturnsFail400()
    {
        var request = OrderBuilder.CreateRequest(details: []);

        var result = await _sut.CreateOrderAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("at least one order detail");
        await _orderRepository.DidNotReceive().AddOrderAsync(Arg.Any<Order>());
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_ReturnsSuccess201AndSetsPendingStatus()
    {
        Order? capturedOrder = null;
        _orderRepository
            .When(x => x.AddOrderAsync(Arg.Any<Order>()))
            .Do(call => capturedOrder = call.Arg<Order>());

        _orderRepository
            .AddOrderAsync(Arg.Any<Order>())
            .Returns(Task.FromResult(new Order()));

        var request = OrderBuilder.CreateRequest();

        var result = await _sut.CreateOrderAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Status.Should().Be(OrderStatus.Pending);
        capturedOrder.OrderDetails.Should().HaveCount(1);
        capturedOrder.OrderDetails.First().TicketTypeName.Should().Be("General");
        await _orderRepository.Received(1).AddOrderAsync(Arg.Any<Order>());
        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenNotFound_ReturnsFail404()
    {
        _orderRepository.GetOrderByIdAsync(Arg.Any<Guid>()).Returns((Order?)null);

        var result = await _sut.GetOrderByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenFound_ReturnsSuccess200()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetOrderByIdAsync(orderId).Returns(OrderBuilder.CreateEntity(id: orderId));

        var result = await _sut.GetOrderByIdAsync(orderId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrdersByEventIdAsync_ReturnsSuccess200WithCollection()
    {
        var eventId = Guid.NewGuid();
        _orderRepository.GetOrdersByEventIdAsync(eventId).Returns(
        [
            OrderBuilder.CreateEntity(eventId: eventId),
            OrderBuilder.CreateEntity(eventId: eventId)
        ]);

        var result = await _sut.GetOrdersByEventIdAsync(eventId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrdersByUserIdAsync_ReturnsSuccess200WithCollection()
    {
        var userId = Guid.NewGuid();
        _orderRepository.GetOrdersByUserIdAsync(userId).Returns(
        [
            OrderBuilder.CreateEntity(userId: userId),
            OrderBuilder.CreateEntity(userId: userId)
        ]);

        var result = await _sut.GetOrdersByUserIdAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrderByPayOsCodeAsync_WhenNotFound_ReturnsFail404()
    {
        _orderRepository.GetOrderByOrderCodeAsync(Arg.Any<long>()).Returns((Order?)null);

        var result = await _sut.GetOrderByPayOsCodeAsync(123456789);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetOrderByPayOsCodeAsync_WhenFound_ReturnsSuccess200()
    {
        var orderCode = 456789;
        _orderRepository.GetOrderByOrderCodeAsync(orderCode)
            .Returns(OrderBuilder.CreateEntity(orderCode: orderCode));

        var result = await _sut.GetOrderByPayOsCodeAsync(orderCode);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateOrderAsync_WhenNotFound_ReturnsFail404()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetOrderByIdAsync(orderId).Returns((Order?)null);

        var result = await _sut.UpdateOrderAsync(orderId, OrderBuilder.UpdateRequest());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateOrderAsync_WhenOrderExists_UpdatesStatusNotesAndOrderCode()
    {
        var orderId = Guid.NewGuid();
        var order = OrderBuilder.CreateEntity(id: orderId, status: OrderStatus.Pending, orderCode: null);
        _orderRepository.GetOrderByIdAsync(orderId).Returns(order);

        var request = OrderBuilder.UpdateRequest(
            status: OrderStatus.Confirmed,
            notes: "Confirmed by system",
            orderCode: 999888);

        var result = await _sut.UpdateOrderAsync(orderId, request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.Notes.Should().Be("Confirmed by system");
        order.OrderCode.Should().Be(999888);
        await _orderRepository.Received(1).UpdateOrderAsync(order);
        await _orderRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenDeleteFails_ReturnsFail500()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.OrderExistsAsync(orderId).Returns(true);
        _orderRepository.DeleteOrderAsync(orderId).Returns(false);

        var result = await _sut.DeleteOrderAsync(orderId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        await _orderRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenOrderNotFound_ReturnsFail404()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.OrderExistsAsync(orderId).Returns(false);

        var result = await _sut.DeleteOrderAsync(orderId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteOrderAsync_WhenDeleteSucceeds_ReturnsSuccess200()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.OrderExistsAsync(orderId).Returns(true);
        _orderRepository.DeleteOrderAsync(orderId).Returns(true);

        var result = await _sut.DeleteOrderAsync(orderId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _orderRepository.Received(1).SaveChangesAsync();
    }
}