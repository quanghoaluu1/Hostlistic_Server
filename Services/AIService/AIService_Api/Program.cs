using System.Text;
using AIService_Application.Interface;
using AIService_Application.Services;
using AIService_Domain.Interfaces;
using AIService_Infrastructure.Data;
using AIService_Infrastructure.Repositories;
using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});builder.Services.AddDbContext<AIServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<IAiProvider, GeminiProvider>();
builder.Services.AddScoped<IAiContentService, AiContentService>();
builder.Services.AddScoped<IAiRequestRepository, AiRequestRepository>();
builder.Services.AddScoped<IAiGeneratedContentRepository, AiGeneratedContentRepository>();
builder.Services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
builder.Services.AddScoped<IPromptTemplateService, PromptTemplateService>();
builder.Services.AddScoped<IPromptTemplateEngine, PromptTemplateEngine>();
builder.Services.AddScoped<IEventServiceClient, EventServiceClient>();
builder.Services.AddHttpClient<IEventServiceClient, EventServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:EventService"]!);
    client.Timeout = TimeSpan.FromSeconds(10);
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