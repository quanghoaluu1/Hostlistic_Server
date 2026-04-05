namespace BookingService_Test.Helpers.TestDataBuilders;

public static class PaymentBuilder
{
    public static Payment CreateEntity(
        Guid? id = null,
        Guid? orderId = null,
        Guid? paymentMethodId = null,
        decimal amount = 100_000,
        PaymentStatus status = PaymentStatus.Pending)
    {
        return new Payment
        {
            Id = id ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            PaymentMethodId = paymentMethodId ?? Guid.NewGuid(),
            Amount = amount,
            Gateway = "PAYOS",
            Status = status,
            TransactionId = "tx_001"
        };
    }

    public static CreatePaymentRequest CreateRequest(
        Guid? orderId = null,
        Guid? paymentMethodId = null,
        decimal amount = 100_000)
    {
        return new CreatePaymentRequest
        {
            OrderId = orderId ?? Guid.NewGuid(),
            PaymentMethodId = paymentMethodId ?? Guid.NewGuid(),
            Amount = amount,
            Gateway = "PAYOS"
        };
    }

    public static UpdatePaymentRequest UpdateRequest(
        PaymentStatus status = PaymentStatus.Completed,
        string? transactionId = "tx_updated")
    {
        return new UpdatePaymentRequest
        {
            Status = status,
            TransactionId = transactionId
        };
    }
}