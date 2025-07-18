using Microsoft.AspNetCore.Mvc;
using Hotel.Services;
using Hotel.DTOs;

namespace Hotel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelController : ControllerBase
{
    private readonly HotelService _hotelService;

    public HotelController(HotelService hotelService)
    {
        _hotelService = hotelService;
    }

    /// <summary>
    /// Search for hotels by name. If no name is provided, returns all hotels.
    /// </summary>
    /// <param name="name">Optional hotel name to search for (case-insensitive, partial match).</param>
    /// <returns>List of hotels matching the search criteria.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchHotelsByName([FromQuery] string? name)
    {
        List<Models.Hotel> hotels;

        if (string.IsNullOrWhiteSpace(name))
        {
            // Return all hotels if name is empty or not provided
            hotels = await _hotelService.GetAllHotelsAsync();
        }
        else
        {
            // Search by name if provided
            hotels = await _hotelService.FindHotelsByNameAsync(name);
        }

        return Ok(hotels);
    }

    /// <summary>
    /// Get a booking by its booking reference.
    /// </summary>
    /// <param name="bookingReference">The booking reference string.</param>
    /// <returns>The booking details if found, otherwise 404 Not Found.</returns>
    [HttpGet("bookings/{bookingReference}")]
    public async Task<IActionResult> GetBookingByReference(string bookingReference)
    {
        var booking = await _hotelService.FindBookingByBookingReferenceAsync(bookingReference);
        if (booking == null)
        {
            return NotFound(new { error = "Booking not found" });
        }
        return Ok(booking);
    }

    /// <summary>
    /// Get all hotels with available rooms for the given date range and people count.
    /// </summary>
    /// <param name="checkInDate">Check-in date (YYYY-MM-DD).</param>
    /// <param name="checkOutDate">Check-out date (YYYY-MM-DD).</param>
    /// <param name="peopleCount">Number of people to accommodate.</param>
    /// <returns>
    /// An object containing search criteria, list of hotels with available rooms, 
    /// total hotels found, and total rooms available.
    /// </returns>
    [HttpGet("bookings/available-rooms")]
    public async Task<IActionResult> GetAvailableRooms(
        [FromQuery] DateOnly checkInDate,
        [FromQuery] DateOnly checkOutDate,
        [FromQuery] int peopleCount)
    {
        try
        {
            var availableHotels = await _hotelService.FindAvailableRoomsAsync(checkInDate, checkOutDate, peopleCount);
            
            var response = new
            {
                searchCriteria = new
                {
                    checkInDate = checkInDate.ToString("yyyy-MM-dd"),
                    checkOutDate = checkOutDate.ToString("yyyy-MM-dd"),
                    peopleCount = peopleCount,
                    nights = checkOutDate.DayNumber - checkInDate.DayNumber
                },
                hotels = availableHotels,
                totalHotelsFound = availableHotels.Count,
                totalRoomsAvailable = availableHotels.Sum(h => h.AvailableRooms.Count)
            };
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while searching for available rooms" });
        }
    }

    /// <summary>
    /// Create a new booking for the specified hotel, rooms, guest, and dates.
    /// </summary>
    /// <param name="request">Booking request details (hotel, rooms, guest, people count, dates).</param>
    /// <returns>
    /// The created booking if successful (201 Created), 
    /// 400 Bad Request for invalid input, 
    /// or 409 Conflict if rooms are not available.
    /// </returns>
    [HttpPost("bookings/book")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
    {
        try
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Parse dates
            if (!DateOnly.TryParse(request.CheckInDate, out var checkInDate))
            {
                return BadRequest(new { error = "Invalid check-in date format. Use YYYY-MM-DD" });
            }

            if (!DateOnly.TryParse(request.CheckOutDate, out var checkOutDate))
            {
                return BadRequest(new { error = "Invalid check-out date format. Use YYYY-MM-DD" });
            }

            var booking = await _hotelService.CreateBookingAsync(
                request.HotelId, 
                request.RoomIds, 
                request.GuestName, 
                request.PeopleCount, 
                checkInDate, 
                checkOutDate);
            
            return CreatedAtAction(nameof(GetBookingByReference), 
                new { bookingReference = booking.BookingReference }, booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while creating the booking" });
        }
    }
    
}