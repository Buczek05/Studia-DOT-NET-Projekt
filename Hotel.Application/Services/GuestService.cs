using Hotel.Application.DTOs;
using Hotel.Application.Entities;
using Hotel.Application.Exceptions;
using Hotel.Application.Interfaces;

namespace Hotel.Application.Services;

public class GuestService : IGuestService
{
    private readonly IUnitOfWork _unitOfWork;

    public GuestService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<GuestDto>> GetAllAsync()
    {
        var guests = await _unitOfWork.Guests.GetAllAsync();
        return guests.Select(MapToDto);
    }

    public async Task<GuestDto?> GetByIdAsync(int id)
    {
        var guest = await _unitOfWork.Guests.GetByIdAsync(id);
        return guest == null ? null : MapToDto(guest);
    }

    public async Task<GuestDto?> GetByEmailAsync(string email)
    {
        var guest = await _unitOfWork.Guests.GetByEmailAsync(email);
        return guest == null ? null : MapToDto(guest);
    }

    public async Task<GuestDto> CreateAsync(CreateGuestDto dto)
    {
        ValidateGuestData(dto.FirstName, dto.LastName, dto.Email);

        var existingGuest = await _unitOfWork.Guests.GetByEmailAsync(dto.Email);
        if (existingGuest != null)
        {
            throw new ConflictException($"Guest with email '{dto.Email}' already exists.");
        }

        var guest = new Guest
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            IdDocument = dto.IdDocument
        };

        await _unitOfWork.Guests.AddAsync(guest);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(guest);
    }

    public async Task<GuestDto> UpdateAsync(int id, UpdateGuestDto dto)
    {
        var guest = await _unitOfWork.Guests.GetByIdAsync(id)
            ?? throw new NotFoundException("Guest", id);

        if (dto.Email != null && dto.Email != guest.Email)
        {
            var existingGuest = await _unitOfWork.Guests.GetByEmailAsync(dto.Email);
            if (existingGuest != null)
            {
                throw new ConflictException($"Guest with email '{dto.Email}' already exists.");
            }
            guest.Email = dto.Email;
        }

        if (dto.FirstName != null) guest.FirstName = dto.FirstName;
        if (dto.LastName != null) guest.LastName = dto.LastName;
        if (dto.Phone != null) guest.Phone = dto.Phone;
        if (dto.IdDocument != null) guest.IdDocument = dto.IdDocument;

        ValidateGuestData(guest.FirstName, guest.LastName, guest.Email);

        await _unitOfWork.Guests.UpdateAsync(guest);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(guest);
    }

    private static void ValidateGuestData(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ValidationException("FirstName", "First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ValidationException("LastName", "Last name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email", "Email is required.");

        if (!email.Contains('@'))
            throw new ValidationException("Email", "Invalid email format.");
    }

    private static GuestDto MapToDto(Guest guest) => new(
        guest.Id,
        guest.FirstName,
        guest.LastName,
        guest.Email,
        guest.Phone,
        guest.IdDocument
    );
}
