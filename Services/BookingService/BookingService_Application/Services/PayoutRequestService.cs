using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class PayoutRequestService : IPayoutRequestService
{
    private readonly IPayoutRequestRepository _payoutRequestRepository;

    public PayoutRequestService(IPayoutRequestRepository payoutRequestRepository)
    {
        _payoutRequestRepository = payoutRequestRepository;
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

    // add real payment intergration later
    public async Task<ApiResponse<PayoutRequestDto>> CreatePayoutRequestAsync(CreatePayoutRequestRequest request)
    {
        var payoutRequest = request.Adapt<PayoutRequest>();

        await _payoutRequestRepository.AddPayoutRequestAsync(payoutRequest);
        await _payoutRequestRepository.SaveChangesAsync();

        var payoutRequestDto = payoutRequest.Adapt<PayoutRequestDto>();
        return ApiResponse<PayoutRequestDto>.Success(201, "Payout request created successfully", payoutRequestDto);
    }

    public async Task<ApiResponse<PayoutRequestDto>> UpdatePayoutRequestAsync(Guid payoutRequestId, UpdatePayoutRequestRequest request)
    {
        var existingPayoutRequest = await _payoutRequestRepository.GetPayoutRequestByIdAsync(payoutRequestId);
        if (existingPayoutRequest == null)
            return ApiResponse<PayoutRequestDto>.Fail(404, "Payout request not found");

        // Update properties
        existingPayoutRequest.Status = request.Status;
        existingPayoutRequest.ProofImageUrl = request.ProofImageUrl;

        await _payoutRequestRepository.UpdatePayoutRequestAsync(existingPayoutRequest);
        await _payoutRequestRepository.SaveChangesAsync();

        var payoutRequestDto = existingPayoutRequest.Adapt<PayoutRequestDto>();
        return ApiResponse<PayoutRequestDto>.Success(200, "Payout request updated successfully", payoutRequestDto);
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