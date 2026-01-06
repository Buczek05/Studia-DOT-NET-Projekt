using Hotel.Application.Entities;

namespace Hotel.Application.DTOs;

public record RoomDto(
    int Id,
    string Number,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    bool IsActive,
    bool? HasBalcony,
    bool? HasView
);

public record CreateRoomDto(
    string Number,
    RoomType Type,
    int Capacity,
    decimal PricePerNight,
    bool? HasBalcony,
    bool? HasView
);

public record UpdateRoomDto(
    string? Number,
    RoomType? Type,
    int? Capacity,
    decimal? PricePerNight,
    bool? IsActive,
    bool? HasBalcony,
    bool? HasView
);

public record RoomFilterDto(
    RoomType? Type = null,
    int? MinCapacity = null,
    decimal? MaxPrice = null,
    bool? IsActive = null,
    bool? HasBalcony = null,
    bool? HasView = null,
    int Page = 1,
    int PageSize = 10
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
