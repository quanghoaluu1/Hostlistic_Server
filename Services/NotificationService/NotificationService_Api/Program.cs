using Microsoft.EntityFrameworkCore;
using NotificationService_Application.Interfaces;
using NotificationService_Application.Services;
using NotificationService_Infrastructure.Data;
using Resend;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<NotificationServiceDbContext>(optionsAction =>
{
    optionsAction.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddOptions();
builder.Services.AddHttpClient<IResend, ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiToken"];
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<IEmailService, EmailService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();