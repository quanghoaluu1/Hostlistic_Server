using BookingService_Test.Helpers.TestDataBuilders;

namespace BookingService_Test;

public class OrderDetailServiceTest
{
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly OrderDetailService _sut;

    public OrderDetailServiceTest()
    {
        _orderDetailRepository = Substitute.For<IOrderDetailRepository>();
        _sut = new OrderDetailService(_orderDetailRepository);
    }

    [Fact]
    public async Task GetOrderDetailByIdAsync_WhenNotFound_ReturnsFail404()
    {
        _orderDetailRepository.GetOrderDetailByIdAsync(Arg.Any<Guid>()).Returns((OrderDetail?)null);

        var result = await _sut.GetOrderDetailByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetOrderDetailByIdAsync_WhenFound_ReturnsSuccess200()
    {
        var orderDetail = OrderDetailBuilder.CreateEntity();
        _orderDetailRepository.GetOrderDetailByIdAsync(orderDetail.Id).Returns(orderDetail);

        var result = await _sut.GetOrderDetailByIdAsync(orderDetail.Id);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(orderDetail.Id);
    }

    [Fact]
    public async Task GetOrderDetailsByOrderIdAsync_ReturnsMappedCollection()
    {
        var orderId = Guid.NewGuid();
        var orderDetails = new List<OrderDetail>
        {
            OrderDetailBuilder.CreateEntity(orderId: orderId),
            OrderDetailBuilder.CreateEntity(orderId: orderId)
        };
        _orderDetailRepository.GetOrderDetailsByOrderIdAsync(orderId).Returns(orderDetails);

        var result = await _sut.GetOrderDetailsByOrderIdAsync(orderId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }
}