namespace BookingService_Test.Helpers.TestDataBuilders;

public static class PayoutRequestBuilder
{
    public static PayoutRequest CreateEntity(
        Guid? id = null,
        Guid? walletId = null,
        decimal amount = 100_000,
        PayoutRequestStatus status = PayoutRequestStatus.Pending)
    {
        return new PayoutRequest
        {
            Id = id ?? Guid.NewGuid(),
            OrganizerBankInfoId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            WalletId = walletId ?? Guid.NewGuid(),
            Amount = amount,
            Status = status,
            ProofImageUrl = "https://cdn.example.com/proof.png"
        };
    }

    public static CreatePayoutRequestRequest CreateRequest()
    {
        return new CreatePayoutRequestRequest
        {
            OrganizerBankInfoId = Guid.NewGuid(),
            EventId = Guid.NewGuid()
        };
    }

    public static UpdatePayoutRequestRequest UpdateRequest(PayoutRequestStatus status = PayoutRequestStatus.Rejected)
    {
        return new UpdatePayoutRequestRequest
        {
            Status = status,
            ProofImageUrl = "https://cdn.example.com/updated-proof.png"
        };
    }
}