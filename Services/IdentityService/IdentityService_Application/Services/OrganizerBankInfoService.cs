using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using Mapster;

namespace IdentityService_Application.Services;

public class OrganizerBankInfoService(IOrganizerBankInfoRepository repository) : IOrganizerBankInfoService
{
    public async Task<ApiResponse<OrganizerBankInfoDto>> CreateAsync(CreateOrganizerBankInfoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BankName) ||
            string.IsNullOrWhiteSpace(dto.AccountNumber) ||
            string.IsNullOrWhiteSpace(dto.AccountName) ||
            string.IsNullOrWhiteSpace(dto.BankBin) )
        {
            return ApiResponse<OrganizerBankInfoDto>.Fail(400, "Thiếu thông tin ngân hàng");
        }

        if (dto.OrganizationId is null && dto.UserId is null)
        {
            return ApiResponse<OrganizerBankInfoDto>.Fail(400, "Phải gắn với User hoặc Organization");
        }

        var entity = dto.Adapt<OrganizerBankInfo>();
        entity.Id = Guid.NewGuid();

        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<OrganizerBankInfoDto>();
        return ApiResponse<OrganizerBankInfoDto>.Success(201, "Tạo tài khoản ngân hàng thành công", result);
    }

    public async Task<ApiResponse<OrganizerBankInfoDto>> GetByIdAsync(Guid id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<OrganizerBankInfoDto>.Fail(404, "Không tìm thấy");

        var dto = entity.Adapt<OrganizerBankInfoDto>();
        return ApiResponse<OrganizerBankInfoDto>.Success(200, "Lấy dữ liệu thành công", dto);
    }

    public async Task<ApiResponse<IEnumerable<OrganizerBankInfoDto>>> GetByUserIdAsync(Guid userId)
    {
        var list = await repository.GetByUserIdAsync(userId);
        var dtos = list.Adapt<IEnumerable<OrganizerBankInfoDto>>();
        return ApiResponse<IEnumerable<OrganizerBankInfoDto>>.Success(200, "OK", dtos);
    }

    public async Task<ApiResponse<IEnumerable<OrganizerBankInfoDto>>> GetByOrganizationIdAsync(Guid organizationId)
    {
        var list = await repository.GetByOrganizationIdAsync(organizationId);
        var dtos = list.Adapt<IEnumerable<OrganizerBankInfoDto>>();
        return ApiResponse<IEnumerable<OrganizerBankInfoDto>>.Success(200, "OK", dtos);
    }

    public async Task<ApiResponse<OrganizerBankInfoDto>> UpdateAsync(Guid id, UpdateOrganizerBankInfoDto dto)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity == null)
            return ApiResponse<OrganizerBankInfoDto>.Fail(404, "Không tìm thấy");

        if (!string.IsNullOrWhiteSpace(dto.BankName))
            entity.BankName = dto.BankName;
        if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
            entity.AccountNumber = dto.AccountNumber;
        if (!string.IsNullOrWhiteSpace(dto.AccountName))
            entity.AccountName = dto.AccountName;
        if (!string.IsNullOrWhiteSpace(dto.BankBin))
        {
            entity.BankBin = dto.BankBin;
        }
        if (dto.OrganizationId.HasValue)
            entity.OrganizationId = dto.OrganizationId;
        if (dto.UserId.HasValue)
            entity.UserId = dto.UserId;

        await repository.UpdateAsync(entity);
        await repository.SaveChangesAsync();

        var result = entity.Adapt<OrganizerBankInfoDto>();
        return ApiResponse<OrganizerBankInfoDto>.Success(200, "Cập nhật thành công", result);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var ok = await repository.DeleteAsync(id);
        if (!ok)
            return ApiResponse<bool>.Fail(404, "Không tìm thấy");

        await repository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Xoá thành công", true);
    }
}
