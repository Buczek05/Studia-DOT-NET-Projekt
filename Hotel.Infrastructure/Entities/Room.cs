namespace Hotel.Infrastructure.Entities;

public enum RoomType
{
    Single,
    Double,
    Suite
}

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;
    public bool? HasBalcony { get; set; }
    public bool? HasView { get; set; }

    // Concurrency token (PostgreSQL uses xmin)
    public uint Version { get; set; }

    // Navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
