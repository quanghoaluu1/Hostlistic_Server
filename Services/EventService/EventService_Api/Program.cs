using EventService_Application.Interfaces;
using EventService_Application.Services;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using EventService_Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Scalar.AspNetCore;
using System.Reflection;
using System.Text;
using EventService_Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
Console.WriteLine(key.ToString());
Console.WriteLine(issuer);
Console.WriteLine(audience);
builder.Services.AddDbContext<EventServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(config);

// Register repositories
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionBookingRepository, SessionBookingRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();

// Register services
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionBookingService, SessionBookingService>();
builder.Services.AddScoped<ITrackService, TrackService>();
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();