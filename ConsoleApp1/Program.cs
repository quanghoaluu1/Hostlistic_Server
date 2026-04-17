
using AIService_Infrastructure.Data;
using BookingService_Infrastructure.Data;
using ConsoleApp1;
using EventService_Infrastructure.Data;
using IdentityService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NotificationService_Infrastructure.Data;
using StreamingService_Infrastructure.Data;

const string DummyConnectionString =
    "Host=localhost;Database=dummy;Username=dummy;Password=dummy";
    
var services = new List<DbContextSource>
{
    new("IdentityService",     CreateContext<IdentityServiceDbContext>()),
    new("EventService",        CreateContext<EventServiceDbContext>()),
    new("BookingService",      CreateContext<BookingServiceDbContext>()),
    new("AIService",           CreateContext<AIServiceDbContext>()),
    new("NotificationService", CreateContext<NotificationServiceDbContext>()),
    new("StreamingService",    CreateContext<StreamingServiceDbContext>()),
};

if (services.Count == 0)
{
    Console.Error.WriteLine(
        "No DbContexts registered. Edit Program.cs to register your DbContexts, "
        + "and add ProjectReference entries in the .csproj file.");
    return 1;
}

var outputPath = args.Length > 0
    ? args[0]
    : Path.Combine(Directory.GetCurrentDirectory(), "hostlistic-schema.dbml");

var generator = new DbmlGenerator(
    projectName: "Hostlistic",
    databaseType: "PostgreSQL",
    projectNote: "AI-integrated event management platform. "
                 + "Microservices architecture with database-per-service isolation.");

var dbml = generator.Generate(services);
 
await File.WriteAllTextAsync(outputPath, dbml);

var totalTables = services.Sum(s => s.Context.Model.GetEntityTypes().Count());
 
Console.WriteLine();
Console.WriteLine("DBML generated successfully.");
Console.WriteLine($"  File:     {Path.GetFullPath(outputPath)}");
Console.WriteLine($"  Services: {services.Count}");
Console.WriteLine($"  Tables:   {totalTables}");
Console.WriteLine();
Console.WriteLine("Next steps:");
Console.WriteLine("  1. Open https://dbdiagram.io");
Console.WriteLine("  2. Paste the content of the .dbml file");
Console.WriteLine("  3. Export as PNG/PDF/SVG for your thesis");

return 0;

static TContext CreateContext<TContext>() where TContext : DbContext
{
    var options = new DbContextOptionsBuilder<TContext>()
        .UseNpgsql(DummyConnectionString)
        .Options;
 
    return (TContext)Activator.CreateInstance(typeof(TContext), options)!;
}
