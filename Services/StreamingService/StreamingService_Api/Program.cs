using System.Text;
using Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;
using StreamingService_Application;
using StreamingService_Infrastructure;
using StreamingService_Infrastructure.Data;
using StreamingService_Infrastructure.Settings;
using StreamingService_Api.Hubs;
using MassTransit;
using StreamingService_Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<StreamingServiceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StreamingDbConnection")));

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<EventCompletedConsumer>();
    config.AddConsumer<SessionCompletedConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var secretKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

var authenticationBuilder = builder.Services.AddAuthentication();

if (!string.IsNullOrWhiteSpace(secretKey))
{
    var key = Encoding.UTF8.GetBytes(secretKey);

    authenticationBuilder.AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "Role"

        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs/streaming"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Token validation failed: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.PostConfigure<AuthenticationOptions>(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    });
}

builder.Services.AddAuthorization();

var app = builder.Build();

var recordingStorageSettings = app.Services
    .GetRequiredService<IConfiguration>()
    .GetSection(RecordingStorageSettings.SectionName)
    .Get<RecordingStorageSettings>() ?? new RecordingStorageSettings();
var recordingAutomationSettings = app.Services
    .GetRequiredService<IConfiguration>()
    .GetSection(RecordingAutomationSettings.SectionName)
    .Get<RecordingAutomationSettings>() ?? new RecordingAutomationSettings();

var recordingRootPath = Path.IsPathRooted(recordingStorageSettings.RootPath)
    ? recordingStorageSettings.RootPath
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, recordingStorageSettings.RootPath));

Directory.CreateDirectory(recordingRootPath);

string ResolveAutomationPath(string configuredPath)
{
    if (Path.IsPathRooted(configuredPath))
    {
        return configuredPath;
    }

    return Path.GetFullPath(Path.Combine(recordingRootPath, configuredPath));
}

if (recordingAutomationSettings.Enabled)
{
    Directory.CreateDirectory(ResolveAutomationPath(recordingAutomationSettings.InboxPath));
    Directory.CreateDirectory(ResolveAutomationPath(recordingAutomationSettings.ProcessedPath));
    Directory.CreateDirectory(ResolveAutomationPath(recordingAutomationSettings.FailedPath));
}

var recordingRequestPath = string.IsNullOrWhiteSpace(recordingStorageSettings.RequestPath)
    ? "/recordings"
    : recordingStorageSettings.RequestPath.StartsWith('/')
        ? recordingStorageSettings.RequestPath
        : $"/{recordingStorageSettings.RequestPath}";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication("Bearer", auth =>
        {
            auth.Token = "";
        }));
}

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(recordingRootPath),
    RequestPath = recordingRequestPath
});

app.MapControllers();
app.MapHub<StreamingHub>("/hubs/streaming");

app.Run();
