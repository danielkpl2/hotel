using Hotel.Models;

namespace Hotel.Repositories;

public interface IRoomRepository
{
    Task<List<Room>> GetAvailableRoomsAsync(DateOnly checkInDate, DateOnly checkOutDate, int peopleCount);
    // Task<List<Room>> GetRoomsByIdsAndHotelAsync(List<int> roomIds, int hotelId);
    // Task<Room?> GetByIdAsync(int id);
}