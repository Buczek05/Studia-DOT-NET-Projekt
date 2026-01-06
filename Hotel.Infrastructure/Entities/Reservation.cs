namespace Hotel.Infrastructure.Entities;

public enum ReservationStatus
{
    Active,
    Canceled
}

public class Reservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int GuestId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Concurrency token (PostgreSQL uses xmin)
    public uint Version { get; set; }

    // Navigation properties
    public Room Room { get; set; } = null!;
    public Guest Guest { get; set; } = null!;
}
