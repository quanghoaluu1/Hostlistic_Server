using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using IdentityService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Domain.Repositories;

public class RefreshTokenRepository(IdentityServiceDbContext dbContext) : IRefreshTokenRepository
{
    public async Task AddTokenAsync(RefreshToken token)
    {
        await dbContext.RefreshTokens.AddAsync(token);
    }

    public async Task<RefreshToken?> GetTokenAsync(string tokenString)
    {
        return await dbContext.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(t => t.Token == tokenString);
    }
    public Task RevokeRefreshTokenAsync(RefreshToken token)
    {
        token.IsRevoked = true;
        dbContext.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }
    public Task SaveChangesAsync()
    {
        return dbContext.SaveChangesAsync();
    }
}