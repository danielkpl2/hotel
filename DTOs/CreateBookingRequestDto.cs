using System.ComponentModel.DataAnnotations;

namespace Hotel.DTOs;

public class CreateBookingRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Hotel ID must be a positive number")]
    public int HotelId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one room must be selected")]
    public List<int> RoomIds { get; set; } = new();

    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Guest name must be between 1 and 100 characters")]
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [Range(1, 20, ErrorMessage = "People count must be between 1 and 20")]
    public int PeopleCount { get; set; }

    [Required]
    public string CheckInDate { get; set; } = string.Empty;

    [Required]
    public string CheckOutDate { get; set; } = string.Empty;
}