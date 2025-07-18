using Hotel.Models;

namespace Hotel.DTOs;

public class RoomAvailabilityDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int MaxOccupancy { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}