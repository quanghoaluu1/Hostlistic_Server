using NotificationService_Application.Interfaces;
using NotificationService_Application.Services;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Repositories;

namespace NotificationService_Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<IEmailCampaignRepository, EmailCampaignRepository>();
        services.AddScoped<IEmailLogRepository, EmailLogRepository>();
        services.AddScoped<IEventRecipientRepository, EventRecipientRepository>();

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationCrudService, NotificationCrudService>();
        services.AddScoped<IUserNotificationService, UserNotificationService>();
        services.AddScoped<IEmailCampaignService, EmailCampaignService>();
        services.AddScoped<IEmailLogService, EmailLogService>();
        services.AddScoped<IRecipientResolutionService, RecipientResolutionService>();
        services.AddScoped<ICampaignSendService, CampaignSendService>();

        return services;
    }
}
