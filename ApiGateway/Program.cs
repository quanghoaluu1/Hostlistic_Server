using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using ApiGateway;

var builder = WebApplication.CreateBuilder(args);

// Add YARP services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", "https://hostlistic.tech", "https://www.hostlistic.tech")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

// Add JWT Authentication (optional - if you want authentication at gateway level)
var secretKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];   

if (!string.IsNullOrEmpty(secretKey))
{
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
    
    builder.Services.AddAuthorization();
}
builder.Services.AddHttpClient("OpenApiFetcher", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
// Add health checks (optional but recommended)
builder.Services.AddHealthChecks();

// Add rate limiter
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapAggregatedScalarDocs();
}

app.UseCors("Production");

app.UseRouting();

// Use authentication if configured
if (!string.IsNullOrEmpty(secretKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseRateLimiter(); // Use rate limiter

// Map reverse proxy
app.MapReverseProxy();

// Health check endpoint (optional)
app.MapHealthChecks("/health");

app.Run();