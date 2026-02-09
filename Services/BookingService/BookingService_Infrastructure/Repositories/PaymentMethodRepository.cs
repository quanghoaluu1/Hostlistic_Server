using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly BookingServiceDbContext _context;

    public PaymentMethodRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId)
    {
        return await _context.PaymentMethods
            .Include(pm => pm.Payments)
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId);
    }

    public async Task<IEnumerable<PaymentMethod>> GetActivePaymentMethodsAsync()
    {
        return await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentMethod>> GetAllPaymentMethodsAsync()
    {
        return await _context.PaymentMethods
            .ToListAsync();
    }

    public async Task<PaymentMethod?> GetPaymentMethodByCodeAsync(string code)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Code == code);
    }

    public async Task<PaymentMethod> AddPaymentMethodAsync(PaymentMethod paymentMethod)
    {
        paymentMethod.Id = Guid.NewGuid();
        paymentMethod.IsActive = true;
        await _context.PaymentMethods.AddAsync(paymentMethod);
        return paymentMethod;
    }

    public Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        _context.PaymentMethods.Update(paymentMethod);
        return Task.FromResult(paymentMethod);
    }

    public async Task<bool> DeletePaymentMethodAsync(Guid paymentMethodId)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(paymentMethodId);
        if (paymentMethod == null)
            return false;

        _context.PaymentMethods.Remove(paymentMethod);
        return true;
    }

    public async Task<bool> PaymentMethodExistsAsync(Guid paymentMethodId)
    {
        return await _context.PaymentMethods.AnyAsync(pm => pm.Id == paymentMethodId);
    }

    public async Task<bool> PaymentMethodCodeExistsAsync(string code)
    {
        return await _context.PaymentMethods.AnyAsync(pm => pm.Code == code);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}