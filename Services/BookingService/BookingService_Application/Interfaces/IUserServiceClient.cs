using BookingService_Application.DTOs;
using BookingService_Application.Services;

namespace BookingService_Application.Interfaces;

public interface IUserServiceClient
{
    Task<UserInfoDto?> GetUserInfoAsync(Guid userId);
}

