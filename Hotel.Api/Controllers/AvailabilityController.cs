using Hotel.Application.DTOs;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<AvailabilityController> _logger;

    public AvailabilityController(IRoomService roomService, ILogger<AvailabilityController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    /// <summary>
    /// Get available rooms for a given date range
    /// </summary>
    /// <remarks>
    /// Returns rooms that are:
    /// - Active
    /// - Have no conflicting reservations for the given date range
    /// - Meet minimum capacity requirement (if specified)
    ///
    /// Each room includes the total price for the entire stay.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AvailableRoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AvailableRoomDto>>> GetAvailableRooms(
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int? minCapacity = null)
    {
        _logger.LogInformation(
            "Checking availability from {CheckIn} to {CheckOut} with min capacity {MinCapacity}",
            checkIn, checkOut, minCapacity);

        var rooms = await _roomService.GetAvailableRoomsAsync(checkIn, checkOut, minCapacity ?? 1);

        var nights = (checkOut - checkIn).Days;
        var availableRooms = rooms.Select(r => new AvailableRoomDto(
            r.Id,
            r.Number,
            r.Type,
            r.Capacity,
            r.PricePerNight,
            r.PricePerNight * nights,
            r.HasBalcony,
            r.HasView
        ));

        return Ok(availableRooms);
    }
}
