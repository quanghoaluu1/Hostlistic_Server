using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class SettlementService(
    IEventSettlementRepository settlementRepository,
    IOrderRepository orderRepository,
    IOrderDetailRepository orderDetailRepository,
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository,
    IConfiguration configuration,
    IUserPlanServiceClient userPlanServiceClient,
    ILogger<SettlementService> logger
    ) : ISettlementService
{
    public async Task<ApiResponse<EventSettlementDto>> SettleEventAsync(Guid eventId, Guid organizerId)
    {
        try
        {
            var existingSettlement = await settlementRepository.GetByEventIdAsync(eventId);
            if (existingSettlement is not null && existingSettlement.Status == SettlementStatus.Settled)
            {
                logger.LogInformation("Event {EventId} already settled", eventId);
                return ApiResponse<EventSettlementDto>.Success(200, "Event already settled",
                    existingSettlement.Adapt<EventSettlementDto>());
            }

            var confirmedOrders = await orderRepository.GetConfirmedOrdersByEventIdAsync(eventId);
            if (!confirmedOrders.Any())
            {
                var noRevenueSettlement = new EventSettlement
                {
                    Id = Guid.CreateVersion7(),
                    EventId = eventId,
                    OrganizerId = organizerId,
                    GrossRevenue = 0,
                    PlatformFeePercent = 0,
                    PlatformFeeAmount = 0,
                    NetRevenue = 0,
                    TotalTicketsSold = 0,
                    TotalOrders = 0,
                    Status = SettlementStatus.NoRevenue,
                    SettledAt = DateTime.UtcNow
                };

                await settlementRepository.AddAsync(noRevenueSettlement);
                await settlementRepository.SaveChangesAsync();

                return ApiResponse<EventSettlementDto>.Success(200, "No revenue to settle",
                    noRevenueSettlement.Adapt<EventSettlementDto>());
            }
            var orderIds = confirmedOrders.Select(o => o.Id).ToList();
            var allOrderDetails = await orderDetailRepository.GetByOrderIds(orderIds);
            
            var grossRevenue = allOrderDetails.Sum(od => od.UnitPrice * od.Quantity);
            var totalTicketsSold = allOrderDetails.Sum(od => od.Quantity);
            
            var userPlan = await userPlanServiceClient.GetByUserIdAsync(organizerId);
            var feePercent = (decimal)userPlan.Single().SubscriptionPlan.CommissionRate;
            var platformFeeAmount = Math.Round(grossRevenue * feePercent / 100, 0); // VND không lẻ
            var netRevenue = grossRevenue - platformFeeAmount;
            
            var wallet = await walletRepository.GetWalletByUserIdAsync(organizerId);
            if (wallet is null)
            {
                wallet = new Wallet
                {
                    Id = Guid.CreateVersion7(),
                    UserId = organizerId,
                    Currency = "VND",
                    Balance = 0,
                    PendingBalance = 0,
                    Status = WalletStatus.Active
                };
                await walletRepository.AddWalletAsync(wallet);
            }


            wallet.Balance += netRevenue;
            var transaction = new Transaction
            {
                Id = Guid.CreateVersion7(),
                WalletId = wallet.Id,
                Type = TransactionType.EventRevenue,
                Amount = grossRevenue,
                PlatformFee = platformFeeAmount,
                NetAmount = netRevenue,
                BalanceAfter = wallet.Balance,
                ReferenceId = eventId,
                ReferenceType = "EventSettlement",
                Status = TransactionStatus.Completed,
                Description = $"Revenue from event. {totalTicketsSold} tickets, {confirmedOrders.Count()} orders. " +
                              $"Gross: {grossRevenue:N0} VND, Fee: {platformFeeAmount:N0} VND ({feePercent}%)"
            };
            var settlement = new EventSettlement
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                OrganizerId = organizerId,
                WalletId = wallet.Id,
                GrossRevenue = grossRevenue,
                PlatformFeePercent = feePercent,
                PlatformFeeAmount = platformFeeAmount,
                NetRevenue = netRevenue,
                TotalTicketsSold = totalTicketsSold,
                TotalOrders = confirmedOrders.Count(),
                Status = SettlementStatus.Settled,
                SettledAt = DateTime.UtcNow
            };

            await walletRepository.UpdateWalletAsync(wallet);
            await transactionRepository.AddAsync(transaction);
            await settlementRepository.AddAsync(settlement);
            await settlementRepository.SaveChangesAsync();
            logger.LogInformation(
                "Settlement completed for event {EventId}. Gross: {Gross}, Fee: {Fee}, Net: {Net}",
                eventId, grossRevenue, platformFeeAmount, netRevenue);

            return ApiResponse<EventSettlementDto>.Success(200, "Settlement completed",
                settlement.Adapt<EventSettlementDto>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Settlement failed for event {EventId}", eventId);
            return ApiResponse<EventSettlementDto>.Fail(500, $"Settlement failed: {ex.Message}");
        }
    }
}