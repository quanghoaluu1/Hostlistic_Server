namespace IdentityService_Domain.Entities;

public class Organization
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ContactEmail { get; set; } = string.Empty;
    public string? ContactPhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<OrganizerBankInfo> OrganizerBankInfos { get; set; } = new List<OrganizerBankInfo>();
}