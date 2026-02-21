using AIService_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIService_Infrastructure.Data;

public class AIServiceDbContext : DbContext
{
    public AIServiceDbContext(DbContextOptions<AIServiceDbContext> options) : base(options)
    {
    }

    public DbSet<AiRequest> AiRequests => Set<AiRequest>();
    public DbSet<AiGeneratedContent> AiGeneratedContents => Set<AiGeneratedContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AiRequest configuration
        modelBuilder.Entity<AiRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasMany(e => e.GeneratedContents)
                .WithOne(c => c.Request)
                .HasForeignKey(c => c.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AiGeneratedContent configuration
        modelBuilder.Entity<AiGeneratedContent>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
