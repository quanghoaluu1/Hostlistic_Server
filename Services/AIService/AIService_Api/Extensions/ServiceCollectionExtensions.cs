using AIService_Application.Interface;
using AIService_Application.Services;
using AIService_Api.Filters;
using AIService_Domain.Interfaces;
using AIService_Infrastructure.Repositories;
using AIService_Infrastructure.ServiceClients; // UserPlanServiceClient, BookingServiceClient

namespace AIService_Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IAiRequestRepository, AiRequestRepository>();
        services.AddScoped<IAiGeneratedContentRepository, AiGeneratedContentRepository>();
        services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();

        // Services
        services.AddScoped<IAiContentService, AiContentService>();
        services.AddScoped<IPromptTemplateService, PromptTemplateService>();
        services.AddScoped<IPromptTemplateEngine, PromptTemplateEngine>();
        services.AddScoped<IUserPlanServiceClient, UserPlanServiceClient>();
        services.AddScoped<IAiPlanEntitlementService, AiPlanEntitlementService>();
        services.AddScoped<RequireAiSubscriptionFilter>();

        // Post-event summary: BookingService client (HttpClient registered in Program.cs)
        services.AddScoped<IBookingServiceClient, BookingServiceClient>();

        // Post-event summary: data aggregation (no LLM dependency)
        services.AddScoped<IAiDataAggregationService, AiDataAggregationService>();

        return services;
    }
}
