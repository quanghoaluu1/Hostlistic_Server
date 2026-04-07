using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class TalentService : ITalentService
    {
        private readonly ITalentRepository _talentRepository;
        public TalentService(ITalentRepository talentRepository)
        {
            _talentRepository = talentRepository;
        }
        public async Task<ApiResponse<TalentDto>> GetTalentByIdAsync(Guid talentId)
        {
            var talent = await _talentRepository.GetTalentByIdAsync(talentId);
            if (talent == null)
            {
                return ApiResponse<TalentDto>.Fail(404, "Talent not found");
            }
            var talentDto = talent.Adapt<TalentDto>();
            return ApiResponse<TalentDto>.Success(200, "Talent retrieved successfully", talentDto); ;
        }

        public async Task<ApiResponse<List<TalentDto>>> GetAllTalentsAsync()
        {
            var talents = await _talentRepository.GetAllTalentsAsync();
            var talentDtos = talents.Adapt<List<TalentDto>>();
            return ApiResponse<List<TalentDto>>.Success(200, "Talents retrieved successfully", talentDtos);
        }

        public async Task<ApiResponse<PagedResult<TalentDto>>> GetAllTalentsWPagingAsync(TalentSearchRequest? request)
        {

            var pagedTalents = await _talentRepository.GetAllTalentsAsync(request?.Name, request?.Page ?? 1, request?.PageSize ?? 10, request?.SortBy);
            var talentDtos = pagedTalents.Items.Adapt<List<TalentDto>>();
            var result = new PagedResult<TalentDto>
             (
                talentDtos,
                pagedTalents.TotalItems,
                pagedTalents.CurrentPage,
                pagedTalents.PageSize
            );
            return ApiResponse<PagedResult<TalentDto>>.Success(200, "Talents retrieved successfully", result);
        }

        public async Task<ApiResponse<TalentDto>> CreateTalentAsync(CreateTalentDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse<TalentDto>.Fail(400, "Talent name is required");
            if (string.IsNullOrWhiteSpace(request.AvatarUrl))
                request.AvatarUrl =
                    "https://res.cloudinary.com/dvsiqkepf/image/upload/v1770737091/istockphoto-519078727-612x612_sspxxk.jpg";
            var talent = request.Adapt<Talent>();
            talent.Id = Guid.CreateVersion7();
            await _talentRepository.AddTalentAsync(talent);
            await _talentRepository.SaveChangesAsync();

            var talentDto = talent.Adapt<TalentDto>();
            return ApiResponse<TalentDto>.Success(201, "Talent created successfully", talentDto);
        }

        public async Task<ApiResponse<TalentDto>> UpdateTalentAsync(Guid talentId, UpdateTalentDto request)
        {
            var existingTalent = await _talentRepository.GetTalentByIdAsync(talentId);
            if (existingTalent == null)
                return ApiResponse<TalentDto>.Fail(404, "Talent not found");

            if (!string.IsNullOrWhiteSpace(request.Name))
                existingTalent.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                existingTalent.AvatarUrl = request.AvatarUrl;
            if (!string.IsNullOrWhiteSpace(request.Organization))
                existingTalent.Organization = request.Organization;
            if (!string.IsNullOrWhiteSpace(request.Email))
                existingTalent.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.Bio))
                existingTalent.Bio = request.Bio;

            await _talentRepository.UpdateTalentAsync(existingTalent);
            await _talentRepository.SaveChangesAsync();
            var talentDto = existingTalent.Adapt<TalentDto>();
            return ApiResponse<TalentDto>.Success(200, "Talent update successfully", talentDto);
        }

        public async Task<ApiResponse<bool>> DeleteTalentAsync(Guid talentId)
        {
            var talentExists = await _talentRepository.GetTalentByIdAsync(talentId);
            if (talentExists == null)
                return ApiResponse<bool>.Fail(404, "Talent not found");
            if (talentExists.Lineups != null && talentExists.Lineups.Any())
                return ApiResponse<bool>.Fail(400, "Talent has Lineups");
            var deleted = await _talentRepository.DeleteTalentAsync(talentId);
            if (!deleted)
                return ApiResponse<bool>.Fail(500, "Failed to delete talent");
            await _talentRepository.SaveChangesAsync();
            return ApiResponse<bool>.Success(200, "Talent deleted successfully", true);
        }
    }
}
