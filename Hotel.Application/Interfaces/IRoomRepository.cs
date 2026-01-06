using Hotel.Application.Entities;

namespace Hotel.Application.Interfaces;

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? minCapacity = null);
    Task<Room?> GetByNumberAsync(string number);
    Task<(IEnumerable<Room> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool? isActive = null);
}
