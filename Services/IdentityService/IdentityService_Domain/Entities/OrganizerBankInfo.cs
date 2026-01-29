namespace IdentityService_Domain.Entities;

public class OrganizerBankInfo
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    
}