using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class PayoutRequestRepository : IPayoutRequestRepository
{
    private readonly BookingServiceDbContext _context;

    public PayoutRequestRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PayoutRequest?> GetPayoutRequestByIdAsync(Guid payoutRequestId)
    {
        return await _context.PayoutRequests
            .FirstOrDefaultAsync(pr => pr.Id == payoutRequestId);
    }

    public async Task<IEnumerable<PayoutRequest>> GetPayoutRequestsByEventIdAsync(Guid eventId)
    {
        return await _context.PayoutRequests
            .Where(pr => pr.EventId == eventId)
            .ToListAsync();
    }

    public async Task<IEnumerable<PayoutRequest>> GetPayoutRequestsByOrganizerAsync(Guid organizerBankInfoId)
    {
        return await _context.PayoutRequests
            .Where(pr => pr.OrganizerBankInfoId == organizerBankInfoId)
            .ToListAsync();
    }

    public async Task<PayoutRequest> AddPayoutRequestAsync(PayoutRequest payoutRequest)
    {
        payoutRequest.Id = Guid.NewGuid();
        payoutRequest.Status = BookingService_Domain.Enum.PayoutRequestStatus.Pending;
        await _context.PayoutRequests.AddAsync(payoutRequest);
        return payoutRequest;
    }

    public Task<PayoutRequest> UpdatePayoutRequestAsync(PayoutRequest payoutRequest)
    {
        if (payoutRequest.Status != BookingService_Domain.Enum.PayoutRequestStatus.Pending)
        {
            payoutRequest.ProcessedAt = DateTime.UtcNow;
        }
        
        _context.PayoutRequests.Update(payoutRequest);
        return Task.FromResult(payoutRequest);
    }

    public async Task<bool> DeletePayoutRequestAsync(Guid payoutRequestId)
    {
        var payoutRequest = await _context.PayoutRequests.FindAsync(payoutRequestId);
        if (payoutRequest == null)
            return false;

        _context.PayoutRequests.Remove(payoutRequest);
        return true;
    }

    public async Task<bool> PayoutRequestExistsAsync(Guid payoutRequestId)
    {
        return await _context.PayoutRequests.AnyAsync(pr => pr.Id == payoutRequestId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}