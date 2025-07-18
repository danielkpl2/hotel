using Hotel.Data;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services;

public class SeedService
{
    private readonly HotelDbContext _context;

    public SeedService(HotelDbContext context)
    {
        _context = context;
    }

    public async Task SeedFromSqlFileAsync(string fileName)
    {
        var filePath = Path.Combine("SeedData", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"SQL file not found: {filePath}");
        }

        var sql = await File.ReadAllTextAsync(filePath);

        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new InvalidOperationException("SQL file is empty");
        }

        // Execute the entire SQL file as-is
        await _context.Database.ExecuteSqlRawAsync(sql);
    }

    public async Task ClearAllDataAsync()
    {
        var sql = @"
            DELETE FROM ""BookingRooms"";
            DELETE FROM ""Bookings"";
            DELETE FROM ""Rooms"";
            DELETE FROM ""Hotels"";
            DELETE FROM ""RoomTypes"";
        ";

        await _context.Database.ExecuteSqlRawAsync(sql);
    }
    
    public async Task<object> GetDataSummaryAsync()
    {
        return new
        {
            HotelCount = await _context.Hotels.CountAsync(),
            RoomCount = await _context.Rooms.CountAsync(),
            BookingCount = await _context.Bookings.CountAsync(),
            RoomTypeCount = await _context.RoomTypes.CountAsync()
        };
    }

    
}