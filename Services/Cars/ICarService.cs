using LuxRentals.Models;
using LuxRentals.Repositories;
using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars;
using LuxRentals.ViewModels.Cars.Admin;

namespace LuxRentals.Services.Cars;

public interface ICarService
{
    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria);
    public Task<Car?> GetByIdAsync(int id);
    public Task<List<FuelType>> GetFuelTypesAsync();
    public Task<List<VehicleClass>> GetVehicleClassesAsync();
    public Task<List<CarStatus>> GetCarStatusesAsync();
    public Task<List<Model>> GetModelsAsync(int? makeId = null);

    public Task<SaveResult> CreateAsync(CarUpsertVm vm);
    public Task<SaveResult> UpdateAsync(int id, CarUpsertVm vm);
    public Task<SaveResult> DeleteAsync(int id);

    public Task<IReadOnlyList<AdminMakeListItemVm>> GetMakesAsync();
    public Task<Make?> GetMakeByIdAsync(int id);
    public Task<SaveResult> CreateMakeAsync(string makeName);
    public Task<SaveResult> UpdateMakeAsync(int id, string makeName);
    public Task<SaveResult> DeleteMakeAsync(int id);

    public Task<IReadOnlyList<AdminModelListItemVm>> GetAdminModelsAsync();
    public Task<Model?> GetModelByIdAsync(int id);
    public Task<SaveResult> CreateModelAsync(int? makeId, string modelName);
    public Task<SaveResult> UpdateModelAsync(int id, int? makeId, string modelName);
    public Task<SaveResult> DeleteModelAsync(int id);

    public Task<IReadOnlyList<Make>> GetMakeOptionsAsync();

    public Task<IReadOnlyList<AdminVehicleClassListItemVm>> GetAdminVehicleClassesAsync();
    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id);
    public Task<SaveResult> CreateVehicleClassAsync(string vehicleClassName);
    public Task<SaveResult> UpdateVehicleClassAsync(int id, string vehicleClassName);
    public Task<SaveResult> DeleteVehicleClassAsync(int id);
}
