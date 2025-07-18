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

    public async Task<List<Room>> GetAvailableRoomsAsync(DateOnly checkInDate, DateOnly checkOutDate)
    {
        return await _context.Rooms
            .Include(r => r.Hotel)
            .Include(r => r.RoomType)
            .Where(r => !r.Bookings.Any(b =>
                b.CheckInDate < checkOutDate &&
                b.CheckOutDate > checkInDate))
            .OrderBy(r => r.Hotel.Name)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();
    }
}