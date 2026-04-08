namespace BookingService_Test.Helpers.TestDataBuilders;

public static class WalletBuilder
{
    public static Wallet CreateEntity(
        Guid? id = null,
        Guid? userId = null,
        decimal balance = 100_000,
        WalletStatus status = WalletStatus.Active,
        string currency = "VND")
    {
        return new Wallet
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Balance = balance,
            PendingBalance = 0,
            Status = status,
            Currency = currency
        };
    }

    public static CreateWalletRequest CreateRequest(Guid? userId = null, string currency = "VND")
    {
        return new CreateWalletRequest
        {
            UserId = userId ?? Guid.NewGuid(),
            Currency = currency
        };
    }

    public static UpdateWalletBalanceRequest BalanceRequest(decimal amount, string transactionType)
    {
        return new UpdateWalletBalanceRequest
        {
            Amount = amount,
            TransactionType = transactionType
        };
    }
}