using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common;
using Mapster;
using Microsoft.EntityFrameworkCore;
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
    IEventServiceClient eventServiceClient,
    ILogger<SettlementService> logger
    ) : ISettlementService
{
    public async Task<ApiResponse<List<UnsettledEventDto>>> GetPendingSettlementsAsync(CancellationToken ct = default)
    {
        var settleEventIds = await settlementRepository.GetEventIds();
        var orderQueryable = orderRepository.GetOrderQueryable();
        var unsettleEvents = await orderQueryable.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Confirmed && !settleEventIds.Contains(o.EventId))
            .GroupBy(o => new {o.EventId})
            .Select(g => new UnsettledEventDto
            {
                EventId = g.Key.EventId,
                OrganizerId = eventServiceClient.GetEventSettlementInfoAsync(g.Key.EventId).Result.OrganizerId,
                EventTitle = eventServiceClient.GetEventSettlementInfoAsync(g.Key.EventId).Result.Title,
                GrossRevenue = g.SelectMany(o => o.OrderDetails).Sum(od => od.UnitPrice * od.Quantity),
                TotalOrders = g.Count(),
                TotalTicketsSold = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity),
            })
            .ToListAsync(ct);
        var pendingSettlement = await settlementRepository.GetByStatusAsync(SettlementStatus.Pending);
        var result = pendingSettlement.Select(s => s.Adapt<EventSettlementDto>()).ToList();
        return ApiResponse<List<UnsettledEventDto>>.Success(200, "Pending settlements retrieved successfully", unsettleEvents);
    }

    public async Task<ApiResponse<List<EventSettlementDto>>> GetAllSettlementsAsync(CancellationToken ct = default)
    {
        var settlements = await settlementRepository.GetAllAsync();
        return ApiResponse<List<EventSettlementDto>>.Success(200, "Settlements retrieved successfully", settlements.Adapt<List<EventSettlementDto>>());
    }

    public async Task<ApiResponse<SettlementPreviewDto>> PreviewSettlementAsync(Guid eventId, Guid organizerId,
        CancellationToken ct = default)
    {
        var existingSettlement = await settlementRepository.GetByEventIdAsync(eventId);
        if (existingSettlement is not null && existingSettlement.Status == SettlementStatus.Settled)
        {
            logger.LogInformation("Event {EventId} already settled", eventId);
            return ApiResponse<SettlementPreviewDto>.Success(200, "Event already settled",
                existingSettlement.Adapt<SettlementPreviewDto>());
        }
        var orderQueryable = orderRepository.GetOrderQueryable();
        var orderData = await orderQueryable.AsNoTracking()
            .Where(o => o.EventId == eventId && o.Status == OrderStatus.Confirmed)
            .Select(o => new
            {
                Details = o.OrderDetails.Select(d => new {d.Quantity, d.UnitPrice}).ToList(),
            })
            .ToListAsync(ct);
        if (orderData.Count == 0)
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

            return ApiResponse<SettlementPreviewDto>.Success(200, "No revenue to settle",
                noRevenueSettlement.Adapt<SettlementPreviewDto>());
        }
        var grossRevenue = orderData.SelectMany(o => o.Details).Sum(d => d.Quantity * d.UnitPrice);
        var totalOrders = orderData.Count;
        var totalTickets = orderData.SelectMany(o => o.Details).Sum(d => d.Quantity);
        var userPlan = await userPlanServiceClient.GetByUserIdAsync(organizerId);
        var activePlan = userPlan.FirstOrDefault();
        if (activePlan?.SubscriptionPlan is null)
            return ApiResponse<SettlementPreviewDto>.Fail(400, 
                "Organizer has no active subscription plan");
        var feePercent = (decimal)activePlan.SubscriptionPlan.CommissionRate;
        var platformFeeAmount = Math.Round(grossRevenue * feePercent / 100, 0); // VND không lẻ
        var netRevenue = grossRevenue - platformFeeAmount;

        var eventTitle = eventServiceClient.GetEventSettlementInfoAsync(eventId).Result.Title;
        var preview = new SettlementPreviewDto(
            EventId: eventId,
            EventTitle: eventTitle,
            OrganizerId: organizerId,
            GrossRevenue: grossRevenue,
            PlatformFeePercent: feePercent,
            PlatformFeeAmount: platformFeeAmount,
            NetAmount: netRevenue,
            TotalOrders: totalOrders,
            TotalTicketsSold: totalTickets,
            AlreadySettled: false
        );
        return ApiResponse<SettlementPreviewDto>.Success(200, "Retrieve Settlement Preview",preview);

    }
    
    
    public async Task<ApiResponse<EventSettlementDto>> SettleEventAsync(Guid eventId, Guid organizerId, Guid adminId, string? notes = null, CancellationToken ct = default)
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
                    Notes = notes,
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
            var activePlan = userPlan.FirstOrDefault();
            if (activePlan?.SubscriptionPlan is null)
                return ApiResponse<EventSettlementDto>.Fail(400, 
                    "Organizer has no active subscription plan");
            var feePercent = (decimal)activePlan.SubscriptionPlan.CommissionRate;
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
            var evenTitle = eventServiceClient.GetEventSettlementInfoAsync(eventId).Result.Title;
            var settlement = new EventSettlement
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventTitle = evenTitle,
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
            return ApiResponse<EventSettlementDto>.Fail(500, $"Settlement failed. Please try again.");
        }
    }
    
    public async Task<ApiResponse<EventSettlementDto>> RejectSettlementAsync(
        Guid eventId, Guid adminId, string reason, CancellationToken ct = default)
    {
        var settlement = await settlementRepository.GetByEventIdAndStatusAsync(eventId, SettlementStatus.Pending);

        if (settlement is null)
        {
            // Create a rejection record if no pending settlement exists
            settlement = new EventSettlement
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                OrganizerId = Guid.Empty, // Resolve from EventService if needed
                Status = SettlementStatus.Rejected,
                RejectionReason = reason,
                Notes = reason,
                SettledByAdminId = adminId,
                SettledAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await settlementRepository.AddAsync(settlement);
        }
        else
        {
            settlement.Status = SettlementStatus.Rejected;
            settlement.RejectionReason = reason;
            settlement.SettledByAdminId = adminId;
            settlement.SettledAt = DateTime.UtcNow;
        }

        await settlementRepository.SaveChangesAsync();

        logger.LogInformation("Rejected settlement for event {EventId}: {Reason}", eventId, reason);

        return ApiResponse<EventSettlementDto>.Success(200, "Rejected",settlement.Adapt<EventSettlementDto>());
    }
}