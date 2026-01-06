using Hotel.Application.Entities;

namespace Hotel.Application.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IEnumerable<Reservation>> GetByRoomIdAsync(int roomId);
    Task<IEnumerable<Reservation>> GetByGuestIdAsync(int guestId);
    Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeReservationId = null);
    Task<Reservation?> GetWithDetailsAsync(int id);
}
