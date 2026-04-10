namespace IdentityService_Application.DTOs
{
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

    public class CreateOrganizerBankInfoDto
    {
        public string BankName { get; set; } = string.Empty;
        public string? BankBin { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public Guid? OrganizationId { get; set; }
        public Guid? UserId { get; set; }
    }

    public class UpdateOrganizerBankInfoDto
    {
        public string? BankName { get; set; }
        public string? BankBin { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? UserId { get; set; }
    }
}
