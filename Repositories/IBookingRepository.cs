using Hotel.Models;

namespace Hotel.Repositories;

public interface IBookingRepository
{
    Task<Booking?> FindByReferenceAsync(string bookingReference);
    Task<List<Booking>> GetBookingsForRoomAndDatesAsync(int roomId, DateOnly checkIn, DateOnly checkOut);
    Task AddAsync(Booking booking);
    Task SaveChangesAsync();
}