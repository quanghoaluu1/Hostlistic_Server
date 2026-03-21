using BookingService_Application.Interfaces;
using BookingService_Application.Services;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using BookingService_Infrastructure.Repositories;
using BookingService_Infrastructure.ServiceClients;
using Common;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;
using PayOS;

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
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPayoutRequestRepository, PayoutRequestRepository>();
builder.Services.AddScoped<ITicketPurchaseService, TicketPurchaseService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IInventoryReservationRepository, InventoryReservationRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IEventSettlementRepository, EventSettlementRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IEventSettlementRepository, EventSettlementRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Register services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IPayoutRequestService, PayoutRequestService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IEventServiceClient, EventServiceClient>();
builder.Services.AddScoped<IUserServiceClient, UserServiceClient>();
builder.Services.AddScoped<INotificationServiceClient, NotificationServiceClient>();
builder.Services.AddScoped<IUserPlanServiceClient, UserPlanServiceClient>();
builder.Services.AddScoped<IPayOsService, PayOsService>();
builder.Services.AddScoped<IPayOsWebhookHandler, PayOsWebhookHandler>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<ISubscriptionPurchaseService, SubscriptionPurchaseService>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

var eventServiceUrl = builder.Configuration["ServiceUrls:EventService"] ?? "http://localhost:5139";
var notificationServiceUrl = builder.Configuration["ServiceUrls:NotificationService"] ?? "http://localhost:5097";
var identityServiceUrl = builder.Configuration["ServiceUrls:IdentityService"] ?? "http://localhost:5049";

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

app.Run();