using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class WithdrawalRequestService(
    IWithdrawalRequestRepository withdrawalRequestRepository,
    ITransactionRepository transactionRepository,
    IWalletRepository walletRepository,
    IUserServiceClient userServiceClient,
    IPayOsService payOsService,
    ILogger<WithdrawalRequestService> logger
    ) : IWithdrawalRequestService
{
    private const decimal MinimumWithdrawal = 2_000m;

    public async Task<ApiResponse<WithdrawalDto>> CreateRequestAsync(Guid userId, CreateWithdrawalRequest request,
        CancellationToken ct = default)
    {
        if (request.Amount < MinimumWithdrawal)
            return ApiResponse<WithdrawalDto>.Fail(400, $"Minimum withdrawal amount is {MinimumWithdrawal:N0} VND.");
        
        var wallet = await walletRepository.GetWalletByUserIdAsync(userId);
        
        if (wallet is null)
            return ApiResponse<WithdrawalDto>.Fail(400,"Wallet not found.");
        if (wallet.Balance < request.Amount)
            return ApiResponse<WithdrawalDto>.Fail(
                400,$"Insufficient balance. Available: {wallet.Balance:N0} VND.");
        
        var hasPending = await withdrawalRequestRepository.IsUserHasPendingWithdrawalRequest(userId);
        if (hasPending)
            return ApiResponse<WithdrawalDto>.Fail(409,
                "You already have a pending withdrawal request. Please wait for it to be processed.");
        
        var bankInfo = await userServiceClient.GetOrganizerBankInfoAsync(userId);
        if (bankInfo is null)
            return ApiResponse<WithdrawalDto>.Fail(400,
            "No bank account found. Please add your bank information in profile settings.");
        if (string.IsNullOrWhiteSpace(bankInfo.BankBin))
            return ApiResponse<WithdrawalDto>.Fail(400,
                "Bank BIN code is missing. Please update your bank information.");

        try
        {
            wallet.Balance -= request.Amount;
            wallet.PendingBalance += request.Amount;
            wallet.UpdatedAt = DateTime.UtcNow;

            var withdrawal = new WithdrawalRequest()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WalletId = wallet.Id,
                Amount = request.Amount,
                BankName = bankInfo.BankName,
                BankBin = bankInfo.BankBin,
                AccountNumber = bankInfo.AccountNumber,
                AccountName = bankInfo.AccountName,
                Status = WithdrawalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await withdrawalRequestRepository.AddWithdrawalRequestAsync(withdrawal);
            var transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Type = TransactionType.Payout,
                Amount = request.Amount,
                PlatformFee = 0,
                NetAmount = request.Amount,
                BalanceAfter = wallet.Balance,
                ReferenceId = withdrawal.Id,
                ReferenceType = nameof(WithdrawalRequest),
                Status = TransactionStatus.Pending,
                Description = $"Withdrawal to {bankInfo.BankName} - {bankInfo.AccountNumber}",
                CreatedAt = DateTime.UtcNow
            };
            await transactionRepository.AddAsync(transaction);
            await withdrawalRequestRepository.SaveChangesAsync();
            logger.LogInformation(
                "Withdrawal request {Id} created: {Amount} VND by user {UserId}",
                withdrawal.Id, request.Amount, userId);
            return ApiResponse<WithdrawalDto>.Success(201,"Withdrawal Request success",withdrawal.Adapt<WithdrawalDto>());

        }catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create withdrawal request for user {UserId}", userId);
            return ApiResponse<WithdrawalDto>.Fail(500,"Failed to create withdrawal request.");
        }
    }
    public async Task<ApiResponse<List<WithdrawalDto>>> GetMyWithdrawalsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var withdrawals = await withdrawalRequestRepository.GetWithdrawalRequestsByUserIdAsync(userId);

        return ApiResponse<List<WithdrawalDto>>.Success(200, "Withdrawals retrieved successfully", withdrawals.Adapt<List<WithdrawalDto>>());
    }

    public async Task<ApiResponse<List<WithdrawalDto>>> GetWithdrawalsByStatusAsync(WithdrawalStatus status,CancellationToken ct = default)
    {
        var withdrawals = await withdrawalRequestRepository.GetWithdrawalRequestsByStatusAsync(status);

        return ApiResponse<List<WithdrawalDto>>.Success(200,"Withdrawals retrieved successfully",withdrawals.Adapt<List<WithdrawalDto>>());
    }

    public async Task<ApiResponse<WithdrawalDto>> ApproveAsync(Guid withdrawalId, Guid adminId, string? notes,
        CancellationToken ct = default)
    {
        var withdrawal =  await withdrawalRequestRepository.GetWithdrawalRequestByIdAsync(withdrawalId);
        if (withdrawal is null)
            return ApiResponse<WithdrawalDto>.Fail(404,"Withdrawal request not found.");
        
        if (withdrawal.Status != WithdrawalStatus.Pending)
            return ApiResponse<WithdrawalDto>.Fail(400,
                $"Cannot approve a withdrawal with status '{withdrawal.Status}'.");

        try
        {
            withdrawal.Status = WithdrawalStatus.Approved;
            withdrawal.ApprovedByAdminId = adminId;
            withdrawal.ApprovedAt = DateTime.UtcNow;
            withdrawal.AdminNotes = notes;
            
            var referenceId = $"wd_{withdrawal.Id:N}"[..20];
            withdrawal.PayosReferenceId = referenceId;
            withdrawal.Status = WithdrawalStatus.Processing;
            await withdrawalRequestRepository.SaveChangesAsync();

            // var payoutResult = await payOsService.CreatePayoutAsync(
            //     referenceId: referenceId,
            //     amount: (long)withdrawal.Amount,
            //     description: $"Hostlistic payout {withdrawal.Id:N}"[..25],
            //     toBin: withdrawal.BankBin,
            //     toAccountNumber: withdrawal.AccountNumber,
            //     ct: ct
            // );
            // if (payoutResult.IsSuccess)
            // {
                withdrawal.Status = WithdrawalStatus.Completed;
                // withdrawal.PayosPayoutId = payoutResult.PayoutId;
                withdrawal.CompletedAt = DateTime.UtcNow;
                
                withdrawal.Wallet.PendingBalance -= withdrawal.Amount;
                withdrawal.Wallet.UpdatedAt = DateTime.UtcNow;

                var transaction = await transactionRepository.GetByReferenceAsync(withdrawalId, nameof(WithdrawalRequest));
                if (transaction is not null)
                {
                    transaction.Status = TransactionStatus.Completed;
                }
                // logger.LogInformation(
                //     "Payout completed for withdrawal {Id}: PayOS ref={Ref}",
                //     withdrawalId, payoutResult.PayoutId);
                logger.LogInformation("Payout completed for withdrawal {Id}", withdrawalId);
            //}
            // else
            // {
            //     withdrawal.Status = WithdrawalStatus.Failed;
            //     withdrawal.Wallet.Balance += withdrawal.Amount;
            //     withdrawal.Wallet.PendingBalance -= withdrawal.Amount;
            //     withdrawal.Wallet.UpdatedAt = DateTime.UtcNow;
            //     var transaction = await transactionRepository.GetByReferenceAsync(withdrawalId, nameof(WithdrawalRequest));
            //     if (transaction is not null)
            //     {
            //         transaction.Status = TransactionStatus.Failed;
            //         transaction.BalanceAfter = withdrawal.Wallet.Balance;
            //     }
            //     logger.LogWarning(
            //         "PayOS payout failed for withdrawal {Id}: {Error}",
            //         withdrawalId, payoutResult.ErrorMessage);
            // }

            await withdrawalRequestRepository.SaveChangesAsync();
            return ApiResponse<WithdrawalDto>.Success(200, "Approve Success", withdrawal.Adapt<WithdrawalDto>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to approve withdrawal {Id}", withdrawalId);
            return ApiResponse<WithdrawalDto>.Fail(500,"Approval process failed. Please try again.");
        }
    }
    
    
    public async Task<ApiResponse<WithdrawalDto>> RejectAsync(
        Guid withdrawalId, Guid adminId, string reason, CancellationToken ct = default)
    {
        var withdrawal = await withdrawalRequestRepository.GetWithdrawalRequestByIdAsync(withdrawalId);

        if (withdrawal is null)
            return ApiResponse<WithdrawalDto>.Fail(404,"Withdrawal request not found.");

        if (withdrawal.Status != WithdrawalStatus.Pending)
            return ApiResponse<WithdrawalDto>.Fail(400,
                $"Cannot reject a withdrawal with status '{withdrawal.Status}'.");

        // Refund back to balance
        withdrawal.Status = WithdrawalStatus.Rejected;
        withdrawal.RejectionReason = reason;
        withdrawal.ApprovedByAdminId = adminId;
        withdrawal.ApprovedAt = DateTime.UtcNow;

        withdrawal.Wallet.Balance += withdrawal.Amount;
        withdrawal.Wallet.PendingBalance -= withdrawal.Amount;
        withdrawal.Wallet.UpdatedAt = DateTime.UtcNow;

        // Update transaction
        var transaction = await transactionRepository.GetByReferenceAsync(withdrawalId, nameof(WithdrawalRequest));

        if (transaction is not null)
        {
            transaction.Status = TransactionStatus.Reversed;
            transaction.BalanceAfter = withdrawal.Wallet.Balance;
        }

        await withdrawalRequestRepository.SaveChangesAsync();

        logger.LogInformation(
            "Withdrawal {Id} rejected by admin {AdminId}: {Reason}",
            withdrawalId, adminId, reason);

        return ApiResponse<WithdrawalDto>.Success(200,"Rejected",withdrawal.Adapt<WithdrawalDto>());
    }
}