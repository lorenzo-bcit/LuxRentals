using LuxRentals.Models;
using LuxRentals.ViewModels.Cars;

namespace LuxRentals.Repositories.Cars;

public interface ICarReadRepository
{
    public Task<PagedList<Car>> SearchAsync(CarSearchVm vm);
    public Task<Car?> GetByIdAsync(int id);
    public Task<bool> VinExistsAsync(string vin, int? excludeCarId);
    public Task<bool> PlateExistsAsync(string plate, int? excludeCarId);
}
