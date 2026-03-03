using LuxRentals.Models;

namespace LuxRentals.Repositories.Cars;

public interface ICarReadRepository
{
    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria);
    public Task<Car?> GetByIdAsync(int id);
    public Task<bool> VinExistsAsync(string vin, int? excludeCarId);
    public Task<bool> PlateExistsAsync(string plate, int? excludeCarId);
}
