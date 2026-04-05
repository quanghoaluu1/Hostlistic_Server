namespace BookingService_Test.Helpers.TestDataBuilders;

public static class PaymentMethodBuilder
{
    public static PaymentMethod CreateEntity(
        Guid? id = null,
        string name = "VNPay",
        string code = "VNPAY",
        bool isActive = true)
    {
        return new PaymentMethod
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Code = code,
            IconUrl = "https://cdn.example.com/vnpay.png",
            FeePercentage = 1.5m,
            FixedFee = 2_000,
            IsActive = isActive
        };
    }

    public static CreatePaymentMethodRequest CreateRequest(
        string name = "VNPay",
        string code = "VNPAY")
    {
        return new CreatePaymentMethodRequest
        {
            Name = name,
            Code = code,
            FeePercentage = 1.5m,
            FixedFee = 2_000
        };
    }

    public static GetPaymentOptionsRequest FreeOptionsRequest()
    {
        return new GetPaymentOptionsRequest
        {
            EventId = Guid.NewGuid(),
            TicketItems =
            [
                new TicketItemRequest
                {
                    TicketTypeId = Guid.NewGuid(),
                    TicketTypeName = "Community Pass",
                    Quantity = 1,
                    UnitPrice = 0
                }
            ]
        };
    }
}