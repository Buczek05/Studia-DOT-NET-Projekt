namespace Hotel.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRoomRepository Rooms { get; }
    IGuestRepository Guests { get; }
    IReservationRepository Reservations { get; }
    Task<int> SaveChangesAsync();
}
