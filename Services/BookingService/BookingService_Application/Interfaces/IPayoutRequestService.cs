using Common;
using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IPayoutRequestService
{
    Task<ApiResponse<PayoutRequestDto>> GetPayoutRequestByIdAsync(Guid payoutRequestId);
    Task<ApiResponse<IEnumerable<PayoutRequestDto>>> GetPayoutRequestsByEventIdAsync(Guid eventId);
    Task<ApiResponse<IEnumerable<PayoutRequestDto>>> GetPayoutRequestsByOrganizerAsync(Guid organizerBankInfoId);
    Task<ApiResponse<PayoutRequestDto>> CreatePayoutRequestAsync(CreatePayoutRequestRequest request);
    Task<ApiResponse<PayoutRequestDto>> UpdatePayoutRequestAsync(Guid payoutRequestId, UpdatePayoutRequestRequest request);
    Task<ApiResponse<bool>> DeletePayoutRequestAsync(Guid payoutRequestId);
}