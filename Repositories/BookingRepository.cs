using Hotel.Data;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly HotelDbContext _context;

    public BookingRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> FindByReferenceAsync(string bookingReference)
    {
        return await _context.Bookings
            .Include(b => b.Hotel)
            .Include(b => b.Rooms)
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference);
    }

    public async Task<List<Booking>> GetBookingsForRoomAndDatesAsync(int roomId, DateOnly checkIn, DateOnly checkOut)
    {
        return await _context.Bookings
            .Where(b => b.Rooms.Any(r => r.Id == roomId))
            .Where(b => b.CheckInDate < checkOut && b.CheckOutDate > checkIn)
            .ToListAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}