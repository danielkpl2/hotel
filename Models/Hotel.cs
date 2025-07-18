namespace Hotel.Models;
using System.Text.Json.Serialization;


public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}