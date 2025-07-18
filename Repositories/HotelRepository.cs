using Hotel.Data;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public HotelRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<Models.Hotel>> GetAllAsync()
    {
        return await _context.Hotels
            .Include(h => h.Rooms)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<List<Models.Hotel>> FindByNameAsync(string name)
    {
        return await _context.Hotels
            .Where(h => h.Name.ToLower().Contains(name.ToLower()))
            .Include(h => h.Rooms)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Models.Hotel?> GetByIdAsync(int id)
    {
        return await _context.Hotels
            .Include(h => h.Rooms)
            .ThenInclude(r => r.RoomType)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

}