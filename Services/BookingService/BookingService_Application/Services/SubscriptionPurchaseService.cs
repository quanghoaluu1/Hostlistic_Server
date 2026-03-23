using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class SubscriptionPurchaseService(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository,
    IUserPlanServiceClient userPlanServiceClient,
    ILogger<SubscriptionPurchaseService> logger
) : ISubscriptionPurchaseService
{
    public async Task<ApiResponse<PurchaseSubscriptionWithWalletResponse>> PurchaseWithWalletAsync(
        PurchaseSubscriptionWithWalletRequest request)
    {
        if (request.UserId == Guid.Empty || request.SubscriptionPlanId == Guid.Empty)
            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(400, "UserId and SubscriptionPlanId are required.");

        var plan = await userPlanServiceClient.GetSubscriptionPlanByIdAsync(request.SubscriptionPlanId);
        if (plan is null || !plan.IsActive)
            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(404, "Subscription plan not found or inactive.");

        var currentActivePlans = (await userPlanServiceClient.GetByUserIdAsync(request.UserId, true)).ToList();
        if (currentActivePlans.Any(x => x.SubscriptionPlanId == request.SubscriptionPlanId))
            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(400, "User already has this plan active.");

        var wallet = await walletRepository.GetWalletByUserIdAsync(request.UserId);
        if (wallet is null)
            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(404, "Wallet not found for this user.");

        if (wallet.Status != WalletStatus.Active)
            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(400, "Wallet is not active.");

        if (wallet.Balance < plan.Price)
            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(400, "Insufficient wallet balance.");

        wallet.Balance -= plan.Price;

        var debitTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            WalletId = wallet.Id,
            Type = TransactionType.SubscriptionPurchase,
            Amount = plan.Price,
            PlatformFee = 0,
            NetAmount = plan.Price,
            BalanceAfter = wallet.Balance,
            ReferenceId = plan.Id,
            ReferenceType = "SubscriptionPlan",
            Status = TransactionStatus.Completed,
            Description = $"Wallet charge for subscription plan {plan.Name}"
        };

        await walletRepository.UpdateWalletAsync(wallet);
        await transactionRepository.AddAsync(debitTransaction);
        await walletRepository.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var endDate = now.AddMonths(Math.Max(1, plan.DurationInMonths));

        var createdUserPlan = await userPlanServiceClient.CreateUserPlanAsync(new CreateUserPlanRequest
        {
            UserId = request.UserId,
            SubscriptionPlanId = plan.Id,
            StartDate = now,
            EndDate = endDate
        });

        if (createdUserPlan is null)
        {
            wallet.Balance += plan.Price;

            var reverseTransaction = new Transaction
            {
                Id = Guid.CreateVersion7(),
                WalletId = wallet.Id,
                Type = TransactionType.Refund,
                Amount = plan.Price,
                PlatformFee = 0,
                NetAmount = plan.Price,
                BalanceAfter = wallet.Balance,
                ReferenceId = plan.Id,
                ReferenceType = "SubscriptionPlan",
                Status = TransactionStatus.Reversed,
                Description = $"Wallet refund because subscription activation failed for plan {plan.Name}"
            };

            await walletRepository.UpdateWalletAsync(wallet);
            await transactionRepository.AddAsync(reverseTransaction);
            await walletRepository.SaveChangesAsync();

            logger.LogWarning(
                "Subscription activation failed after wallet debit for user {UserId}, plan {PlanId}. Wallet amount refunded.",
                request.UserId, request.SubscriptionPlanId);

            return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Fail(502,
                "Failed to activate subscription after charging wallet. Wallet amount has been refunded.");
        }

        // Keep a single active plan for user.
        foreach (var active in currentActivePlans.Where(x => x.Id != createdUserPlan.Id))
        {
            await userPlanServiceClient.CancelUserPlanAsync(active.Id);
        }

        var response = new PurchaseSubscriptionWithWalletResponse
        {
            UserPlanId = createdUserPlan.Id,
            WalletId = wallet.Id,
            SubscriptionPlanId = plan.Id,
            SubscriptionPlanName = plan.Name,
            ChargedAmount = plan.Price,
            WalletBalanceAfter = wallet.Balance,
            StartDate = createdUserPlan.StartDate,
            EndDate = createdUserPlan.EndDate
        };

        return ApiResponse<PurchaseSubscriptionWithWalletResponse>.Success(200,
            "Subscription purchased successfully with wallet balance.", response);
    }
}
