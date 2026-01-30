using IdentityService_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Infrastructure.Data;

public class IdentityServiceDbContext : DbContext
{
    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizerBankInfo> OrganizerBankInfos => Set<OrganizerBankInfo>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserPlan> UserPlans => Set<UserPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasMany(e => e.OrganizerBankInfos)
                .WithOne()
                .HasForeignKey(obi => obi.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UserPlans)
                .WithOne()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Users)
                .WithOne()
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.OrganizerBankInfos)
                .WithOne()
                .HasForeignKey(obi => obi.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrganizerBankInfo configuration
        modelBuilder.Entity<OrganizerBankInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // SubscriptionPlan configuration
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.CommissionRate)
                .HasPrecision(5, 4);

            entity.HasMany(e => e.UserPlans)
                .WithOne()
                .HasForeignKey(up => up.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserPlan configuration
        modelBuilder.Entity<UserPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique constraint: one user can have only one active plan per subscription type
            entity.HasIndex(e => new { e.UserId, e.SubscriptionPlanId, e.IsActive })
                .HasFilter("\"IsActive\" = true")
                .IsUnique();
        });
    }
}
