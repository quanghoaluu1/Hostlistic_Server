using BookingService_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Data;

public class BookingServiceDbContext : DbContext
{
    public BookingServiceDbContext(DbContextOptions<BookingServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PayoutRequest> PayoutRequests => Set<PayoutRequest>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.OrderDetails)
                .WithOne()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Tickets)
                .WithOne()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderDetail configuration
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UnitPrice)
                .HasPrecision(18, 2);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);
        });

        // PaymentMethod configuration
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FeePercentage)
                .HasPrecision(5, 4);
            
            entity.Property(e => e.FixedFee)
                .HasPrecision(18, 2);

            entity.HasMany(e => e.Payments)
                .WithOne()
                .HasForeignKey(e => e.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayoutRequest configuration
        modelBuilder.Entity<PayoutRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Ticket configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => e.TicketCode)
                .IsUnique();
        });
    }
}
