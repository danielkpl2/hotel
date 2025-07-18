using Hotel.Models;

namespace Hotel.Repositories;

public interface IHotelRepository
{
    Task<List<Models.Hotel>> GetAllAsync();
    Task<List<Models.Hotel>> FindByNameAsync(string name);
    Task<Models.Hotel?> GetByIdAsync(int id);
}