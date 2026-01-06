using Hotel.Application.DTOs;

namespace Hotel.Application.Services;

public interface IReservationService
{
    Task<IEnumerable<ReservationDto>> GetAllAsync();
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<IEnumerable<ReservationDto>> GetByGuestIdAsync(int guestId);
    Task<IEnumerable<ReservationDto>> GetByRoomIdAsync(int roomId);
    Task<ReservationDto> CreateAsync(CreateReservationDto dto);
    Task<bool> CancelAsync(int id);
}
