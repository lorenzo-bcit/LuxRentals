using LuxRentals.Models;

namespace LuxRentals.Repositories.Cars;

public interface ICarWriteRepository
{
    public Task AddAsync(Car car);
    public Task SaveChangesAsync();

    public Task<bool> VinExistsAsync(string vin, int? excludeCarId);
    public Task<bool> PlateExistsAsync(string plate, int? excludeCarId);
}
