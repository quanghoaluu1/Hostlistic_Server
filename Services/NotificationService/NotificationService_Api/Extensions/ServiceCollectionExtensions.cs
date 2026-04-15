using NotificationService_Api.Services;
using NotificationService_Application.Emails;
using NotificationService_Application.Interfaces;
using NotificationService_Application.Jobs;
using NotificationService_Application.Services;
using NotificationService_Domain.Interfaces;
using NotificationService_Infrastructure.Repositories;
using NotificationService_Infrastructure.ServiceClients;

namespace NotificationService_Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // SignalR push service
        services.AddScoped<INotificationPushService, SignalRNotificationPushService>();

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
        services.AddScoped<IEmailRateLimiter, EmailRateLimiter>();
        services.AddScoped<HolderTicketEmailRenderer>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<SendReminderCampaignJob>();
        services.AddScoped<IEventServiceClient, EventServiceClient>();
        services.AddScoped<IBookingServiceClient, BookingServiceClient>();
        return services;
    }
}
