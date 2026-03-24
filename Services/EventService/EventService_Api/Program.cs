using Common;
using EventService_Api;
using EventService_Application.Interfaces;
using EventService_Application.Services;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using EventService_Infrastructure.Repositories;
using EventService_Infrastructure.ServiceClients;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using EventService_Infrastructure.Interfaces;

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

// builder.Services.AddSwaggerGen(options =>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
// {
//     options.SwaggerDoc("v1", new OpenApiInfo
//     {
//         Title = "Event API",
//         Version = "v1"
//     });
//
//     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.Http,
//         Scheme = "bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "JWT Authorization header using the Bearer scheme."
//     });
//
//     options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
//     {
//         [new OpenApiSecuritySchemeReference("Bearer", document)] = []
//     });
// });
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
// Register repositories
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionBookingRepository, SessionBookingRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IEventTypeRepository, EventTypeRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();
builder.Services.AddScoped<ITalentRepository, TalentRepository>();
builder.Services.AddScoped<ILineupRepository, LineupRepository>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<IEventTemplateRepository, EventTemplateRepository>();
builder.Services.AddScoped<ISponsorRepository, SponsorRepository>();
builder.Services.AddScoped<ISponsorTierRepository, SponsorTierRepository>();
builder.Services.AddScoped<ISponsorInteractionRepository, SponsorInteractionRepository>();
builder.Services.AddScoped<IEventTeamMemberRepository, EventTeamMemberRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IAgendaService, AgendaService>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();

// Register services
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionBookingService, SessionBookingService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IEventTypeService, EventTypeService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ITalentService, TalentService>();
builder.Services.AddScoped<ILineupService, LineupService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IEventTemplateService, EventTemplateService>();
builder.Services.AddScoped<ISponsorService, SponsorService>();
builder.Services.AddScoped<ISponsorTierService, SponsorTierService>();
builder.Services.AddScoped<ISponsorInteractionService, SponsorInteractionService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IUserPlanServiceClient, UserPlanServiceClient>();
builder.Services.AddScoped<IAgendaRepository, AgendaRepository>();
builder.Services.AddScoped<IVenueService, VenueService>();

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