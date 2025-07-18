using Hotel.Models;

namespace Hotel.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAvailableRoomsAsync(DateOnly checkInDate, DateOnly checkOutDate);
    Task<List<Room>> GetRoomsByIdsAndHotelAsync(List<int> roomIds, int hotelId);
    Task<List<Room>> GetRoomsWithTypesByIdsAndHotelAsync(List<int> roomIds, int hotelId);
}