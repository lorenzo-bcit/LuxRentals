using LuxRentals.Models;

namespace LuxRentals.Repositories.Cars;

public interface ICarLookupAdminRepository
{
    public Task<List<Make>> GetMakesAsync();
    public Task<Make?> GetMakeByIdAsync(int id);
    public Task<bool> MakeNameExistsAsync(string makeName, int? excludeMakeId = null);
    public Task<bool> MakeHasModelsAsync(int makeId);
    public Task AddMakeAsync(Make make);
    public void RemoveMake(Make make);

    public Task<List<Model>> GetModelsAsync();
    public Task<Model?> GetModelByIdAsync(int id);
    public Task<bool> ModelNameExistsAsync(int makeId, string modelName, int? excludeModelId = null);
    public Task<bool> ModelHasCarsAsync(int modelId);
    public Task AddModelAsync(Model model);
    public void RemoveModel(Model model);

    public Task<List<VehicleClass>> GetVehicleClassesAsync();
    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id);
    public Task<bool> VehicleClassNameExistsAsync(string vehicleClassName, int? excludeVehicleClassId = null);
    public Task<bool> VehicleClassHasCarsAsync(int vehicleClassId);
    public Task AddVehicleClassAsync(VehicleClass vehicleClass);
    public void RemoveVehicleClass(VehicleClass vehicleClass);

    public Task<bool> MakeExistsAsync(int makeId);
    public Task SaveChangesAsync();
}
