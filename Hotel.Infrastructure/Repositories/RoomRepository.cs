using Hotel.Application.Entities;
using Hotel.Application.Interfaces;
using Hotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositories;

public class RoomRepository : RepositoryBase<Room>, IRoomRepository
{
    public RoomRepository(HotelDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? minCapacity = null)
    {
        // PostgreSQL requires UTC dates
        var checkInUtc = DateTime.SpecifyKind(checkIn, DateTimeKind.Utc);
        var checkOutUtc = DateTime.SpecifyKind(checkOut, DateTimeKind.Utc);

        var query = _dbSet
            .Where(r => r.IsActive)
            .Where(r => !r.Reservations.Any(res =>
                res.Status == ReservationStatus.Active &&
                res.CheckInDate < checkOutUtc &&
                res.CheckOutDate > checkInUtc));

        if (minCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Room?> GetByNumberAsync(string number)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Number == number);
    }

    public async Task<(IEnumerable<Room> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool? isActive = null)
    {
        var query = _dbSet.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
