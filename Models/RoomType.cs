namespace Hotel.Models;
using System.Text.Json.Serialization;


public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxOccupancy { get; set; }

    [JsonIgnore]
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}