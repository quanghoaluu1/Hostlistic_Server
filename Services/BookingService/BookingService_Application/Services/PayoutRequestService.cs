using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;

namespace BookingService_Application.Services;

public class PayoutRequestService : IPayoutRequestService
{
    private readonly IPayoutRequestRepository _payoutRequestRepository;
    private readonly IPhotoService _photoService;

    public PayoutRequestService(IPayoutRequestRepository payoutRequestRepository, IPhotoService photoService)
    {
        _payoutRequestRepository = payoutRequestRepository;
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