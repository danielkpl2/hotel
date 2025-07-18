namespace Hotel.Models;
using System.Text.Json.Serialization;

public class Room
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int HotelId { get; set; }

    public int RoomTypeId { get; set; }

    public Hotel Hotel { get; set; } = null!;

    public RoomType RoomType { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

}