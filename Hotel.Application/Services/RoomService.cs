using Hotel.Application.DTOs;
using Hotel.Application.Entities;
using Hotel.Application.Exceptions;
using Hotel.Application.Interfaces;

namespace Hotel.Application.Services;

public class RoomService : IRoomService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoomService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoomDto>> GetAllAsync(RoomFilterDto? filter = null)
    {
        var rooms = await _unitOfWork.Rooms.GetAllAsync();

        if (filter != null)
        {
            rooms = ApplyFilter(rooms, filter);
        }

        return rooms.Select(MapToDto);
    }

    public async Task<RoomDto?> GetByIdAsync(int id)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        return room == null ? null : MapToDto(room);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto)
    {
        var existingRoom = await _unitOfWork.Rooms.GetByNumberAsync(dto.Number);
        if (existingRoom != null)
        {
            throw new ConflictException($"Room with number '{dto.Number}' already exists.");
        }

        ValidateRoomData(dto.Capacity, dto.PricePerNight);

        var room = new Room
        {
            Number = dto.Number,
            Type = dto.Type,
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight,
            IsActive = true,
            HasBalcony = dto.HasBalcony,
            HasView = dto.HasView
        };

        await _unitOfWork.Rooms.AddAsync(room);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id)
            ?? throw new NotFoundException("Room", id);

        if (dto.Number != null && dto.Number != room.Number)
        {
            var existingRoom = await _unitOfWork.Rooms.GetByNumberAsync(dto.Number);
            if (existingRoom != null)
            {
                throw new ConflictException($"Room with number '{dto.Number}' already exists.");
            }
            room.Number = dto.Number;
        }

        if (dto.Type.HasValue) room.Type = dto.Type.Value;
        if (dto.Capacity.HasValue) room.Capacity = dto.Capacity.Value;
        if (dto.PricePerNight.HasValue) room.PricePerNight = dto.PricePerNight.Value;
        if (dto.IsActive.HasValue) room.IsActive = dto.IsActive.Value;
        if (dto.HasBalcony.HasValue) room.HasBalcony = dto.HasBalcony.Value;
        if (dto.HasView.HasValue) room.HasView = dto.HasView.Value;

        ValidateRoomData(room.Capacity, room.PricePerNight);

        await _unitOfWork.Rooms.UpdateAsync(room);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(id);
        if (room == null) return false;

        room.IsActive = false;
        await _unitOfWork.Rooms.UpdateAsync(room);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<RoomDto>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int minCapacity = 1)
    {
        ValidateDateRange(checkIn, checkOut);

        var rooms = await _unitOfWork.Rooms.GetAvailableRoomsAsync(checkIn, checkOut, minCapacity);
        return rooms.Select(MapToDto);
    }

    private static IEnumerable<Room> ApplyFilter(IEnumerable<Room> rooms, RoomFilterDto filter)
    {
        if (filter.Type.HasValue)
            rooms = rooms.Where(r => r.Type == filter.Type.Value);

        if (filter.MinCapacity.HasValue)
            rooms = rooms.Where(r => r.Capacity >= filter.MinCapacity.Value);

        if (filter.MaxPrice.HasValue)
            rooms = rooms.Where(r => r.PricePerNight <= filter.MaxPrice.Value);

        if (filter.IsActive.HasValue)
            rooms = rooms.Where(r => r.IsActive == filter.IsActive.Value);

        if (filter.HasBalcony.HasValue)
            rooms = rooms.Where(r => r.HasBalcony == filter.HasBalcony.Value);

        if (filter.HasView.HasValue)
            rooms = rooms.Where(r => r.HasView == filter.HasView.Value);

        return rooms;
    }

    private static void ValidateRoomData(int capacity, decimal pricePerNight)
    {
        if (capacity < 1)
            throw new ValidationException("Capacity", "Capacity must be at least 1.");

        if (pricePerNight <= 0)
            throw new ValidationException("PricePerNight", "Price per night must be greater than 0.");
    }

    private static void ValidateDateRange(DateTime checkIn, DateTime checkOut)
    {
        if (checkIn >= checkOut)
            throw new ValidationException("CheckInDate", "Check-in date must be before check-out date.");
    }

    private static RoomDto MapToDto(Room room) => new(
        room.Id,
        room.Number,
        room.Type,
        room.Capacity,
        room.PricePerNight,
        room.IsActive,
        room.HasBalcony,
        room.HasView
    );
}
