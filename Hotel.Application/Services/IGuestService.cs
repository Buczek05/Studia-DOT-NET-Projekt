using Hotel.Application.DTOs;

namespace Hotel.Application.Services;

public interface IGuestService
{
    Task<IEnumerable<GuestDto>> GetAllAsync();
    Task<GuestDto?> GetByIdAsync(int id);
    Task<GuestDto?> GetByEmailAsync(string email);
    Task<GuestDto> CreateAsync(CreateGuestDto dto);
    Task<GuestDto> UpdateAsync(int id, UpdateGuestDto dto);
}
