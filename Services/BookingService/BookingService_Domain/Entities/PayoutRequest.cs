using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class PayoutRequest
{
    public Guid Id { get; set; }
    public Guid OrganizerBankInfoId { get; set; }
    public Guid EventId { get; set; }
    public PayoutRequestStatus Status { get; set; }
    public string? ProofImageUrl { get; set; }
    public DateTime? ProcessedAt { get; set; }
}