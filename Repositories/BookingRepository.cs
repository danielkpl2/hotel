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

}