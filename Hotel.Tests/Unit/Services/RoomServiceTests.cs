using FluentAssertions;
using Hotel.Application.DTOs;
using Hotel.Application.Entities;
using Hotel.Application.Exceptions;
using Hotel.Application.Interfaces;
using Hotel.Application.Services;
using Moq;

namespace Hotel.Tests.Unit.Services;

public class RoomServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly RoomService _sut;

    public RoomServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _roomRepositoryMock = new Mock<IRoomRepository>();

        _unitOfWorkMock.Setup(u => u.Rooms).Returns(_roomRepositoryMock.Object);

        _sut = new RoomService(_unitOfWorkMock.Object);
    }

    #region Get Available Rooms Tests

    [Fact]
    public async Task GetAvailableRooms_ShouldReturnOnlyAvailableRooms()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(3);

        var availableRooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Capacity = 2, PricePerNight = 100m, IsActive = true },
            new() { Id = 2, Number = "102", Capacity = 2, PricePerNight = 120m, IsActive = true }
        };

        _roomRepositoryMock
            .Setup(r => r.GetAvailableRoomsAsync(checkIn, checkOut, 1))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _sut.GetAvailableRoomsAsync(checkIn, checkOut, 1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetAvailableRooms_ShouldFilterByMinCapacity()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(3);
        var minCapacity = 3;

        var availableRooms = new List<Room>
        {
            new() { Id = 3, Number = "301", Capacity = 4, PricePerNight = 300m, IsActive = true }
        };

        _roomRepositoryMock
            .Setup(r => r.GetAvailableRoomsAsync(checkIn, checkOut, minCapacity))
            .ReturnsAsync(availableRooms);

        // Act
        var result = await _sut.GetAvailableRoomsAsync(checkIn, checkOut, minCapacity);

        // Assert
        result.Should().HaveCount(1);
        result.First().Capacity.Should().BeGreaterThanOrEqualTo(minCapacity);
    }

    [Fact]
    public async Task GetAvailableRooms_WhenNoRoomsAvailable_ShouldReturnEmptyList()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(3);

        _roomRepositoryMock
            .Setup(r => r.GetAvailableRoomsAsync(checkIn, checkOut, 1))
            .ReturnsAsync(new List<Room>());

        // Act
        var result = await _sut.GetAvailableRoomsAsync(checkIn, checkOut, 1);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableRooms_WhenInvalidDates_ShouldThrowValidationException()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.Date.AddDays(10);
        var checkOut = checkIn.AddDays(-1); // Invalid

        // Act
        var act = () => _sut.GetAvailableRoomsAsync(checkIn, checkOut, 1);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Create Room Tests

    [Fact]
    public async Task CreateRoom_WhenValid_ShouldSucceed()
    {
        // Arrange
        var dto = new CreateRoomDto("201", RoomType.Double, 2, 200m, true, false);

        _roomRepositoryMock.Setup(r => r.GetByNumberAsync("201")).ReturnsAsync((Room?)null);
        _roomRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Room>()))
            .ReturnsAsync((Room r) => { r.Id = 1; return r; });

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().Be("201");
        result.Type.Should().Be(RoomType.Double);
        result.Capacity.Should().Be(2);
        result.PricePerNight.Should().Be(200m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRoom_WhenDuplicateNumber_ShouldThrowConflictException()
    {
        // Arrange
        var existingRoom = new Room { Id = 1, Number = "201" };
        var dto = new CreateRoomDto("201", RoomType.Double, 2, 200m, null, null);

        _roomRepositoryMock.Setup(r => r.GetByNumberAsync("201")).ReturnsAsync(existingRoom);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateRoom_WhenInvalidCapacity_ShouldThrowValidationException()
    {
        // Arrange
        var dto = new CreateRoomDto("201", RoomType.Double, 0, 200m, null, null); // Invalid capacity

        _roomRepositoryMock.Setup(r => r.GetByNumberAsync("201")).ReturnsAsync((Room?)null);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Capacity*");
    }

    [Fact]
    public async Task CreateRoom_WhenInvalidPrice_ShouldThrowValidationException()
    {
        // Arrange
        var dto = new CreateRoomDto("201", RoomType.Double, 2, -100m, null, null); // Invalid price

        _roomRepositoryMock.Setup(r => r.GetByNumberAsync("201")).ReturnsAsync((Room?)null);

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Price*");
    }

    #endregion

    #region Deactivate Room Tests

    [Fact]
    public async Task DeactivateRoom_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var room = new Room { Id = 1, Number = "101", IsActive = true };
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);

        // Act
        var result = await _sut.DeactivateAsync(1);

        // Assert
        result.Should().BeTrue();
        room.IsActive.Should().BeFalse();
        _roomRepositoryMock.Verify(r => r.UpdateAsync(room), Times.Once);
    }

    [Fact]
    public async Task DeactivateRoom_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        _roomRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Room?)null);

        // Act
        var result = await _sut.DeactivateAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Filter Tests

    [Fact]
    public async Task GetAll_WithTypeFilter_ShouldFilterByType()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Type = RoomType.Single, Capacity = 1, PricePerNight = 100m, IsActive = true },
            new() { Id = 2, Number = "201", Type = RoomType.Double, Capacity = 2, PricePerNight = 200m, IsActive = true },
            new() { Id = 3, Number = "301", Type = RoomType.Suite, Capacity = 4, PricePerNight = 500m, IsActive = true }
        };

        _roomRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        var filter = new RoomFilterDto(Type: RoomType.Double);

        // Act
        var result = await _sut.GetAllAsync(filter);

        // Assert
        result.Should().HaveCount(1);
        result.First().Type.Should().Be(RoomType.Double);
    }

    [Fact]
    public async Task GetAll_WithMaxPriceFilter_ShouldFilterByPrice()
    {
        // Arrange
        var rooms = new List<Room>
        {
            new() { Id = 1, Number = "101", Type = RoomType.Single, Capacity = 1, PricePerNight = 100m, IsActive = true },
            new() { Id = 2, Number = "201", Type = RoomType.Double, Capacity = 2, PricePerNight = 200m, IsActive = true },
            new() { Id = 3, Number = "301", Type = RoomType.Suite, Capacity = 4, PricePerNight = 500m, IsActive = true }
        };

        _roomRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);

        var filter = new RoomFilterDto(MaxPrice: 200m);

        // Act
        var result = await _sut.GetAllAsync(filter);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.PricePerNight.Should().BeLessThanOrEqualTo(200m));
    }

    #endregion
}
