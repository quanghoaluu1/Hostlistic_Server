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
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<EventSettlement> EventSettlements => Set<EventSettlement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Tickets)
                .WithOne(t => t.Order)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Payments)
                .WithOne()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.OrderCode)
                .IsUnique()
                .HasFilter("\"OrderCode\" IS NOT NULL");
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
                .WithOne(p => p.PaymentMethod)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayoutRequest configuration
        modelBuilder.Entity<PayoutRequest>(entity => { entity.HasKey(e => e.Id); });

        // Ticket configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.TicketCode)
                .IsUnique();
        });

        // InventoryReservation configuration
        modelBuilder.Entity<InventoryReservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReservationId);
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        modelBuilder.Entity<EventSettlement>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
