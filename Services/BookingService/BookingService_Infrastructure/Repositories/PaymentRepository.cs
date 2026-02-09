using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly BookingServiceDbContext _context;

    public PaymentRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.Id == paymentId);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        payment.Id = Guid.NewGuid();
        payment.PaymentDate = DateTime.UtcNow;
        await _context.Payments.AddAsync(payment);
        return payment;
    }

    public Task<Payment> UpdatePaymentAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        return Task.FromResult(payment);
    }

    public async Task<bool> DeletePaymentAsync(Guid paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);
        if (payment == null)
            return false;

        _context.Payments.Remove(payment);
        return true;
    }

    public async Task<bool> PaymentExistsAsync(Guid paymentId)
    {
        return await _context.Payments.AnyAsync(p => p.Id == paymentId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}