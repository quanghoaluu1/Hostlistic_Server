using IdentityService_Domain.Entities;

namespace IdentityService_Domain.Interfaces;

public interface IRefreshTokenRepository
{
    
    Task AddTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetTokenAsync(string tokenString);
    Task RevokeRefreshTokenAsync(RefreshToken token);
    Task SaveChangesAsync();
}