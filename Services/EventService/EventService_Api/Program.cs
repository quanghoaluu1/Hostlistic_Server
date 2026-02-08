using Common;
using EventService_Api;
using EventService_Application.Interfaces;
using EventService_Application.Services;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using EventService_Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);

builder.Services.AddCors(options =>
{
    options.AddPolicy("NextApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
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

// Register services
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionBookingService, SessionBookingService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IEventTypeService, EventTypeService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ITalentService, TalentService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication("Bearer", auth =>
        {
            auth.Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        }));
}
app.UseCors("NextApp");
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();