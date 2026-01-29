using Common;
using IdentityService_Domain.Enum;

namespace IdentityService_Domain.Entities;

public class User : BaseClass
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string HashedPassword { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid OrganizationId { get; set; }

    public ICollection<OrganizerBankInfo> OrganizerBankInfos { get; set; } = new List<OrganizerBankInfo>();
    public ICollection<UserPlan> UserPlans { get; set; } = new List<UserPlan>();
}