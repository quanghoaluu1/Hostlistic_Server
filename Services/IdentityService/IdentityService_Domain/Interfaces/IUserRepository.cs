using Common;
using IdentityService_Domain.Entities;

namespace IdentityService_Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<bool> IsExistByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task SaveChangesAsync();
    Task<User?> GetUserByGoogleIdAsync(string googleId);
    Task<List<User>> SearchByEmailAsync(string email, int maxResults = 5);
    Task<(int totalUsers, List<DateTime> userData)> GetUserDashboardRawAsync(DateTime start);
    Task<PagedResult<User>> GetUsersAsync(BaseQueryParams request);
}