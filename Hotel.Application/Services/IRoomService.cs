using Hotel.Application.DTOs;

namespace Hotel.Application.Services;

public interface IRoomService
{
    Task<IEnumerable<RoomDto>> GetAllAsync(RoomFilterDto? filter = null);
    Task<RoomDto?> GetByIdAsync(int id);
    Task<RoomDto> CreateAsync(CreateRoomDto dto);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto);
    Task<bool> DeactivateAsync(int id);
    Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int minCapacity = 1);
}
