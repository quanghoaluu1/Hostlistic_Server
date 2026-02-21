using Common;
using BookingService_Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace BookingService_Application.Interfaces;

public interface IPayoutRequestService
{
    Task<ApiResponse<PayoutRequestDto>> GetPayoutRequestByIdAsync(Guid payoutRequestId);
    Task<ApiResponse<IEnumerable<PayoutRequestDto>>> GetPayoutRequestsByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<PayoutRequestDto>>> GetPayoutRequestsByOrganizerAsync(Guid organizerBankInfoId);
    Task<ApiResponse<PayoutRequestDto>> CreatePayoutRequestWithProofAsync(CreatePayoutRequestRequest request, IFormFile? proofFile);
    Task<ApiResponse<PayoutRequestDto>> UpdatePayoutRequestWithProofAsync(Guid payoutRequestId, UpdatePayoutRequestRequest request, IFormFile? proofFile);
    Task<ApiResponse<bool>> DeletePayoutRequestAsync(Guid payoutRequestId);
}