using Hotel.Application.DTOs;
using Hotel.Application.Entities;
using Hotel.Application.Exceptions;
using Hotel.Application.Interfaces;

namespace Hotel.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private const int MaxNights = 30;
    private const int MinNights = 1;

    public ReservationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ReservationDto>> GetAllAsync()
    {
        var reservations = await _unitOfWork.Reservations.GetAllAsync();
        var dtos = new List<ReservationDto>();

        foreach (var reservation in reservations)
        {
            var dto = await LoadReservationDtoAsync(reservation);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var reservation = await _unitOfWork.Reservations.GetWithDetailsAsync(id);
        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<IEnumerable<ReservationDto>> GetByGuestIdAsync(int guestId)
    {
        var reservations = await _unitOfWork.Reservations.GetByGuestIdAsync(guestId);
        var dtos = new List<ReservationDto>();

        foreach (var reservation in reservations)
        {
            var dto = await LoadReservationDtoAsync(reservation);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<ReservationDto>> GetByRoomIdAsync(int roomId)
    {
        var reservations = await _unitOfWork.Reservations.GetByRoomIdAsync(roomId);
        var dtos = new List<ReservationDto>();

        foreach (var reservation in reservations)
        {
            var dto = await LoadReservationDtoAsync(reservation);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        // Validate dates
        ValidateDates(dto.CheckInDate, dto.CheckOutDate);

        // Get and validate room
        var room = await _unitOfWork.Rooms.GetByIdAsync(dto.RoomId)
            ?? throw new NotFoundException("Room", dto.RoomId);

        if (!room.IsActive)
            throw new ValidationException("RoomId", "Cannot create reservation for inactive room.");

        // Validate guest count
        if (dto.GuestsCount < 1)
            throw new ValidationException("GuestsCount", "At least 1 guest is required.");

        if (dto.GuestsCount > room.Capacity)
            throw new ValidationException("GuestsCount", $"Number of guests ({dto.GuestsCount}) exceeds room capacity ({room.Capacity}).");

        // Validate guest exists
        var guest = await _unitOfWork.Guests.GetByIdAsync(dto.GuestId)
            ?? throw new NotFoundException("Guest", dto.GuestId);

        // Check for overlapping reservations
        var hasOverlap = await _unitOfWork.Reservations.HasOverlappingReservationAsync(
            dto.RoomId, dto.CheckInDate, dto.CheckOutDate);

        if (hasOverlap)
            throw new RoomNotAvailableException(dto.RoomId, dto.CheckInDate, dto.CheckOutDate);

        // Calculate total price
        var nights = (dto.CheckOutDate - dto.CheckInDate).Days;
        var totalPrice = nights * room.PricePerNight;

        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            GuestId = dto.GuestId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            GuestsCount = dto.GuestsCount,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Reservations.AddAsync(reservation);
        await _unitOfWork.SaveChangesAsync();

        // Return DTO with room and guest info
        return new ReservationDto(
            reservation.Id,
            room.Id,
            room.Number,
            guest.Id,
            $"{guest.FirstName} {guest.LastName}",
            reservation.CheckInDate,
            reservation.CheckOutDate,
            reservation.GuestsCount,
            reservation.TotalPrice,
            reservation.Status,
            reservation.CreatedAt
        );
    }

    public async Task<bool> CancelAsync(int id)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
        if (reservation == null)
            return false;

        // Idempotent - already canceled is OK
        if (reservation.Status == ReservationStatus.Canceled)
            return true;

        // Cannot cancel past reservations (after check-in date)
        if (reservation.CheckInDate.Date <= DateTime.UtcNow.Date)
            throw new ValidationException("Reservation", "Cannot cancel reservation after check-in date.");

        reservation.Status = ReservationStatus.Canceled;
        await _unitOfWork.Reservations.UpdateAsync(reservation);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static void ValidateDates(DateTime checkIn, DateTime checkOut)
    {
        // Check-in must be before check-out
        if (checkIn >= checkOut)
            throw new ValidationException("CheckInDate", "Check-in date must be before check-out date.");

        // Minimum 1 night
        var nights = (checkOut - checkIn).Days;
        if (nights < MinNights)
            throw new ValidationException("CheckOutDate", $"Minimum stay is {MinNights} night(s).");

        // Maximum 30 nights
        if (nights > MaxNights)
            throw new ValidationException("CheckOutDate", $"Maximum stay is {MaxNights} nights.");

        // Check-in cannot be in the past
        if (checkIn.Date < DateTime.UtcNow.Date)
            throw new ValidationException("CheckInDate", "Check-in date cannot be in the past.");
    }

    private async Task<ReservationDto> LoadReservationDtoAsync(Reservation reservation)
    {
        var room = await _unitOfWork.Rooms.GetByIdAsync(reservation.RoomId);
        var guest = await _unitOfWork.Guests.GetByIdAsync(reservation.GuestId);

        return new ReservationDto(
            reservation.Id,
            reservation.RoomId,
            room?.Number ?? "Unknown",
            reservation.GuestId,
            guest != null ? $"{guest.FirstName} {guest.LastName}" : "Unknown",
            reservation.CheckInDate,
            reservation.CheckOutDate,
            reservation.GuestsCount,
            reservation.TotalPrice,
            reservation.Status,
            reservation.CreatedAt
        );
    }

    private static ReservationDto MapToDto(Reservation reservation) => new(
        reservation.Id,
        reservation.RoomId,
        reservation.Room.Number,
        reservation.GuestId,
        $"{reservation.Guest.FirstName} {reservation.Guest.LastName}",
        reservation.CheckInDate,
        reservation.CheckOutDate,
        reservation.GuestsCount,
        reservation.TotalPrice,
        reservation.Status,
        reservation.CreatedAt
    );
}
