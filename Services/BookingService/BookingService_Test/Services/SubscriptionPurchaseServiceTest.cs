using Microsoft.Extensions.Logging;

namespace BookingService_Test;

public class SubscriptionPurchaseServiceTest
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserPlanServiceClient _userPlanServiceClient;
    private readonly ILogger<SubscriptionPurchaseService> _logger;
    private readonly SubscriptionPurchaseService _sut;

    public SubscriptionPurchaseServiceTest()
    {
        _walletRepository = Substitute.For<IWalletRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _userPlanServiceClient = Substitute.For<IUserPlanServiceClient>();
        _logger = Substitute.For<ILogger<SubscriptionPurchaseService>>();

        _sut = new SubscriptionPurchaseService(
            _walletRepository,
            _transactionRepository,
            _userPlanServiceClient,
            _logger);
    }

    [Fact]
    public async Task PurchaseWithWalletAsync_WhenIdsMissing_ReturnsFail400()
    {
        var result = await _sut.PurchaseWithWalletAsync(new PurchaseSubscriptionWithWalletRequest
        {
            UserId = Guid.Empty,
            SubscriptionPlanId = Guid.Empty
        });

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task PurchaseWithWalletAsync_WhenPlanInactive_ReturnsFail404()
    {
        var request = new PurchaseSubscriptionWithWalletRequest
        {
            UserId = Guid.NewGuid(),
            SubscriptionPlanId = Guid.NewGuid()
        };

        _userPlanServiceClient.GetSubscriptionPlanByIdAsync(request.SubscriptionPlanId)
            .Returns(new SubscriptionPlanDto { Id = request.SubscriptionPlanId, Name = "Pro", IsActive = false });

        var result = await _sut.PurchaseWithWalletAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task PurchaseWithWalletAsync_WhenUserAlreadyHasSameActivePlan_ReturnsFail400()
    {
        var request = new PurchaseSubscriptionWithWalletRequest
        {
            UserId = Guid.NewGuid(),
            SubscriptionPlanId = Guid.NewGuid()
        };

        _userPlanServiceClient.GetSubscriptionPlanByIdAsync(request.SubscriptionPlanId)
            .Returns(new SubscriptionPlanDto
            {
                Id = request.SubscriptionPlanId,
                Name = "Pro",
                IsActive = true,
                Price = 100_000,
                DurationInDays = 30
            });

        _userPlanServiceClient.GetByUserIdAsync(request.UserId, true)
            .Returns(
            [
                new UserPlanDto
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    SubscriptionPlanId = request.SubscriptionPlanId,
                    IsActive = true
                }
            ]);

        var result = await _sut.PurchaseWithWalletAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("already has this plan active");
    }

    [Fact]
    public async Task PurchaseWithWalletAsync_WhenInsufficientBalance_ReturnsFail400()
    {
        var request = new PurchaseSubscriptionWithWalletRequest
        {
            UserId = Guid.NewGuid(),
            SubscriptionPlanId = Guid.NewGuid()
        };

        _userPlanServiceClient.GetSubscriptionPlanByIdAsync(request.SubscriptionPlanId)
            .Returns(new SubscriptionPlanDto
            {
                Id = request.SubscriptionPlanId,
                Name = "Pro",
                IsActive = true,
                Price = 200_000,
                DurationInDays = 30
            });

        _userPlanServiceClient.GetByUserIdAsync(request.UserId, true).Returns([]);

        _walletRepository.GetWalletByUserIdAsync(request.UserId)
            .Returns(new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Balance = 100_000,
                Status = WalletStatus.Active
            });

        var result = await _sut.PurchaseWithWalletAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Insufficient wallet balance");
    }

    [Fact]
    public async Task PurchaseWithWalletAsync_WhenCreateUserPlanFails_RefundsAndReturnsFail502()
    {
        var request = new PurchaseSubscriptionWithWalletRequest
        {
            UserId = Guid.NewGuid(),
            SubscriptionPlanId = Guid.NewGuid()
        };

        var plan = new SubscriptionPlanDto
        {
            Id = request.SubscriptionPlanId,
            Name = "Pro",
            IsActive = true,
            Price = 120_000,
            DurationInDays = 30
        };

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Balance = 500_000,
            Status = WalletStatus.Active
        };

        _userPlanServiceClient.GetSubscriptionPlanByIdAsync(request.SubscriptionPlanId).Returns(plan);
        _userPlanServiceClient.GetByUserIdAsync(request.UserId, true).Returns([]);
        _walletRepository.GetWalletByUserIdAsync(request.UserId).Returns(wallet);
        _userPlanServiceClient.CreateUserPlanAsync(Arg.Any<CreateUserPlanRequest>()).Returns((UserPlanDto?)null);

        var result = await _sut.PurchaseWithWalletAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(502);
        wallet.Balance.Should().Be(500_000);
        await _transactionRepository.Received(2).AddAsync(Arg.Any<Transaction>());
        await _walletRepository.Received(2).SaveChangesAsync();
    }

    [Fact]
    public async Task PurchaseWithWalletAsync_WithValidFlow_ReturnsSuccessAndCancelsOldActivePlans()
    {
        var request = new PurchaseSubscriptionWithWalletRequest
        {
            UserId = Guid.NewGuid(),
            SubscriptionPlanId = Guid.NewGuid()
        };

        var oldPlanId = Guid.NewGuid();
        var newUserPlanId = Guid.NewGuid();

        _userPlanServiceClient.GetSubscriptionPlanByIdAsync(request.SubscriptionPlanId)
            .Returns(new SubscriptionPlanDto
            {
                Id = request.SubscriptionPlanId,
                Name = "Pro",
                IsActive = true,
                Price = 100_000,
                DurationInDays = 30
            });

        _userPlanServiceClient.GetByUserIdAsync(request.UserId, true)
            .Returns(
            [
                new UserPlanDto
                {
                    Id = oldPlanId,
                    UserId = request.UserId,
                    SubscriptionPlanId = Guid.NewGuid(),
                    IsActive = true
                }
            ]);

        _walletRepository.GetWalletByUserIdAsync(request.UserId)
            .Returns(new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Balance = 300_000,
                Status = WalletStatus.Active
            });

        _userPlanServiceClient.CreateUserPlanAsync(Arg.Any<CreateUserPlanRequest>())
            .Returns(new UserPlanDto
            {
                Id = newUserPlanId,
                UserId = request.UserId,
                SubscriptionPlanId = request.SubscriptionPlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            });

        _userPlanServiceClient.CancelUserPlanAsync(oldPlanId).Returns(true);

        var result = await _sut.PurchaseWithWalletAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.UserPlanId.Should().Be(newUserPlanId);
        await _userPlanServiceClient.Received(1).CancelUserPlanAsync(oldPlanId);
    }
}