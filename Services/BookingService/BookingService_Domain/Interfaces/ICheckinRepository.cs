using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface ICheckinRepository
{
    IQueryable<CheckIn> GetCheckinQueryable();
}