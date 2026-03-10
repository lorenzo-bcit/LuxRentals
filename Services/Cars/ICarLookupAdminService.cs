using LuxRentals.Models;
using LuxRentals.ViewModels.Cars.Admin;

namespace LuxRentals.Services.Cars;

public interface ICarLookupAdminService
{
    public Task<IReadOnlyList<AdminMakeListItemVm>> GetMakesAsync();
    public Task<Make?> GetMakeByIdAsync(int id);
    public Task<SaveResult> CreateMakeAsync(string makeName);
    public Task<SaveResult> UpdateMakeAsync(int id, string makeName);
    public Task<SaveResult> DeleteMakeAsync(int id);

    public Task<IReadOnlyList<AdminModelListItemVm>> GetModelsAsync();
    public Task<Model?> GetModelByIdAsync(int id);
    public Task<SaveResult> CreateModelAsync(int? makeId, string modelName);
    public Task<SaveResult> UpdateModelAsync(int id, int? makeId, string modelName);
    public Task<SaveResult> DeleteModelAsync(int id);

    public Task<IReadOnlyList<Make>> GetMakeOptionsAsync();

    public Task<IReadOnlyList<AdminVehicleClassListItemVm>> GetVehicleClassesAsync();
    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id);
    public Task<SaveResult> CreateVehicleClassAsync(string vehicleClassName);
    public Task<SaveResult> UpdateVehicleClassAsync(int id, string vehicleClassName);
    public Task<SaveResult> DeleteVehicleClassAsync(int id);
}
