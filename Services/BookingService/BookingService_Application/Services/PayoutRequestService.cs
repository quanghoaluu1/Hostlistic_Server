using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace BookingService_Application.Services;

public class PayoutRequestService : IPayoutRequestService
{
    private readonly IPayoutRequestRepository _payoutRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPhotoService _photoService;

    public PayoutRequestService(IPayoutRequestRepository payoutRequestRepository,IWalletRepository walletRepository, ITransactionRepository transactionRepository, IPhotoService photoService)
    {
        _payoutRequestRepository = payoutRequestRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _photoService = photoService;
    }

    public async Task<ApiResponse<PayoutRequestDto>> GetPayoutRequestByIdAsync(Guid payoutRequestId)
    {
        var payoutRequest = await _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutRequestId);
        if (payoutRequest == null)
            return ApiResponse<PayoutRequestDto>.Fail(404, "Payout request not found");

        var payoutRequestDto = payoutRequest.Adapt<PayoutRequestDto>();
        return ApiResponse<PayoutRequestDto>.Success(200, "Payout request retrieved successfully", payoutRequestDto);
    }

    public async Task<ApiResponse<IEnumerable<PayoutRequestDto>>> GetPayoutRequestsByEventIdAsync(Guid eventId)
    {
        var payoutRequests = await _payoutRequestRepository.GetPayoutRequestsByEventIdAsync(eventId);
        var payoutRequestDtos = payoutRequests.Adapt<IEnumerable<PayoutRequestDto>>();
        return ApiResponse<IEnumerable<PayoutRequestDto>>.Success(200, "Payout requests retrieved successfully", payoutRequestDtos);
    }

    public async Task<ApiResponse<IEnumerable<PayoutRequestDto>>> GetPayoutRequestsByOrganizerAsync(Guid organizerBankInfoId)
    {
        var payoutRequests = await _payoutRequestRepository.GetPayoutRequestsByOrganizerAsync(organizerBankInfoId);
        var payoutRequestDtos = payoutRequests.Adapt<IEnumerable<PayoutRequestDto>>();
        return ApiResponse<IEnumerable<PayoutRequestDto>>.Success(200, "Payout requests retrieved successfully", payoutRequestDtos);
    }

    public async Task<ApiResponse<PayoutRequestDto>> CreatePayoutRequestWithProofAsync(CreatePayoutRequestRequest request, IFormFile? proofFile)
    {
        var payoutRequest = request.Adapt<PayoutRequest>();

        // Upload proof image if provided
        if (proofFile is not null && proofFile.Length > 0)
        {
            var uploadResult = await _photoService.UploadPhotoAsync(proofFile);
            if (uploadResult.Error != null)
                return ApiResponse<PayoutRequestDto>.Fail(400, $"Proof image upload failed: {uploadResult.Error.Message}");
            
            payoutRequest.ProofImageUrl = uploadResult.SecureUrl.AbsoluteUri;
        }

        await _payoutRequestRepository.AddPayoutRequestAsync(payoutRequest);
        await _payoutRequestRepository.SaveChangesAsync();

        var payoutRequestDto = payoutRequest.Adapt<PayoutRequestDto>();
        return ApiResponse<PayoutRequestDto>.Success(201, "Payout request created successfully", payoutRequestDto);
    }

    public async Task<ApiResponse<PayoutRequestDto>> UpdatePayoutRequestWithProofAsync(Guid payoutRequestId, UpdatePayoutRequestRequest request, IFormFile? proofFile)
    {
        var existingPayoutRequest = await _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutRequestId);
        if (existingPayoutRequest == null)
            return ApiResponse<PayoutRequestDto>.Fail(404, "Payout request not found");

        // Upload new proof image if provided
        if (proofFile is not null && proofFile.Length > 0)
        {
            var uploadResult = await _photoService.UploadPhotoAsync(proofFile);
            if (uploadResult.Error != null)
                return ApiResponse<PayoutRequestDto>.Fail(400, $"Proof image upload failed: {uploadResult.Error.Message}");
            
            // Delete old proof image if exists
            if (!string.IsNullOrEmpty(existingPayoutRequest.ProofImageUrl))
            {
                var publicId = ExtractPublicIdFromUrl(existingPayoutRequest.ProofImageUrl);
                if (!string.IsNullOrEmpty(publicId))
                {
                    await _photoService.DeletePhotoAsync(publicId);
                }
            }
            
            existingPayoutRequest.ProofImageUrl = uploadResult.SecureUrl.AbsoluteUri;
        }
        else if (!string.IsNullOrEmpty(request.ProofImageUrl))
        {
            existingPayoutRequest.ProofImageUrl = request.ProofImageUrl;
        }

        // Update other properties
        existingPayoutRequest.Status = request.Status;

        await _payoutRequestRepository.UpdatePayoutRequestAsync(existingPayoutRequest);
        await _payoutRequestRepository.SaveChangesAsync();

        var payoutRequestDto = existingPayoutRequest.Adapt<PayoutRequestDto>();
        return ApiResponse<PayoutRequestDto>.Success(200, "Payout request updated successfully", payoutRequestDto);
    }

    public async Task<ApiResponse<PayoutRequestDto>> ApprovePayoutAsync(
    Guid payoutRequestId, 
    IFormFile? proofFile)
{
    var payout = await _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutRequestId);
    if (payout is null)
        return ApiResponse<PayoutRequestDto>.Fail(404, "Payout request not found");

    if (payout.Status != PayoutRequestStatus.Pending)
        return ApiResponse<PayoutRequestDto>.Fail(400, "Payout request is not pending");

    // 1. Kiểm tra wallet balance đủ
    var wallet = await _walletRepository.GetWalletByIdAsync(payout.WalletId);
    if (wallet is null)
        return ApiResponse<PayoutRequestDto>.Fail(404, "Wallet not found");

    if (wallet.Balance < payout.Amount)
        return ApiResponse<PayoutRequestDto>.Fail(400, 
            $"Insufficient wallet balance. Available: {wallet.Balance:N0} VND, Requested: {payout.Amount:N0} VND");

    // 2. Upload proof image (admin chụp màn hình chuyển khoản)
    if (proofFile is not null && proofFile.Length > 0)
    {
        var uploadResult = await _photoService.UploadPhotoAsync(proofFile);
        if (uploadResult.Error is not null)
            return ApiResponse<PayoutRequestDto>.Fail(400, $"Proof upload failed: {uploadResult.Error.Message}");
        
        payout.ProofImageUrl = uploadResult.SecureUrl.AbsoluteUri;
    }

    // 3. Debit wallet — ATOMIC
    wallet.Balance -= payout.Amount;

    // 4. Tạo Transaction record
    var transaction = new Transaction
    {
        Id = Guid.CreateVersion7(),
        WalletId = wallet.Id,
        Type = TransactionType.Payout,
        Amount = payout.Amount,
        PlatformFee = 0,
        NetAmount = payout.Amount,
        BalanceAfter = wallet.Balance,
        ReferenceId = payout.Id,
        ReferenceType = "PayoutRequest",
        Status = TransactionStatus.Completed,
        Description = $"Payout to bank account. Request #{payout.Id.ToString()[..8]}"
    };

    // 5. Update payout status
    payout.Status = PayoutRequestStatus.Approved;
    payout.ProcessedAt = DateTime.UtcNow;

    // 6. Persist atomically
    await _walletRepository.UpdateWalletAsync(wallet);
    await _transactionRepository.AddAsync(transaction);
    await _payoutRequestRepository.UpdatePayoutRequestAsync(payout);
    await _payoutRequestRepository.SaveChangesAsync();

    var dto = payout.Adapt<PayoutRequestDto>();
    return ApiResponse<PayoutRequestDto>.Success(200, "Payout approved and processed", dto);
}
    
    private static string ExtractPublicIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/');
            var fileName = pathSegments[^1]; // Get last segment
            return Path.GetFileNameWithoutExtension(fileName);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<ApiResponse<bool>> DeletePayoutRequestAsync(Guid payoutRequestId)
    {
        var exists = await _payoutRequestRepository.PayoutRequestExistsAsync(payoutRequestId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Payout request not found");

        var deleted = await _payoutRequestRepository.DeletePayoutRequestAsync(payoutRequestId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete payout request");

        await _payoutRequestRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Payout request deleted successfully", true);
    }
}