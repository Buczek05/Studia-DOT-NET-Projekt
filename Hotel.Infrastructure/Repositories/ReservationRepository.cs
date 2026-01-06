using Hotel.Application.Entities;
using Hotel.Application.Interfaces;
using Hotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositories;

public class ReservationRepository : RepositoryBase<Reservation>, IReservationRepository
{
    public ReservationRepository(HotelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reservation>> GetByRoomIdAsync(int roomId)
    {
        return await _dbSet
            .Where(r => r.RoomId == roomId)
            .Include(r => r.Guest)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByGuestIdAsync(int guestId)
    {
        return await _dbSet
            .Where(r => r.GuestId == guestId)
            .Include(r => r.Room)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null)
    {
        // PostgreSQL requires UTC dates
        var checkInUtc = DateTime.SpecifyKind(checkIn, DateTimeKind.Utc);
        var checkOutUtc = DateTime.SpecifyKind(checkOut, DateTimeKind.Utc);

        var query = _dbSet
            .Where(r => r.RoomId == roomId)
            .Where(r => r.Status == ReservationStatus.Active)
            .Where(r => r.CheckInDate < checkOutUtc && r.CheckOutDate > checkInUtc);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<Reservation?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
