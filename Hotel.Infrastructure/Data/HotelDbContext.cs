using Hotel.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Data;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Room configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Number)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(r => r.Number)
                .IsUnique();

            entity.Property(r => r.Type)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(r => r.Capacity)
                .IsRequired();

            entity.Property(r => r.PricePerNight)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(r => r.Version)
                .IsRowVersion();
        });

        // Guest configuration
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.Property(g => g.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(g => g.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(g => g.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(g => g.Email)
                .IsUnique();

            entity.Property(g => g.Phone)
                .HasMaxLength(20);

            entity.Property(g => g.IdDocument)
                .HasMaxLength(50);
        });

        // Reservation configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.CheckInDate)
                .IsRequired();

            entity.Property(r => r.CheckOutDate)
                .IsRequired();

            entity.Property(r => r.GuestsCount)
                .IsRequired();

            entity.Property(r => r.TotalPrice)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(r => r.Status)
                .IsRequired()
                .HasConversion<string>();

            entity.Property(r => r.CreatedAt)
                .IsRequired();

            entity.Property(r => r.Version)
                .IsRowVersion();

            // Relationships
            entity.HasOne(r => r.Room)
                .WithMany(room => room.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Guest)
                .WithMany(guest => guest.Reservations)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for date range queries
            entity.HasIndex(r => new { r.RoomId, r.CheckInDate, r.CheckOutDate });
        });
    }
}
