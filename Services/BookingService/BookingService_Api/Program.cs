using BookingService_Api.Extensions;
using BookingService_Api.Hubs;
using BookingService_Infrastructure.Data;
using Common;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PayOS;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;
using BookingService_Application.Consumers;
using BookingService_Application.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
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
        ClockSkew = TimeSpan.Zero
    };
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

builder.Services.AddDbContext<BookingServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<EventCompletedConsumer>();
    config.AddConsumer<SessionSyncedEventConsumer>();
    config.AddConsumer<SessionDeletedEventConsumer>();
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)
        ));
        cfg.ConfigureEndpoints(context);
    });
});
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);
builder.Services.AddSignalR();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddApplicationServices();

// IsNullOrWhiteSpace: empty appsettings values are not null, so ?? alone is not enough.
var eventServiceUrl = builder.Configuration["ServiceUrls:EventService"];
if (string.IsNullOrWhiteSpace(eventServiceUrl))
    eventServiceUrl = "http://localhost:5139";
var notificationServiceUrl = builder.Configuration["ServiceUrls:NotificationService"];
if (string.IsNullOrWhiteSpace(notificationServiceUrl))
    notificationServiceUrl = "http://localhost:5097";
var identityServiceUrl = builder.Configuration["ServiceUrls:IdentityService"];
if (string.IsNullOrWhiteSpace(identityServiceUrl))
    identityServiceUrl = "http://localhost:5049";

builder.Services.AddHttpClient("EventService", client =>
{
    client.BaseAddress = new Uri(eventServiceUrl.TrimEnd('/'));
});

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(notificationServiceUrl.TrimEnd('/'));
});

builder.Services.AddHttpClient("IdentityService", client =>
{
    client.BaseAddress = new Uri(identityServiceUrl.TrimEnd('/'));
});

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new PayOSClient(new PayOSOptions
    {
        ClientId = config["PayOS:ClientId"]!,
        ApiKey = config["PayOS:ApiKey"]!,
        ChecksumKey = config["PayOS:ChecksumKey"]!,
        TimeoutMs = 30000,
        MaxRetries = 2
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

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
app.UseCors("Production");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<PaymentHub>("/hubs/payment");

app.Run();