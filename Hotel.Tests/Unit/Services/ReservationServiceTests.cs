using FluentAssertions;
using Hotel.Application.DTOs;
using Hotel.Application.Entities;
using Hotel.Application.Exceptions;
using Hotel.Application.Interfaces;
using Hotel.Application.Services;
using Moq;

namespace Hotel.Tests.Unit.Services;

public class ReservationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly ReservationService _sut;

    public ReservationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();

        _unitOfWorkMock.Setup(u => u.Rooms).Returns(_roomRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Guests).Returns(_guestRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Reservations).Returns(_reservationRepositoryMock.Object);

        _sut = new ReservationService(_unitOfWorkMock.Object);
    }

    #region Price Calculation Tests

    [Fact]
    public async Task CreateReservation_ShouldCalculateTotalPrice_MultiplyNightsByPricePerNight()
    {
        // Arrange: 3 nights * 200 PLN = 600 PLN
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 200m, IsActive = true };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(3); // 3 nights

        SetupValidReservation(room, guest, checkIn, checkOut);

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 2);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.TotalPrice.Should().Be(600m); // 3 * 200
    }

    [Fact]
    public async Task CreateReservation_SingleNight_ShouldReturnPricePerNight()
    {
        // Arrange: 1 night * 150 PLN = 150 PLN
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 150m, IsActive = true };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(1); // 1 night

        SetupValidReservation(room, guest, checkIn, checkOut);

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 1);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.TotalPrice.Should().Be(150m);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task CreateReservation_WhenGuestsExceedCapacity_ShouldThrowValidationException()
    {
        // Arrange
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 100m, IsActive = true };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
        _guestRepositoryMock.Setup(g => g.GetByIdAsync(guest.Id)).ReturnsAsync(guest);

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 5); // 5 guests, capacity 2

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*exceeds room capacity*");
    }

    [Fact]
    public async Task CreateReservation_WhenRoomNotActive_ShouldThrowValidationException()
    {
        // Arrange
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 100m, IsActive = false };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 2);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*inactive room*");
    }

    [Fact]
    public async Task CreateReservation_WhenDatesOverlap_ShouldThrowRoomNotAvailableException()
    {
        // Arrange
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 100m, IsActive = true };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(2);

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
        _guestRepositoryMock.Setup(g => g.GetByIdAsync(guest.Id)).ReturnsAsync(guest);
        _reservationRepositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(room.Id, checkIn, checkOut, null))
            .ReturnsAsync(true); // Overlap exists

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 2);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<RoomNotAvailableException>();
    }

    [Fact]
    public async Task CreateReservation_WhenStayTooLong_ShouldThrowValidationException()
    {
        // Arrange: 31 nights (max is 30)
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 100m, IsActive = true };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(31); // 31 nights - too long

        _roomRepositoryMock.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 2);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Maximum stay*");
    }

    [Fact]
    public async Task CreateReservation_WhenCheckOutBeforeCheckIn_ShouldThrowValidationException()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(-1); // Invalid: checkout before checkin

        var dto = new CreateReservationDto(1, 1, checkIn, checkOut, 2);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Check-in date must be before check-out*");
    }

    [Fact]
    public async Task CreateReservation_WhenValid_ShouldSucceed()
    {
        // Arrange
        var room = new Room { Id = 1, Number = "101", Capacity = 2, PricePerNight = 100m, IsActive = true };
        var guest = new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan@test.com" };
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(3);

        SetupValidReservation(room, guest, checkIn, checkOut);

        var dto = new CreateReservationDto(room.Id, guest.Id, checkIn, checkOut, 2);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.RoomId.Should().Be(room.Id);
        result.GuestId.Should().Be(guest.Id);
        result.GuestsCount.Should().Be(2);
        result.Status.Should().Be(ReservationStatus.Active);
        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task CancelReservation_BeforeCheckIn_ShouldSucceed()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = 1,
            RoomId = 1,
            GuestId = 1,
            CheckInDate = DateTime.UtcNow.Date.AddDays(5), // Future check-in
            CheckOutDate = DateTime.UtcNow.Date.AddDays(7),
            Status = ReservationStatus.Active
        };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reservation);

        // Act
        var result = await _sut.CancelAsync(1);

        // Assert
        result.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Canceled);
        _reservationRepositoryMock.Verify(r => r.UpdateAsync(reservation), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelReservation_AfterCheckIn_ShouldThrowValidationException()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = 1,
            RoomId = 1,
            GuestId = 1,
            CheckInDate = DateTime.UtcNow.Date.AddDays(-1), // Past check-in
            CheckOutDate = DateTime.UtcNow.Date.AddDays(2),
            Status = ReservationStatus.Active
        };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reservation);

        // Act
        var act = () => _sut.CancelAsync(1);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*after check-in*");
    }

    [Fact]
    public async Task CancelReservation_AlreadyCanceled_ShouldBeIdempotent()
    {
        // Arrange
        var reservation = new Reservation
        {
            Id = 1,
            RoomId = 1,
            GuestId = 1,
            CheckInDate = DateTime.UtcNow.Date.AddDays(5),
            CheckOutDate = DateTime.UtcNow.Date.AddDays(7),
            Status = ReservationStatus.Canceled // Already canceled
        };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(reservation);

        // Act
        var result = await _sut.CancelAsync(1);

        // Assert
        result.Should().BeTrue();
        _reservationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CancelReservation_WhenNotFound_ShouldReturnFalse()
    {
        // Arrange
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Reservation?)null);

        // Act
        var result = await _sut.CancelAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private void SetupValidReservation(Room room, Guest guest, DateTime checkIn, DateTime checkOut)
    {
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
        _guestRepositoryMock.Setup(g => g.GetByIdAsync(guest.Id)).ReturnsAsync(guest);
        _reservationRepositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(room.Id, checkIn, checkOut, null))
            .ReturnsAsync(false);
        _reservationRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .ReturnsAsync((Reservation r) => r);
    }

    #endregion
}
