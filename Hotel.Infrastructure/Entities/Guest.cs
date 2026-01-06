namespace Hotel.Infrastructure.Entities;

public class Guest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? IdDocument { get; set; }

    // Navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
