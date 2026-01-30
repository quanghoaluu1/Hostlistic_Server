using IdentityService_Domain.Entities;

namespace IdentityService_Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> IsExistByEmailAsync(string email);
    Task AddUserAsync(User user);
    Task SaveChangesAsync();

}