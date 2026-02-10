using LuxRentals.ViewModels.Cars;

namespace LuxRentals.Services.Cars;

public interface ICarInventoryService
{
    public Task<SaveResult> UpsertAsync(CarUpsertVm vm);
}