using LuxRentals.ViewModels.Cars;

namespace LuxRentals.Services.Cars;

public interface ICarInventoryService
{
    public Task<SaveResult> CreateAsync(CarUpsertVm vm);
    public Task<SaveResult> UpdateAsync(int id, CarUpsertVm vm);
}