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
            
            entity.Property(e => e.InputParams)
                .HasColumnType("jsonb");

            entity.HasMany(e => e.GeneratedContents)
                .WithOne()
                .HasForeignKey(e => e.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AiGeneratedContent configuration
        modelBuilder.Entity<AiGeneratedContent>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
