using Hotel.Models;

namespace Hotel.DTOs;

public class HotelAvailabilityDto
{
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<RoomAvailabilityDto> AvailableRooms { get; set; } = new();
    public int TotalAvailableCapacity { get; set; }
    public bool CanAccommodateGuests { get; set; }
}