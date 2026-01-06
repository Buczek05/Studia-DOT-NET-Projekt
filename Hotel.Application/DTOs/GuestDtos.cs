namespace Hotel.Application.DTOs;

public record GuestDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? IdDocument
);

public record CreateGuestDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? IdDocument
);

public record UpdateGuestDto(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? IdDocument
);
