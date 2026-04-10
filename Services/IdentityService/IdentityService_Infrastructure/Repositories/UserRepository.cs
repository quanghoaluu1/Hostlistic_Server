using Common;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Interfaces;
using IdentityService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Domain.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityServiceDbContext _dbContext;
    public UserRepository(IdentityServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsExistByEmailAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }
    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
    }

    public Task UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
    public async Task<User?> GetUserByGoogleIdAsync(string googleId)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    public async Task<List<User>> SearchByEmailAsync(string email, int maxResults = 5)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Email.StartsWith(email))
            .OrderBy(u => u.Email)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<PagedResult<User>> GetUsersAsync(BaseQueryParams request)
    {
        var query = _dbContext.Users
            .Where(u => u.IsActive).AsQueryable();
        query = query.ApplySorting(request.SortBy);
        return await query.ToPagedResultAsync(request.Page, request.PageSize);
    }

    public async Task<(int totalUsers, List<object> userData)> GetUserDashboardRawAsync(DateTime start)
    {
        var totalUsers = await _dbContext.Users
            .CountAsync(u => u.IsActive);

        var userData = await _dbContext.Users
            .Where(u => u.CreatedAt >= start)
            .GroupBy(u => new
            {
                Year = u.CreatedAt.Year,
                Week = u.CreatedAt.DayOfYear / 7 // tránh dùng method custom (EF không translate)
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Week,
                Count = g.Count()
            })
            .ToListAsync<dynamic>();

        return (totalUsers, userData.Cast<object>().ToList());
    }
}