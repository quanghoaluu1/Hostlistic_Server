using Common;
using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces
{
    public interface ITalentRepository
    {
        Task<Talent> GetTalentByIdAsync(Guid talentId);
        Task<IEnumerable<Talent>> GetAllTalentsAsync();
        Task<PagedResult<Talent>> GetAllTalentsAsync(string? search, int pageNumber, int pageSize, string? sortBy = null);
        Task<Talent> AddTalentAsync(Talent talent);
        Task<Talent> UpdateTalentAsync(Talent talent);
        Task<bool> DeleteTalentAsync(Guid talentId);
        Task<bool> TalentExistsAsync(Guid talentId);
        Task<List<Talent>> GetTalentByIdAsync(List<Guid> talentIds);
        Task SaveChangesAsync();
    }
}
