using Hotel.Application.DTOs;
using Hotel.Application.Entities;
using Hotel.Application.Exceptions;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    /// <summary>
    /// Get all rooms with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RoomDto>>> GetAll([FromQuery] RoomFilterDto filter)
    {
        _logger.LogInformation("Getting rooms with filter: {@Filter}", filter);

        var rooms = await _roomService.GetAllAsync(filter);
        var roomsList = rooms.ToList();

        // Apply pagination
        var totalCount = roomsList.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
        var pagedRooms = roomsList
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);

        var result = new PagedResult<RoomDto>(
            pagedRooms,
            totalCount,
            filter.Page,
            filter.PageSize,
            totalPages
        );

        return Ok(result);
    }

    /// <summary>
    /// Get room by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomDto>> GetById(int id)
    {
        _logger.LogInformation("Getting room with ID: {RoomId}", id);

        var room = await _roomService.GetByIdAsync(id);
        if (room == null)
        {
            return NotFound(new { message = $"Room with ID {id} not found." });
        }

        return Ok(room);
    }

    /// <summary>
    /// Get available rooms for given date range
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailable(
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int minCapacity = 1)
    {
        _logger.LogInformation(
            "Getting available rooms from {CheckIn} to {CheckOut} with min capacity {MinCapacity}",
            checkIn, checkOut, minCapacity);

        var rooms = await _roomService.GetAvailableRoomsAsync(checkIn, checkOut, minCapacity);
        return Ok(rooms);
    }

    /// <summary>
    /// Create a new room
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomDto>> Create([FromBody] CreateRoomDto dto)
    {
        _logger.LogInformation("Creating room with number: {RoomNumber}", dto.Number);

        var room = await _roomService.CreateAsync(dto);

        _logger.LogInformation("Created room with ID: {RoomId}", room.Id);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    /// <summary>
    /// Update an existing room
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomDto>> Update(int id, [FromBody] UpdateRoomDto dto)
    {
        _logger.LogInformation("Updating room with ID: {RoomId}", id);

        var room = await _roomService.UpdateAsync(id, dto);

        _logger.LogInformation("Updated room with ID: {RoomId}", room.Id);
        return Ok(room);
    }

    /// <summary>
    /// Deactivate a room (soft delete)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("Deactivating room with ID: {RoomId}", id);

        var result = await _roomService.DeactivateAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Room with ID {id} not found." });
        }

        _logger.LogInformation("Deactivated room with ID: {RoomId}", id);
        return NoContent();
    }
}
