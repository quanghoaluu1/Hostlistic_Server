using IdentityService_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService_Infrastructure.Data;

public class IdentityServiceDbContext : DbContext
{
    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
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
                .WithOne(obi => obi.User)
                .HasForeignKey(obi => obi.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UserPlans)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token)
                .IsUnique();
            entity.Property(e => e.Token)
                .HasMaxLength(500);
        });

        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.OrganizerBankInfos)
                .WithOne(obi => obi.Organization)
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
                .WithOne(up => up.SubscriptionPlan)
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
