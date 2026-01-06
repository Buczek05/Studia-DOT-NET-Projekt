using Hotel.Application.Entities;

namespace Hotel.Application.Interfaces;

public interface IGuestRepository : IRepository<Guest>
{
    Task<Guest?> GetByEmailAsync(string email);
}
