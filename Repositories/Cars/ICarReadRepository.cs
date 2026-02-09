using LuxRentals.Models;
using LuxRentals.ViewModels.Cars;

namespace LuxRentals.Repositories.Cars;

public interface ICarReadRepository
{
    public Task<List<Car>> SearchAsync(CarSearchVm vm);
    public Task<Car?> GetByIdAsync(int id);
}
