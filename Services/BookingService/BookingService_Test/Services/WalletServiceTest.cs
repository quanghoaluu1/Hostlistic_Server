using BookingService_Test.Helpers.TestDataBuilders;
using Microsoft.Extensions.Logging;

namespace BookingService_Test;

public class WalletServiceTest
{
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger<WalletService> _logger;
    private readonly WalletService _sut;

    public WalletServiceTest()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _logger = Substitute.For<ILogger<WalletService>>();
        _sut = new WalletService(_walletRepository, _logger);
    }

    [Fact]
    public async Task CreateWalletAsync_WhenUserAlreadyHasWallet_ReturnsFail400()
    {
        var request = WalletBuilder.CreateRequest();
        _walletRepository.GetWalletByUserIdAsync(request.UserId)
            .Returns(WalletBuilder.CreateEntity(userId: request.UserId));

        var result = await _sut.CreateWalletAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("already has a wallet");
        await _walletRepository.DidNotReceive().AddWalletAsync(Arg.Any<Wallet>());
    }

    [Fact]
    public async Task CreateWalletAsync_WithValidRequest_ReturnsSuccess201()
    {
        Wallet? capturedWallet = null;
        _walletRepository.GetWalletByUserIdAsync(Arg.Any<Guid>()).Returns((Wallet?)null);

        _walletRepository
            .When(x => x.AddWalletAsync(Arg.Any<Wallet>()))
            .Do(call => capturedWallet = call.Arg<Wallet>());

        _walletRepository
            .AddWalletAsync(Arg.Any<Wallet>())
            .Returns(Task.FromResult(new Wallet()));

        var request = WalletBuilder.CreateRequest();
        var result = await _sut.CreateWalletAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        capturedWallet.Should().NotBeNull();
        capturedWallet!.Status.Should().Be(WalletStatus.Active);
        capturedWallet.Balance.Should().Be(0);
        await _walletRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateWalletBalanceAsync_WhenInsufficientFundsForWithdraw_ReturnsFail400()
    {
        var walletId = Guid.NewGuid();
        var wallet = WalletBuilder.CreateEntity(id: walletId, balance: 50);
        _walletRepository.GetWalletByIdAsync(walletId).Returns(wallet);

        var request = WalletBuilder.BalanceRequest(amount: 100, transactionType: "Withdraw");
        var result = await _sut.UpdateWalletBalanceAsync(walletId, request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Insufficient balance");
    }

    [Fact]
    public async Task UpdateWalletBalanceAsync_WithInvalidTransactionType_ReturnsFail400()
    {
        var walletId = Guid.NewGuid();
        var wallet = WalletBuilder.CreateEntity(id: walletId, balance: 500);
        _walletRepository.GetWalletByIdAsync(walletId).Returns(wallet);

        var request = WalletBuilder.BalanceRequest(amount: 100, transactionType: "Transfer");
        var result = await _sut.UpdateWalletBalanceAsync(walletId, request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Invalid transaction type");
    }

    [Fact]
    public async Task DeleteWalletAsync_WhenWalletNotFound_ReturnsFail404()
    {
        var walletId = Guid.NewGuid();
        _walletRepository.DeleteWalletAsync(walletId).Returns(false);

        var result = await _sut.DeleteWalletAsync(walletId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}