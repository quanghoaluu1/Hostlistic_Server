using IdentityService_Application.Interfaces;
using IdentityService_Application.Services;
using IdentityService_Domain.Interfaces;
using IdentityService_Domain.Repositories;
using NotificationService_Application.Interfaces;

namespace IdentityService_Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IOrganizerBankInfoRepository, OrganizerBankInfoRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IUserPlanRepository, UserPlanRepository>();

        // Services
        services.AddScoped<INotificationServiceClient, NotificationServiceClient>();
        services.AddScoped<IBookingServiceClient, BookingServiceClient>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IUserTicketService, UserTicketService>();
        services.AddScoped<IOrganizerBankInfoService, OrganizerBankInfoService>();
        services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
        services.AddScoped<IUserPlanService, UserPlanService>();

        return services;
    }
}
