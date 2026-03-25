using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Common;
using IdentityService_Api.Extensions;
using IdentityService_Application.Services;
using IdentityService_Infrastructure.Data;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});
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
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token valid failed: " + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddDbContext<IdentityServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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

// Configure Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);
builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:NotificationService"] ?? "http://notificationservice:8080");
});
builder.Services.AddHttpClient("BookingService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BookingService"] ?? "http://bookingservice:8080");
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices();
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
        }).CustomCss = """

                       /* Set the width constraints */
                       .cm-editor, .cm-scroller, .cm-content {
                         min-width: 0 !important;
                         max-width: calc(100% - 46px); !important; /* account for gutter and border */
                       }

                       /* Disable horizontal scrolling at every layer that might scroll */
                       .body-raw-scroller, .cm-scroller {
                         overflow-x: hidden !important;
                       }

                       /* Force wrapping on each rendered line */
                       .cm-content, .cm-line {
                         white-space: pre-wrap !important;
                         overflow-wrap: anywhere !important;
                         word-break: break-word !important;
                       }

                       /* Make sure syntax-highlight spans don’t re-disable wrapping */
                       .cm-line > span {
                         white-space: inherit !important;
                         overflow-wrap: anywhere !important;
                         word-break: break-word !important;
                       }

                       /* Align value span with key span */
                       span.ͼu { 
                         padding-left: 2ch !important;
                       }
                       """);
}

app.UseRouting();
app.UseCors("Production");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();