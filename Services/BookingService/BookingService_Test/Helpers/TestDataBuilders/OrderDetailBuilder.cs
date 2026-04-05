namespace BookingService_Test.Helpers.TestDataBuilders;

public static class OrderDetailBuilder
{
    public static OrderDetail CreateEntity(
        Guid? id = null,
        Guid? orderId = null,
        Guid? ticketTypeId = null,
        int quantity = 2,
        decimal unitPrice = 100_000)
    {
        return new OrderDetail
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            TicketTypeId = ticketTypeId ?? Guid.NewGuid(),
            TicketTypeName = "General",
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}