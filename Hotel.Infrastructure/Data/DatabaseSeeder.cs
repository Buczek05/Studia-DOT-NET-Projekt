using Hotel.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(HotelDbContext context)
    {
        if (await context.Rooms.AnyAsync())
        {
            return; // Database already seeded
        }

        // Seed Rooms
        var rooms = new List<Room>
        {
            new() { Number = "101", Type = RoomType.Single, Capacity = 1, PricePerNight = 150m, IsActive = true },
            new() { Number = "102", Type = RoomType.Single, Capacity = 1, PricePerNight = 150m, IsActive = true },
            new() { Number = "201", Type = RoomType.Double, Capacity = 2, PricePerNight = 250m, IsActive = true },
            new() { Number = "202", Type = RoomType.Double, Capacity = 2, PricePerNight = 250m, IsActive = true },
            new() { Number = "301", Type = RoomType.Suite, Capacity = 4, PricePerNight = 500m, IsActive = true, HasBalcony = true, HasView = true },
            new() { Number = "302", Type = RoomType.Suite, Capacity = 4, PricePerNight = 500m, IsActive = true, HasBalcony = true, HasView = true },
            new() { Number = "401", Type = RoomType.Double, Capacity = 3, PricePerNight = 300m, IsActive = true, HasView = true },
            new() { Number = "402", Type = RoomType.Double, Capacity = 3, PricePerNight = 300m, IsActive = true }
        };

        await context.Rooms.AddRangeAsync(rooms);

        // Seed Guests
        var guests = new List<Guest>
        {
            new() { FirstName = "Jan", LastName = "Kowalski", Email = "jan.kowalski@email.com", Phone = "+48123456789" },
            new() { FirstName = "Anna", LastName = "Nowak", Email = "anna.nowak@email.com", Phone = "+48987654321" },
            new() { FirstName = "Piotr", LastName = "Wiśniewski", Email = "piotr.wisniewski@email.com" },
            new() { FirstName = "Maria", LastName = "Zielińska", Email = "maria.zielinska@email.com", Phone = "+48555666777" }
        };

        await context.Guests.AddRangeAsync(guests);
        await context.SaveChangesAsync();

        // Seed Reservations (need to get IDs after save)
        var room201 = await context.Rooms.FirstAsync(r => r.Number == "201");
        var room301 = await context.Rooms.FirstAsync(r => r.Number == "301");
        var janKowalski = await context.Guests.FirstAsync(g => g.Email == "jan.kowalski@email.com");
        var annaNowak = await context.Guests.FirstAsync(g => g.Email == "anna.nowak@email.com");

        var reservations = new List<Reservation>
        {
            new()
            {
                RoomId = room201.Id,
                GuestId = janKowalski.Id,
                CheckInDate = DateTime.UtcNow.Date.AddDays(7),
                CheckOutDate = DateTime.UtcNow.Date.AddDays(10),
                GuestsCount = 2,
                TotalPrice = 3 * room201.PricePerNight, // 3 nights
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                RoomId = room301.Id,
                GuestId = annaNowak.Id,
                CheckInDate = DateTime.UtcNow.Date.AddDays(14),
                CheckOutDate = DateTime.UtcNow.Date.AddDays(19),
                GuestsCount = 3,
                TotalPrice = 5 * room301.PricePerNight, // 5 nights
                Status = ReservationStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();
    }
}
