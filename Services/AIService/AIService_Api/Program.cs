using System.Text;
using AIService_Api.Extensions;
using AIService_Application.Interface;
using AIService_Application.Services;
using AIService_Infrastructure.Data;
using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});builder.Services.AddDbContext<AIServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("AiDbConnection"));
});
builder.Services.AddSingleton<IAiProvider, GeminiProvider>();
builder.Services.AddApplicationServices();
// Same config keys as BookingService (ServiceUrls:*). Use IsNullOrWhiteSpace — empty string is not null.
var eventServiceUrl = builder.Configuration["ServiceUrls:EventService"];
if (string.IsNullOrWhiteSpace(eventServiceUrl))
    eventServiceUrl = "http://localhost:5139";

builder.Services.AddHttpClient<IEventServiceClient, EventServiceClient>(client =>
{
    client.BaseAddress = new Uri(eventServiceUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(10);
});

var identityServiceUrl = builder.Configuration["ServiceUrls:IdentityService"];
if (string.IsNullOrWhiteSpace(identityServiceUrl))
    identityServiceUrl = "http://localhost:5049";

builder.Services.AddHttpClient("IdentityService", client =>
{
    client.BaseAddress = new Uri(identityServiceUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(15);
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
        })
        .CustomCss = """

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
app.UseCors("Production");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();