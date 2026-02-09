using LuxRentals.Models;

namespace LuxRentals.Repositories.Cars;

public interface ICarWriteRepository
{
    public Task AddAsync(Car car);
    public Task SaveChangesAsync();
}
