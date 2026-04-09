using BookingService_Application.DTOs;
using BookingService_Application.Services;

namespace BookingService_Application.Interfaces;

public interface IUserServiceClient
{
    Task<UserInfoDto?> GetUserInfoAsync(Guid userId);
    Task<OrganizerBankInfoDto?> GetOrganizerBankInfoAsync(Guid userId);

}
public class OrganizerBankInfoDto
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? BankBin { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
}
