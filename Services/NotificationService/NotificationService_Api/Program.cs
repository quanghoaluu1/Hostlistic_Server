using System.Security.Authentication;
using System.Text;
using Common;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using NotificationService_Api;
using NotificationService_Api.Extensions;
using NotificationService_Api.Hubs;
using NotificationService_Application.Consumers;
using NotificationService_Application.Interfaces;
using NotificationService_Application.Mappings;
using NotificationService_Application.Services;
using NotificationService_Infrastructure.Data;
using Resend;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
var secretKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
var key = Encoding.UTF8.GetBytes(secretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token valid failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            // SignalR passes the token as a query parameter
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

var hangfireConn = builder.Configuration.GetConnectionString("NotificationDbConnection");
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(opts => opts.UseNpgsqlConnection(hangfireConn),
        new PostgreSqlStorageOptions()
    {
        SchemaName                  =  "hangfire",
        PrepareSchemaIfNecessary    = true,  // tự tạo bảng lần đầu
        QueuePollInterval           = TimeSpan.FromSeconds(5),
        InvisibilityTimeout         = TimeSpan.FromMinutes(30),
        DistributedLockTimeout      = TimeSpan.FromMinutes(10),
    }));

builder.Services.AddHangfireServer(opts =>
{
    opts.ServerName  = "NotificationService";
    opts.WorkerCount = 5;
    opts.Queues      = ["reminders", "campaigns", "default"];
    
    opts.HeartbeatInterval        = TimeSpan.FromSeconds(30);
    opts.ServerCheckInterval      = TimeSpan.FromMinutes(1);
    opts.SchedulePollingInterval  = TimeSpan.FromSeconds(15);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", "https://hostlistic.tech")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddDbContext<NotificationServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDbConnection"));
});
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("RedisConnection")!;
    var config = ConfigurationOptions.Parse(connectionString);
    
    config.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
    config.AbortOnConnectFail = false;
    config.ConnectTimeout = 10000;
    config.CertificateValidation += (_, _, _, _) => true;
    
    return ConnectionMultiplexer.Connect(config);
});

builder.Services.AddScoped<IEmailRateLimiter, EmailRateLimiter>();

// Mapster configuration
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(MappingConfig).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddMassTransit(x =>
{
    // Register all consumers in this assembly
    x.AddConsumer<BookingConfirmedConsumer>();
    x.AddConsumer<BulkEmailConsumer>();
    x.AddConsumer<TeamMemberInvitedConsumer>();
 
    x.UsingRabbitMq((context, cfg) =>
    {
        var uri = builder.Configuration.GetConnectionString("rabbitmq");
        if (!string.IsNullOrEmpty(uri))
            cfg.Host(new Uri(uri));
        else
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
            {
                h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
            });
 
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(30)
        ));
 
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddOptions();
builder.Services.AddHttpClient<IResend, ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiToken"];
});
builder.Services.AddTransient<IResend, ResendClient>();

var eventServiceUrl = builder.Configuration["ServiceUrls:EventService"];
if (string.IsNullOrWhiteSpace(eventServiceUrl))
    eventServiceUrl = "http://localhost:5139";
builder.Services.AddHttpClient("EventService", client =>
{
    client.BaseAddress = new Uri(eventServiceUrl.TrimEnd('/'));
});

var bookingServiceUrl = builder.Configuration["ServiceUrls:BookingService"];
if (string.IsNullOrWhiteSpace(bookingServiceUrl))
    bookingServiceUrl = "http://localhost:5077";
builder.Services.AddHttpClient("BookingService", client =>
{
    client.BaseAddress = new Uri(bookingServiceUrl.TrimEnd('/'));
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();

builder.Services.AddApplicationServices();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseExceptionHandler();
app.UseCors("Production");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<NotificationHub>("/hubs/notifications");

    app.UseHangfireDashboard("/hangfire", new DashboardOptions()
    {
        Authorization = [new LocalRequestsOnlyAuthorizationFilter()],
        AppPath                = null,
        DisplayStorageConnectionString = false,
        DashboardTitle         = "Hostlistic — Job Dashboard",
    });


app.Run();
