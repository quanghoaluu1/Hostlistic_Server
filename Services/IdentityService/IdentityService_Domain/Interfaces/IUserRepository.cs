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
}