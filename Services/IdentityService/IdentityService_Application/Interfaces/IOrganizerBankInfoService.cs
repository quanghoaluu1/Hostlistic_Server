using Common;
using IdentityService_Application.DTOs;

namespace IdentityService_Application.Interfaces;

public interface IOrganizerBankInfoService
{
    Task<ApiResponse<OrganizerBankInfoDto>> CreateAsync(CreateOrganizerBankInfoDto dto);
    Task<ApiResponse<OrganizerBankInfoDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<IEnumerable<OrganizerBankInfoDto>>> GetByUserIdAsync(Guid userId);
    Task<ApiResponse<IEnumerable<OrganizerBankInfoDto>>> GetByOrganizationIdAsync(Guid organizationId);
    Task<ApiResponse<OrganizerBankInfoDto>> UpdateAsync(Guid id, UpdateOrganizerBankInfoDto dto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
