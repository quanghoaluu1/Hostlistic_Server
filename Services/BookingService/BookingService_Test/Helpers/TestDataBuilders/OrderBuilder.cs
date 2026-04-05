namespace BookingService_Test.Helpers.TestDataBuilders;

public static class OrderBuilder
{
    public static Order CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        Guid? userId = null,
        OrderStatus status = OrderStatus.Pending,
        long? orderCode = null,
        List<OrderDetail>? orderDetails = null)
    {
        return new Order
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Status = status,
            OrderCode = orderCode,
            Notes = "Test order",
            BuyerName = "Test Buyer",
            BuyerEmail = "buyer@test.com",
            OrderDetails = orderDetails ?? new List<OrderDetail>()
        };
    }

    public static CreateOrderRequest CreateRequest(
        Guid? eventId = null,
        Guid? userId = null,
        List<CreateOrderDetailRequest>? details = null)
    {
        return new CreateOrderRequest
        {
            EventId = eventId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Notes = "Please process quickly",
            BuyerName = "Test Buyer",
            BuyerEmail = "buyer@test.com",
            OrderDetails = details ??
            [
                new CreateOrderDetailRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "General",
                    Quantity = 2,
                    UnitPrice = 100_000
                }
            ]
        };
    }

    public static UpdateOrderRequest UpdateRequest(
        OrderStatus status = OrderStatus.Confirmed,
        string notes = "Updated notes",
        long? orderCode = 123456)
    {
        return new UpdateOrderRequest
        {
            Status = status,
            Notes = notes,
            OrderCode = orderCode
        };
    }
}