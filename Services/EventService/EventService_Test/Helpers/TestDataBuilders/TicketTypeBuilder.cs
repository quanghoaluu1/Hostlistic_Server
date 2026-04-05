namespace EventService_Test.Helpers.TestDataBuilders;

public static class TicketTypeBuilder
{
    public static TicketType CreateEntity(
        Guid? id = null,
        Guid? eventId = null,
        string name = "General Admission",
        double price = 50.0,
        int quantityAvailable = 100,
        int quantitySold = 0)
    {
        return new TicketType
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            Name = name,
            Price = price,
            Description = "Standard ticket",
            QuantityAvailable = quantityAvailable,
            QuantitySold = quantitySold,
            SaleStartDate = DateTime.UtcNow.AddDays(-1),
            SaleEndTime = DateTime.UtcNow.AddDays(7),
            MinPerOrder = 1,
            MaxPerOrder = 10,
            IsRequireHolderInfo = false,
            Status = TicketTypeStatus.Active,
            SaleChannel = SaleChannel.OnlineOnly
        };
    }

    public static CreateTicketTypeRequest CreateRequest(
        string name = "VIP Ticket",
        double price = 100.0,
        int quantity = 50,
        int minPerOrder = 1,
        int maxPerOrder = 5) => new CreateTicketTypeRequest
    {
        EventId = Guid.NewGuid(),
        Name = name,
        Price = price,
        QuantityAvailable = quantity,
        SaleStartDate = DateTime.UtcNow.AddDays(-1),
        SaleEndTime = DateTime.UtcNow.AddDays(7),
        MinPerOrder = minPerOrder,
        MaxPerOrder = maxPerOrder,
        IsRequireHolderInfo = false,
        SaleChannel = SaleChannel.OnlineOnly
    };

    public static UpdateTicketTypeRequest UpdateRequest(
        string name = "Updated Ticket",
        double price = 75.0,
        int quantity = 80) => new UpdateTicketTypeRequest
    {
        Name = name,
        Price = price,
        QuantityAvailable = quantity,
        SaleStartDate = DateTime.UtcNow.AddDays(-1),
        SaleEndTime = DateTime.UtcNow.AddDays(7),
        MinPerOrder = 1,
        MaxPerOrder = 10,
        IsRequireHolderInfo = false,
        Status = TicketTypeStatus.Active,
        SaleChannel = SaleChannel.OnlineOnly
    };
}
