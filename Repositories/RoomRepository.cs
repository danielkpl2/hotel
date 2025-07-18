using Hotel.Data;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly HotelDbContext _context;

    public RoomRepository(HotelDbContext context)
    {
        _context = context;
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

    public async Task<List<Room>> GetRoomsByIdsAndHotelAsync(List<int> roomIds, int hotelId)
    {
        return await _context.Rooms
            .Where(r => roomIds.Contains(r.Id) && r.HotelId == hotelId)
            .ToListAsync();
    }

    public async Task<List<Room>> GetRoomsWithTypesByIdsAndHotelAsync(List<int> roomIds, int hotelId)
    {
        return await _context.Rooms
            .Include(r => r.RoomType)
            .Where(r => roomIds.Contains(r.Id) && r.HotelId == hotelId)
            .ToListAsync();
    }
}