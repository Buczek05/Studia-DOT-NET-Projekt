using Hotel.Application.Entities;

namespace Hotel.Application.DTOs;

public record ReservationDto(
    int Id,
    int RoomId,
    string RoomNumber,
    int GuestId,
    string GuestName,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int GuestsCount,
    decimal TotalPrice,
    ReservationStatus Status,
    DateTime CreatedAt
);

public record CreateReservationDto(
    int RoomId,
    int GuestId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int GuestsCount
);
