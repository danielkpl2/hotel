using Hotel.Data;
using Hotel.Models;
using Hotel.DTOs;
using Hotel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Services;

public class HotelService
{
    private readonly HotelDbContext _context;
    private readonly IHotelRepository _hotelRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomRepository _roomRepository;

    public HotelService(
        HotelDbContext context,
        IHotelRepository hotelRepository,
        IBookingRepository bookingRepository,
        IRoomRepository roomRepository)
    {
        _context = context;
        _hotelRepository = hotelRepository;
        _bookingRepository = bookingRepository;
        _roomRepository = roomRepository;
    }

    public async Task<List<Models.Hotel>> FindHotelsByNameAsync(string name)
    {
        return await _hotelRepository.FindByNameAsync(name);
    }

    public async Task<List<Models.Hotel>> GetAllHotelsAsync()
    {
        return await _hotelRepository.GetAllAsync();
    }

    public async Task<Booking?> FindBookingByBookingReferenceAsync(string bookingReference)
    {
        return await _bookingRepository.FindByReferenceAsync(bookingReference);
    }

    public async Task<List<HotelAvailabilityDto>> FindAvailableRoomsAsync(DateOnly checkInDate, DateOnly checkOutDate, int peopleCount)
    {
        ValidateDates(checkInDate, checkOutDate);
        
        var availableRooms = await _roomRepository.GetAvailableRoomsAsync(checkInDate, checkOutDate);
        
        var nights = checkOutDate.DayNumber - checkInDate.DayNumber;
        
        var hotelAvailability = availableRooms
            .GroupBy(r => new { r.HotelId, r.Hotel.Name, r.Hotel.Address, r.Hotel.PhoneNumber, r.Hotel.Email })
            .Select(hotelGroup => 
            {
                var rooms = hotelGroup.ToList();
                var totalCapacity = rooms.Sum(r => r.RoomType.MaxOccupancy);
                
                return new HotelAvailabilityDto
                {
                    HotelId = hotelGroup.Key.HotelId,
                    HotelName = hotelGroup.Key.Name,
                    Address = hotelGroup.Key.Address,
                    PhoneNumber = hotelGroup.Key.PhoneNumber,
                    Email = hotelGroup.Key.Email,
                    AvailableRooms = rooms.Select(r => new RoomAvailabilityDto
                    {
                        RoomId = r.Id,
                        RoomNumber = r.RoomNumber,
                        RoomType = r.RoomType.Name,
                        MaxOccupancy = r.RoomType.MaxOccupancy,
                        Price = r.Price,
                        TotalPrice = r.Price * nights
                    }).ToList(),
                    TotalAvailableCapacity = totalCapacity,
                    CanAccommodateGuests = totalCapacity >= peopleCount
                };
            })
            .Where(h => h.CanAccommodateGuests)
            .OrderBy(h => h.HotelName)
            .ToList();

        return hotelAvailability;
    }

    private void ValidateDates(DateOnly checkInDate, DateOnly checkOutDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var tomorrow = today.AddDays(1);
        
        if (checkInDate < today)
        {
            throw new ArgumentException($"Check-in date cannot be in the past. Today is {today}");
        }

        if (checkOutDate < tomorrow)
        {
            throw new ArgumentException($"Check-out date must be at least tomorrow. Today is {today}");
        }

        if (checkInDate >= checkOutDate)
        {
            throw new ArgumentException("Check-in date must be before check-out date");
        }
    }

    public async Task<(bool IsAvailable, List<string> Issues)> ValidateBookingAsync(
        int hotelId,
        List<int> roomIds, 
        DateOnly checkInDate, 
        DateOnly checkOutDate,
        int peopleCount)
    {
        var issues = new List<string>();

        // 1. Validate dates using the existing method
        try
        {
            ValidateDates(checkInDate, checkOutDate);
        }
        catch (ArgumentException ex)
        {
            issues.Add(ex.Message);
        }

        // Early return if basic date validation fails
        if (issues.Any())
        {
            return (false, issues);
        }

        // 2. Validate room selection
        if (roomIds == null || !roomIds.Any())
        {
            issues.Add("At least one room must be selected");
            return (false, issues);
        }

        // 3. Check for duplicate room IDs
        var duplicateRoomIds = roomIds.GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateRoomIds.Any())
        {
            issues.Add($"Duplicate room IDs found: {string.Join(", ", duplicateRoomIds)}");
        }

        // 4. Check if hotel exists
        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        if (hotel == null)
        {
            issues.Add("Hotel not found");
            return (false, issues);
        }

        // 5. Get the requested rooms with their types
        var rooms = await _roomRepository.GetRoomsWithTypesByIdsAndHotelAsync(roomIds, hotelId);

        // 6. Check if all rooms exist and belong to the hotel
        var foundRoomIds = rooms.Select(r => r.Id).ToList();
        var missingRoomIds = roomIds.Except(foundRoomIds).ToList();
        
        if (missingRoomIds.Any())
        {
            issues.Add($"Rooms with IDs {string.Join(", ", missingRoomIds)} not found or don't belong to this hotel");
        }

        if (rooms.Count != roomIds.Count)
        {
            return (false, issues);
        }

        // 7. Validate occupancy
        var totalCapacity = rooms.Sum(r => r.RoomType.MaxOccupancy);
        if (totalCapacity < peopleCount)
        {
            var roomDetails = rooms.Select(r => $"Room {r.RoomNumber} ({r.RoomType.Name}, max {r.RoomType.MaxOccupancy} people)");
            issues.Add($"Selected rooms can only accommodate {totalCapacity} people, but {peopleCount} requested. " +
                      $"Rooms: {string.Join(", ", roomDetails)}");
        }

        // 8. Check availability for each room during the specified dates
        var unavailableRooms = new List<string>();
        
        foreach (var room in rooms)
        {
            var conflictingBookings = await _bookingRepository.GetBookingsForRoomAndDatesAsync(room.Id, checkInDate, checkOutDate);

            if (conflictingBookings.Any())
            {
                var conflicts = conflictingBookings.Select(b => 
                    $"Booking {b.BookingReference} ({b.GuestName}) from {b.CheckInDate} to {b.CheckOutDate}");
                unavailableRooms.Add($"Room {room.RoomNumber}: {string.Join(", ", conflicts)}");
            }
        }

        if (unavailableRooms.Any())
        {
            issues.Add($"The following rooms are not available: {string.Join("; ", unavailableRooms)}");
        }

        return (issues.Count == 0, issues);
    }

    public async Task<Booking> CreateBookingAsync(
        int hotelId,
        List<int> roomIds,
        string guestName,
        int peopleCount,
        DateOnly checkInDate,
        DateOnly checkOutDate)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Use the comprehensive validation method
            var (isAvailable, issues) = await ValidateBookingAsync(
                hotelId, roomIds, checkInDate, checkOutDate, peopleCount);

            if (!isAvailable)
            {
                throw new InvalidOperationException($"Booking validation failed: {string.Join("; ", issues)}");
            }

            // Get the rooms (we know they exist and are valid from the validation above)
            var rooms = await _roomRepository.GetRoomsWithTypesByIdsAndHotelAsync(roomIds, hotelId);

            // Calculate total price
            var nights = checkOutDate.DayNumber - checkInDate.DayNumber;
            var totalPrice = rooms.Sum(r => r.Price) * nights;

            // Create the booking
            var booking = new Booking
            {
                HotelId = hotelId,
                GuestName = guestName,
                PeopleCount = peopleCount,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                TotalPrice = totalPrice,
                BookingReference = GenerateBookingReference(),
                Rooms = rooms
            };

            // Save to database
            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();
            
            await transaction.CommitAsync();
            return booking;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private string GenerateBookingReference()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(100, 999);
        return $"BK{timestamp}{random}";
    }
}