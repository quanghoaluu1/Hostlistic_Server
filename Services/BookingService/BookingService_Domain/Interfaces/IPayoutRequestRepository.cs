using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface IPayoutRequestRepository
{
    Task<PayoutRequest?> GetPayoutRequestByIdAsync(Guid payoutRequestId);
    Task<IEnumerable<PayoutRequest>> GetPayoutRequestsByEventIdAsync(Guid eventId);
    Task<IEnumerable<PayoutRequest>> GetPayoutRequestsByOrganizerAsync(Guid organizerBankInfoId);
    Task<PayoutRequest> AddPayoutRequestAsync(PayoutRequest payoutRequest);
    Task<PayoutRequest> UpdatePayoutRequestAsync(PayoutRequest payoutRequest);
    Task<bool> DeletePayoutRequestAsync(Guid payoutRequestId);
    Task<bool> PayoutRequestExistsAsync(Guid payoutRequestId);
    Task SaveChangesAsync();
}