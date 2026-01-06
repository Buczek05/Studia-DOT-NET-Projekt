using Hotel.Application.Interfaces;
using Hotel.Infrastructure.Data;

namespace Hotel.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly HotelDbContext _context;
    private IRoomRepository? _rooms;
    private IGuestRepository? _guests;
    private IReservationRepository? _reservations;

    public UnitOfWork(HotelDbContext context)
    {
        _context = context;
    }

    public IRoomRepository Rooms => _rooms ??= new RoomRepository(_context);
    public IGuestRepository Guests => _guests ??= new GuestRepository(_context);
    public IReservationRepository Reservations => _reservations ??= new ReservationRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
