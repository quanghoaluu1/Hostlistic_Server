// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
//
// namespace BookingService_Test;
//
// public class SettlementServiceTest
// {
//     private readonly IEventSettlementRepository _settlementRepository;
//     private readonly IOrderRepository _orderRepository;
//     private readonly IOrderDetailRepository _orderDetailRepository;
//     private readonly IWalletRepository _walletRepository;
//     private readonly ITransactionRepository _transactionRepository;
//     private readonly IConfiguration _configuration;
//     private readonly IUserPlanServiceClient _userPlanServiceClient;
//     private readonly ILogger<SettlementService> _logger;
//     private readonly SettlementService _sut;
//
//     public SettlementServiceTest()
//     {
//         _settlementRepository = Substitute.For<IEventSettlementRepository>();
//         _orderRepository = Substitute.For<IOrderRepository>();
//         _orderDetailRepository = Substitute.For<IOrderDetailRepository>();
//         _walletRepository = Substitute.For<IWalletRepository>();
//         _transactionRepository = Substitute.For<ITransactionRepository>();
//         _configuration = Substitute.For<IConfiguration>();
//         _userPlanServiceClient = Substitute.For<IUserPlanServiceClient>();
//         _logger = Substitute.For<ILogger<SettlementService>>();
//
//         _sut = new SettlementService(
//             _settlementRepository,
//             _orderRepository,
//             _orderDetailRepository,
//             _walletRepository,
//             _transactionRepository,
//             _configuration,
//             _userPlanServiceClient,
//             _logger);
//     }
//
//     [Fact]
//     public async Task SettleEventAsync_WhenAlreadySettled_ReturnsSuccess200AndSkipsProcessing()
//     {
//         var eventId = Guid.NewGuid();
//         var organizerId = Guid.NewGuid();
//
//         _settlementRepository.GetByEventIdAsync(eventId)
//             .Returns(new EventSettlement
//             {
//                 Id = Guid.NewGuid(),
//                 EventId = eventId,
//                 OrganizerId = organizerId,
//                 Status = SettlementStatus.Settled,
//                 GrossRevenue = 1_000_000,
//                 NetRevenue = 950_000
//             });
//
//         var result = await _sut.SettleEventAsync(eventId, organizerId);
//
//         result.IsSuccess.Should().BeTrue();
//         result.StatusCode.Should().Be(200);
//         result.Message.Should().Contain("already settled");
//         await _orderRepository.DidNotReceive().GetConfirmedOrdersByEventIdAsync(Arg.Any<Guid>());
//     }
//
//     [Fact]
//     public async Task SettleEventAsync_WhenNoConfirmedOrders_CreatesNoRevenueSettlement()
//     {
//         var eventId = Guid.NewGuid();
//         var organizerId = Guid.NewGuid();
//
//         _settlementRepository.GetByEventIdAsync(eventId).Returns((EventSettlement?)null);
//         _orderRepository.GetConfirmedOrdersByEventIdAsync(eventId).Returns([]);
//
//         var result = await _sut.SettleEventAsync(eventId, organizerId);
//
//         result.IsSuccess.Should().BeTrue();
//         result.StatusCode.Should().Be(200);
//         result.Message.Should().Contain("No revenue");
//
//         await _settlementRepository.Received(1).AddAsync(
//             Arg.Is<EventSettlement>(s =>
//                 s!.EventId == eventId &&
//                 s.OrganizerId == organizerId &&
//                 s.Status == SettlementStatus.NoRevenue &&
//                 s.NetRevenue == 0));
//         await _settlementRepository.Received(1).SaveChangesAsync();
//     }
//
//     [Fact]
//     public async Task SettleEventAsync_WithRevenueAndExistingWallet_CalculatesAndPersistsSettlement()
//     {
//         var eventId = Guid.NewGuid();
//         var organizerId = Guid.NewGuid();
//         var walletId = Guid.NewGuid();
//         var order1 = Guid.NewGuid();
//         var order2 = Guid.NewGuid();
//
//         _settlementRepository.GetByEventIdAsync(eventId).Returns((EventSettlement?)null);
//
//         _orderRepository.GetConfirmedOrdersByEventIdAsync(eventId).Returns(
//         [
//             new Order { Id = order1, EventId = eventId, UserId = organizerId, Status = OrderStatus.Confirmed },
//             new Order { Id = order2, EventId = eventId, UserId = organizerId, Status = OrderStatus.Confirmed }
//         ]);
//
//         _orderDetailRepository.GetByOrderIds(Arg.Any<List<Guid>>()).Returns(
//         [
//             new OrderDetail { Id = Guid.NewGuid(), OrderId = order1, Quantity = 2, UnitPrice = 100_000, TicketTypeId = Guid.NewGuid(), TicketTypeName = "General" },
//             new OrderDetail { Id = Guid.NewGuid(), OrderId = order2, Quantity = 1, UnitPrice = 300_000, TicketTypeId = Guid.NewGuid(), TicketTypeName = "VIP" }
//         ]);
//
//         _userPlanServiceClient.GetByUserIdAsync(organizerId).Returns(
//         [
//             new UserPlanDto
//             {
//                 Id = Guid.NewGuid(),
//                 UserId = organizerId,
//                 SubscriptionPlanId = Guid.NewGuid(),
//                 IsActive = true,
//                 SubscriptionPlan = new SubscriptionPlanDto
//                 {
//                     Id = Guid.NewGuid(),
//                     Name = "Pro",
//                     CommissionRate = 10,
//                     IsActive = true
//                 }
//             }
//         ]);
//
//         var wallet = new Wallet
//         {
//             Id = walletId,
//             UserId = organizerId,
//             Balance = 50_000,
//             Status = WalletStatus.Active,
//             Currency = "VND"
//         };
//         _walletRepository.GetWalletByUserIdAsync(organizerId).Returns(wallet);
//
//         var result = await _sut.SettleEventAsync(eventId, organizerId);
//
//         // gross = 2*100000 + 1*300000 = 500000
//         // fee 10% => 50000, net => 450000
//         result.IsSuccess.Should().BeTrue();
//         result.StatusCode.Should().Be(200);
//         wallet.Balance.Should().Be(500_000);
//
//         await _walletRepository.Received(1).UpdateWalletAsync(wallet);
//         await _transactionRepository.Received(1).AddAsync(
//             Arg.Is<Transaction>(t =>
//                 t!.WalletId == walletId &&
//                 t.Amount == 500_000 &&
//                 t.PlatformFee == 50_000 &&
//                 t.NetAmount == 450_000 &&
//                 t.Type == TransactionType.EventRevenue &&
//                 t.Status == TransactionStatus.Completed));
//         await _settlementRepository.Received(1).AddAsync(
//             Arg.Is<EventSettlement>(s =>
//                 s!.EventId == eventId &&
//                 s.OrganizerId == organizerId &&
//                 s.GrossRevenue == 500_000 &&
//                 s.PlatformFeeAmount == 50_000 &&
//                 s.NetRevenue == 450_000 &&
//                 s.TotalTicketsSold == 3 &&
//                 s.TotalOrders == 2 &&
//                 s.Status == SettlementStatus.Settled));
//         await _settlementRepository.Received(1).SaveChangesAsync();
//     }
//
//     [Fact]
//     public async Task SettleEventAsync_WhenWalletMissing_CreatesWalletThenSettles()
//     {
//         var eventId = Guid.NewGuid();
//         var organizerId = Guid.NewGuid();
//
//         _settlementRepository.GetByEventIdAsync(eventId).Returns((EventSettlement?)null);
//         _orderRepository.GetConfirmedOrdersByEventIdAsync(eventId).Returns(
//         [
//             new Order { Id = Guid.NewGuid(), EventId = eventId, UserId = organizerId, Status = OrderStatus.Confirmed }
//         ]);
//         _orderDetailRepository.GetByOrderIds(Arg.Any<List<Guid>>()).Returns(
//         [
//             new OrderDetail { Id = Guid.NewGuid(), Quantity = 1, UnitPrice = 100_000, TicketTypeId = Guid.NewGuid(), TicketTypeName = "General" }
//         ]);
//         _userPlanServiceClient.GetByUserIdAsync(organizerId).Returns(
//         [
//             new UserPlanDto
//             {
//                 Id = Guid.NewGuid(),
//                 UserId = organizerId,
//                 SubscriptionPlanId = Guid.NewGuid(),
//                 IsActive = true,
//                 SubscriptionPlan = new SubscriptionPlanDto { CommissionRate = 0, IsActive = true, Name = "Free" }
//             }
//         ]);
//         _walletRepository.GetWalletByUserIdAsync(organizerId).Returns((Wallet?)null);
//
//         var result = await _sut.SettleEventAsync(eventId, organizerId);
//
//         result.IsSuccess.Should().BeTrue();
//         await _walletRepository.Received(1).AddWalletAsync(
//             Arg.Is<Wallet>(w => w!.UserId == organizerId && w.Status == WalletStatus.Active));
//         await _walletRepository.Received(1).UpdateWalletAsync(Arg.Any<Wallet>());
//     }
// }