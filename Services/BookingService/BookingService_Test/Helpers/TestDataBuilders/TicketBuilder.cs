namespace BookingService_Test.Helpers.TestDataBuilders;

public static class TicketBuilder
{
    public static Ticket CreateEntity(
        Guid? id = null,
        Guid? orderId = null,
        Guid? eventId = null,
        Guid? ticketTypeId = null,
        string code = "TICKET-001")
    {
        var resolvedEventId = eventId ?? Guid.NewGuid();

        return new Ticket
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            TicketTypeId = ticketTypeId ?? Guid.NewGuid(),
            TicketCode = code,
            TicketTypeName = "VIP",
            EventName = "Tech Conference",
            HolderName = "Holder",
            HolderEmail = "holder@test.com",
            HolderPhone = "0123456789",
            Order = new Order
            {
                Id = orderId ?? Guid.NewGuid(),
                EventId = resolvedEventId,
                UserId = Guid.NewGuid(),
                Status = OrderStatus.Pending
            }
        };
    }

    public static CreateTicketRequest CreateRequest(
        Guid? orderId = null,
        Guid? ticketTypeId = null,
        Guid? eventId = null)
    {
        return new CreateTicketRequest
        {
            OrderId = orderId ?? Guid.NewGuid(),
            TicketTypeId = ticketTypeId ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            TicketTypeName = "VIP",
            EventName = "Tech Conference",
            HolderName = "Holder",
            HolderEmail = "holder@test.com",
            HolderPhone = "0123456789"
        };
    }

    public static UpdateTicketRequest UpdateRequest(bool isUsed = true)
    {
        return new UpdateTicketRequest { IsUsed = isUsed };
    }
}