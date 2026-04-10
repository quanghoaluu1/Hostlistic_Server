using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService_Domain.Entities;

public class OrganizerBankInfo
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? BankBin { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    
    // Navigation properties to parent
    [ForeignKey("OrganizationId")]
    public virtual Organization? Organization { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}