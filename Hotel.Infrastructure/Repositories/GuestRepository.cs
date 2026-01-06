using Hotel.Application.Entities;
using Hotel.Application.Interfaces;
using Hotel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositories;

public class GuestRepository : RepositoryBase<Guest>, IGuestRepository
{
    public GuestRepository(HotelDbContext context) : base(context)
    {
    }

    public async Task<Guest?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(g => g.Email == email);
    }
}
