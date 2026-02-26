using IdentityService_Domain.Entities;

namespace IdentityService_Domain.Interfaces
{
    public interface IOrganizerBankInfoRepository
    {
        Task<OrganizerBankInfo?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<OrganizerBankInfo>> GetByUserIdAsync(Guid userId);
        Task<IReadOnlyList<OrganizerBankInfo>> GetByOrganizationIdAsync(Guid organizationId);
        Task AddAsync(OrganizerBankInfo entity);
        Task UpdateAsync(OrganizerBankInfo entity);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
