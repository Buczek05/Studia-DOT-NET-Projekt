using Hotel.Application.Entities;

namespace Hotel.Application.DTOs;

public record AvailabilityQueryDto(
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int? MinCapacity = null
);

public record AvailableRoomDto(
    int Id,
    string Number,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    decimal TotalPriceForStay,
    bool? HasBalcony,
    bool? HasView
);
