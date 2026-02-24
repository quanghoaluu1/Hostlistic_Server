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
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();

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
        
        modelBuilder.Entity<PromptTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TemplateKey)
                .HasConversion<string>()
                .HasMaxLength(100);

            entity.HasIndex(e => e.TemplateKey)
                .IsUnique();

            entity.Property(e => e.Category)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasIndex(e => e.Category);

            entity.Property(e => e.SystemPrompt).HasColumnType("text");
            entity.Property(e => e.UserPromptTemplate).HasColumnType("text");
            entity.Property(e => e.DefaultTemperature).HasPrecision(3, 2);
        });
    }
}
