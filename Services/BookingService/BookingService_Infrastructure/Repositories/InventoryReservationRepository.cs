using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService_Infrastructure.Repositories
{
    public class InventoryReservationRepository : IInventoryReservationRepository
    {
        public readonly BookingServiceDbContext _context;

        public InventoryReservationRepository(BookingServiceDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryReservation> CreateReservationAsync(InventoryReservation reservation)
        {
            await _context.InventoryReservations.AddAsync(reservation);
            return reservation;
        }

        public async Task<List<InventoryReservation>> GetReservationsByIdAsync(Guid reservationId)
        {
            return await _context.InventoryReservations
                .Where(r => r.ReservationId == reservationId)
                .ToListAsync();
        }

        public async Task DeleteReservationsAsync(Guid reservationId)
        {
            var reservations = await _context.InventoryReservations
                .Where(r => r.ReservationId == reservationId)
                .ToListAsync();

            _context.InventoryReservations.RemoveRange(reservations);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
