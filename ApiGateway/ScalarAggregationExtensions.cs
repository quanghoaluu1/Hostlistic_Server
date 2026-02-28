using Scalar.AspNetCore;

namespace ApiGateway;

public static class ScalarAggregationExtensions
{
    public static void MapAggregatedScalarDocs(this WebApplication app)
    {
        var serviceSection = app.Configuration.GetSection("ServiceDocs:Services");
        var serviceConfigs = serviceSection.GetChildren().ToDictionary(
            section => section.Key,
            section => new ServiceDocConfig(
                section["Title"] ?? section.Key,
                section["BaseUrl"] ??
                throw new InvalidOperationException(
                    $"Missing BaseUrl for service '{section.Key}' in ServiceDocs config.")
            )
        );
        if (serviceConfigs.Count == 0)
        {
            app.Logger.LogWarning("No services configured for ServiceDocs" +
                                  "Scalar aggregation will show empty docs.");
            return;
        }

        foreach (var (slug, config) in serviceConfigs)
        {
            var serviceSlug = slug;
            var serviceConfig = config;

            app.MapGet($"/docs/{serviceSlug}/openapi.json",
                    async (IHttpClientFactory httpClientFactory, ILogger<Program> logger) =>
                    {
                        var client = httpClientFactory.CreateClient("OpenApiFetcher");
                        try
                        {
                            var specUrl = $"{serviceConfig.BaseUrl}/openapi/v1.json";
                            logger.LogDebug("Fetching OpenAPI spec for {Service} from {Url}",
                                serviceSlug, specUrl);

                            var response = await client.GetAsync(specUrl);

                            if (!response.IsSuccessStatusCode)
                            {
                                logger.LogWarning(
                                    "Failed to fetch OpenAPI spec for {Service}. Status: {StatusCode}",
                                    serviceSlug, response.StatusCode);

                                return Results.Problem(
                                    title: "Service Unavailable",
                                    detail: $"Cannot fetch OpenAPI spec from {serviceSlug}. " +
                                            $"Status: {response.StatusCode}. Service may be offline.",
                                    statusCode: StatusCodes.Status503ServiceUnavailable);
                            }

                            var spec = await response.Content.ReadAsStringAsync();
                            return Results.Content(spec, "application/json");
                        }
                        catch (HttpRequestException ex)
                        {
                            logger.LogError(ex, "Connection failed for {Service} at {BaseUrl}",
                                serviceSlug, serviceConfig.BaseUrl);

                            return Results.Problem(
                                title: "Service Unavailable",
                                detail: $"Cannot connect to {serviceSlug} at {serviceConfig.BaseUrl}. " +
                                        "Ensure the service is running.",
                                statusCode: StatusCodes.Status503ServiceUnavailable);
                        }
                        catch (TaskCanceledException)
                        {
                            logger.LogWarning("Timeout fetching spec for {Service}", serviceSlug);

                            return Results.Problem(
                                title: "Request Timeout",
                                detail: $"Timeout fetching OpenAPI spec from {serviceSlug}.",
                                statusCode: StatusCodes.Status504GatewayTimeout);
                        }
                    })
                .WithTags("Documentation")
                .ExcludeFromDescription();
        }

        // app.MapScalarApiReference(options =>
        // {
        //     options.Title = "Hostlistic - API Documentation";
        //     options.Theme = ScalarTheme.DeepSpace;
        //
        //     // Đăng ký tất cả service specs
        //     foreach (var (slug, config) in serviceConfigs)
        //     {
        //         options.AddDocument(slug, config.Title, $"/docs{slug}/openapi.json");
        //     }
        //
        //     // Bearer auth mặc định cho tất cả documents
        //     options.AddPreferredSecuritySchemes("Bearer");
        //     options.AddHttpAuthentication("Bearer", auth =>
        //     {
        //         auth.Token = "";
        //     });
        // });

        var scalarHtml = GenerateScalarHtml(serviceConfigs);

        app.MapGet("/scalar", () => Results.Content(scalarHtml, "text/html"))
            .ExcludeFromDescription();

        app.Logger.LogInformation(
            "Scalar aggregated docs mapped for {Count} services at /scalar",
            serviceConfigs.Count);
    }

    private static string GenerateScalarHtml(Dictionary<string, ServiceDocConfig> services)
    {
        var sourcesJson = string.Join(",\n            ",
            services.Select(s =>
                $$"""{ "url": "/docs/{{s.Key}}/openapi.json", "title": "{{s.Value.Title}}" }"""));

        return $$"""
                 <!doctype html>
                 <html>
                 <head>
                     <title>Hostlistic - API Documentation</title>
                     <meta charset="utf-8" />
                     <meta name="viewport" content="width=device-width, initial-scale=1" />
                 </head>
                 <body>
                     <div id="app"></div>
                     <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
                     <script>
                         Scalar.createApiReference('#app', {
                             sources: [
                                 {{sourcesJson}}
                             ],
                             theme: 'deepSpace',
                             layout: 'modern',
                             darkMode: true,
                             searchHotKey: 'k',
                             authentication: {
                                 preferredSecurityScheme: 'Bearer',
                                 http: {
                                     bearer: {
                                         token: ''
                                     }
                                 }
                             }
                         })
                     </script>
                 </body>
                 </html>
                 """;
    }
}

record ServiceDocConfig(string Title, string BaseUrl);