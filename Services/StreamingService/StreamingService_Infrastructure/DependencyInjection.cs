using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StreamingService_Application.Interfaces;
using StreamingService_Infrastructure.Services;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LiveKitSettings>(configuration.GetSection(LiveKitSettings.SectionName));

        services.AddHttpClient<ILiveKitService, LiveKitService>();
        services.AddTransient<ITokenGenerator, TokenGenerator>();

        services.AddScoped<IStreamingServiceDbContext>(provider => provider.GetRequiredService<StreamingService_Infrastructure.Data.StreamingServiceDbContext>());
        services.AddHttpClient<IEventServiceClient, EventServiceClient>((provider, client) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var eventServiceUrl = config["ServiceUrls:EventService"];
            client.BaseAddress = new Uri(eventServiceUrl ?? "https://localhost:7075");
        });
        
        return services;
    }
}
