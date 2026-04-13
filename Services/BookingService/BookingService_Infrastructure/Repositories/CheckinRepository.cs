using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class CheckinRepository(BookingServiceDbContext dbContext) : ICheckinRepository
{
    public IQueryable<CheckIn> GetCheckinQueryable()
    {
        return dbContext.CheckIns;
    }
}