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
}