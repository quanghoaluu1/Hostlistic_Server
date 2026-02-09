using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public class PayoutRequestDto
{
    public Guid Id { get; set; }
    public Guid OrganizerBankInfoId { get; set; }
    public Guid EventId { get; set; }
    public PayoutRequestStatus Status { get; set; }
    public string? ProofImageUrl { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CreatePayoutRequestRequest
{
    public Guid OrganizerBankInfoId { get; set; }
    public Guid EventId { get; set; }
}

public class UpdatePayoutRequestRequest
{
    public PayoutRequestStatus Status { get; set; }
    public string? ProofImageUrl { get; set; }
}