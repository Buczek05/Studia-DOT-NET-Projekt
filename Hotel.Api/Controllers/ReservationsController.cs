using Hotel.Application.DTOs;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all reservations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAll()
    {
        _logger.LogInformation("Getting all reservations");

        var reservations = await _reservationService.GetAllAsync();
        return Ok(reservations);
    }

    /// <summary>
    /// Get reservation by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationDto>> GetById(int id)
    {
        _logger.LogInformation("Getting reservation with ID: {ReservationId}", id);

        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation == null)
        {
            return NotFound(new { message = $"Reservation with ID {id} not found." });
        }

        return Ok(reservation);
    }

    /// <summary>
    /// Get reservations by guest ID
    /// </summary>
    [HttpGet("by-guest/{guestId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByGuestId(int guestId)
    {
        _logger.LogInformation("Getting reservations for guest ID: {GuestId}", guestId);

        var reservations = await _reservationService.GetByGuestIdAsync(guestId);
        return Ok(reservations);
    }

    /// <summary>
    /// Get reservations by room ID
    /// </summary>
    [HttpGet("by-room/{roomId:int}")]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetByRoomId(int roomId)
    {
        _logger.LogInformation("Getting reservations for room ID: {RoomId}", roomId);

        var reservations = await _reservationService.GetByRoomIdAsync(roomId);
        return Ok(reservations);
    }

    /// <summary>
    /// Create a new reservation
    /// </summary>
    /// <remarks>
    /// Validations:
    /// - Room must exist and be active
    /// - Guest must exist
    /// - CheckOut must be after CheckIn
    /// - Stay must be 1-30 nights
    /// - GuestsCount must not exceed room capacity
    /// - No date conflicts with existing reservations
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationDto>> Create([FromBody] CreateReservationDto dto)
    {
        _logger.LogInformation(
            "Creating reservation for room {RoomId}, guest {GuestId}, from {CheckIn} to {CheckOut}",
            dto.RoomId, dto.GuestId, dto.CheckInDate, dto.CheckOutDate);

        var reservation = await _reservationService.CreateAsync(dto);

        _logger.LogInformation("Created reservation with ID: {ReservationId}", reservation.Id);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation);
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    /// <remarks>
    /// - Can only cancel before check-in date
    /// - Idempotent: calling multiple times on cancelled reservation is OK
    /// </remarks>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id)
    {
        _logger.LogInformation("Cancelling reservation with ID: {ReservationId}", id);

        var result = await _reservationService.CancelAsync(id);
        if (!result)
        {
            return NotFound(new { message = $"Reservation with ID {id} not found." });
        }

        _logger.LogInformation("Cancelled reservation with ID: {ReservationId}", id);
        return NoContent();
    }
}
