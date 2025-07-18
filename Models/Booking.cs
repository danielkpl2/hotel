namespace Hotel.Models;

public class Booking
{
    public int Id { get; set; }

    public Hotel Hotel { get; set; } = null!;

    public int HotelId { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();

    public int PeopleCount { get; set; }

    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal TotalPrice { get; set; }

    public string BookingReference { get; set; } = string.Empty;


}