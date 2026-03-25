using Common;
using EventService_Api;
using EventService_Api.Extensions;
using EventService_Infrastructure.Data;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using EventService_Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = new ApiResponse<object>
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Dữ liệu không hợp lệ",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});
// builder.Services.AddValidatorsFromAssemblyContaining<CreateEventValidator>();
// builder.Services.AddFluentValidationAutoValidation();   
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

builder.Services.AddDbContext<EventServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly(), typeof(EventService_Application.Mappings.MappingConfig).Assembly);
builder.Services.AddSingleton(config);

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
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddApplicationServices();

var identityServiceUrl = builder.Configuration["ServiceUrls:IdentityService"] ?? "http://localhost:5049";
builder.Services.AddHttpClient("IdentityService", client =>
{
    client.BaseAddress = new Uri(identityServiceUrl.TrimEnd('/'));
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
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();