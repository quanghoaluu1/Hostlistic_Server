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
        services.Configure<RecordingStorageSettings>(configuration.GetSection(RecordingStorageSettings.SectionName));
        services.Configure<RecordingAutomationSettings>(configuration.GetSection(RecordingAutomationSettings.SectionName));
        services.Configure<RecordingS3Settings>(configuration.GetSection(RecordingS3Settings.SectionName));

        services.AddHttpClient<ILiveKitService, LiveKitService>();
        services.AddTransient<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<IRecordingStorageService>(sp =>
        {
            var s3 = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RecordingS3Settings>>().Value;
            return s3.Enabled
                ? new S3RecordingStorageService(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RecordingS3Settings>>())
                : new LocalRecordingStorageService(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RecordingStorageSettings>>());
        });
        services.AddHostedService<AutoRecordingIngestionService>();

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
