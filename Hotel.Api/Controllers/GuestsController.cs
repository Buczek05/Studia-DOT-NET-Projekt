using Hotel.Application.DTOs;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly ILogger<GuestsController> _logger;

    public GuestsController(IGuestService guestService, ILogger<GuestsController> logger)
    {
        _guestService = guestService;
        _logger = logger;
    }

    /// <summary>
    /// Get all guests
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GuestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GuestDto>>> GetAll()
    {
        _logger.LogInformation("Getting all guests");

        var guests = await _guestService.GetAllAsync();
        return Ok(guests);
    }

    /// <summary>
    /// Get guest by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GuestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuestDto>> GetById(int id)
    {
        _logger.LogInformation("Getting guest with ID: {GuestId}", id);

        var guest = await _guestService.GetByIdAsync(id);
        if (guest == null)
        {
            return NotFound(new { message = $"Guest with ID {id} not found." });
        }

        return Ok(guest);
    }

    /// <summary>
    /// Get guest by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(GuestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuestDto>> GetByEmail(string email)
    {
        _logger.LogInformation("Getting guest with email: {Email}", email);

        var guest = await _guestService.GetByEmailAsync(email);
        if (guest == null)
        {
            return NotFound(new { message = $"Guest with email '{email}' not found." });
        }

        return Ok(guest);
    }

    /// <summary>
    /// Create a new guest
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GuestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuestDto>> Create([FromBody] CreateGuestDto dto)
    {
        _logger.LogInformation("Creating guest with email: {Email}", dto.Email);

        var guest = await _guestService.CreateAsync(dto);

        _logger.LogInformation("Created guest with ID: {GuestId}", guest.Id);
        return CreatedAtAction(nameof(GetById), new { id = guest.Id }, guest);
    }

    /// <summary>
    /// Update an existing guest
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(GuestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuestDto>> Update(int id, [FromBody] UpdateGuestDto dto)
    {
        _logger.LogInformation("Updating guest with ID: {GuestId}", id);

        var guest = await _guestService.UpdateAsync(id, dto);

        _logger.LogInformation("Updated guest with ID: {GuestId}", guest.Id);
        return Ok(guest);
    }
}
