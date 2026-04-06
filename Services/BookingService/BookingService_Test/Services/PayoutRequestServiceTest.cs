using BookingService_Test.Helpers.TestDataBuilders;

namespace BookingService_Test;

public class PayoutRequestServiceTest
{
    private readonly IPayoutRequestRepository _payoutRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPhotoService _photoService;
    private readonly PayoutRequestService _sut;

    public PayoutRequestServiceTest()
    {
        _payoutRequestRepository = Substitute.For<IPayoutRequestRepository>();
        _walletRepository = Substitute.For<IWalletRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _photoService = Substitute.For<IPhotoService>();
        _sut = new PayoutRequestService(
            _payoutRequestRepository,
            _walletRepository,
            _transactionRepository,
            _photoService);
    }

    [Fact]
    public async Task GetPayoutRequestByIdAsync_WhenNotFound_ReturnsFail404()
    {
        _payoutRequestRepository.GetPayoutRequestByIdAsync(Arg.Any<Guid>()).Returns((PayoutRequest?)null);

        var result = await _sut.GetPayoutRequestByIdAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreatePayoutRequestWithProofAsync_WithoutProofFile_ReturnsSuccess201()
    {
        var request = PayoutRequestBuilder.CreateRequest();

        var result = await _sut.CreatePayoutRequestWithProofAsync(request, proofFile: null);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        await _payoutRequestRepository.Received(1).AddPayoutRequestAsync(Arg.Any<PayoutRequest>());
        await _payoutRequestRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdatePayoutRequestWithProofAsync_WhenNotFound_ReturnsFail404()
    {
        _payoutRequestRepository.GetPayoutRequestByIdAsync(Arg.Any<Guid>())
            .Returns((PayoutRequest?)null);

        var result = await _sut.UpdatePayoutRequestWithProofAsync(
            Guid.NewGuid(),
            PayoutRequestBuilder.UpdateRequest(),
            proofFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ApprovePayoutAsync_WhenPayoutNotPending_ReturnsFail400()
    {
        var payoutId = Guid.NewGuid();
        var payout = PayoutRequestBuilder.CreateEntity(id: payoutId, status: PayoutRequestStatus.Approved);
        _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutId).Returns(payout);

        var result = await _sut.ApprovePayoutAsync(payoutId, proofFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("not pending");
    }

    [Fact]
    public async Task ApprovePayoutAsync_WhenWalletNotFound_ReturnsFail404()
    {
        var payoutId = Guid.NewGuid();
        var payout = PayoutRequestBuilder.CreateEntity(id: payoutId, status: PayoutRequestStatus.Pending);
        _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutId).Returns(payout);
        _walletRepository.GetWalletByIdAsync(payout.WalletId).Returns((Wallet?)null);

        var result = await _sut.ApprovePayoutAsync(payoutId, proofFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Wallet not found");
    }

    [Fact]
    public async Task ApprovePayoutAsync_WhenBalanceInsufficient_ReturnsFail400()
    {
        var payoutId = Guid.NewGuid();
        var payout = PayoutRequestBuilder.CreateEntity(id: payoutId, amount: 1_000, status: PayoutRequestStatus.Pending);
        var wallet = WalletBuilder.CreateEntity(id: payout.WalletId, balance: 500);

        _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutId).Returns(payout);
        _walletRepository.GetWalletByIdAsync(payout.WalletId).Returns(wallet);

        var result = await _sut.ApprovePayoutAsync(payoutId, proofFile: null);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Insufficient wallet balance");
    }

    [Fact]
    public async Task ApprovePayoutAsync_WithValidData_DebitsWalletAndCompletesTransaction()
    {
        var payoutId = Guid.NewGuid();
        var payout = PayoutRequestBuilder.CreateEntity(id: payoutId, amount: 1_000, status: PayoutRequestStatus.Pending);
        var wallet = WalletBuilder.CreateEntity(id: payout.WalletId, balance: 5_000);

        _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutId).Returns(payout);
        _walletRepository.GetWalletByIdAsync(payout.WalletId).Returns(wallet);

        var result = await _sut.ApprovePayoutAsync(payoutId, proofFile: null);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        payout.Status.Should().Be(PayoutRequestStatus.Approved);
        payout.ProcessedAt.Should().NotBeNull();
        wallet.Balance.Should().Be(4_000);

        await _walletRepository.Received(1).UpdateWalletAsync(wallet);
        await _transactionRepository.Received(1).AddAsync(
            Arg.Is<Transaction>(t =>
                t!.WalletId == wallet.Id &&
                t.Type == TransactionType.Payout &&
                t.Status == TransactionStatus.Completed &&
                t.Amount == payout.Amount));
        await _payoutRequestRepository.Received(1).UpdatePayoutRequestAsync(payout);
        await _payoutRequestRepository.Received(1).SaveChangesAsync();
    }
}