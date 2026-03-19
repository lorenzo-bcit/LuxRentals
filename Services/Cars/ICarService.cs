using LuxRentals.Models;
using LuxRentals.Repositories;
using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars;
using LuxRentals.ViewModels.Cars.Admin;

namespace LuxRentals.Services.Cars;

public interface ICarService
{
    // Cars
    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria);
    public Task<Car?> GetByIdAsync(int id);
    public Task<IReadOnlyList<Booking>> GetActiveOrUpcomingBookingsAsync(int carId);
    public Task<SaveResult> CreateAsync(CarEditVm vm);
    public Task<SaveResult> UpdateAsync(int id, CarEditVm vm);
    public Task<SaveResult> DeleteAsync(int id);

    // Lookup data
    public Task<List<FuelType>> GetFuelTypesAsync();
    public Task<List<VehicleClass>> GetVehicleClassesAsync();
    public Task<List<CarStatus>> GetCarStatusesAsync();

    // Makes
    public Task<IReadOnlyList<MakeListItemVm>> GetMakeListAsync();
    public Task<IReadOnlyList<Make>> GetMakeOptionsAsync();
    public Task<Make?> GetMakeByIdAsync(int id);
    public Task<SaveResult> CreateMakeAsync(string makeName);
    public Task<SaveResult> UpdateMakeAsync(int id, string makeName);
    public Task<SaveResult> DeleteMakeAsync(int id);

    // Models
    public Task<List<Model>> GetModelsAsync(int? makeId = null);
    public Task<IReadOnlyList<ModelListItemVm>> GetModelListAsync();
    public Task<Model?> GetModelByIdAsync(int id);
    public Task<SaveResult> CreateModelAsync(int? makeId, string modelName);
    public Task<SaveResult> UpdateModelAsync(int id, int? makeId, string modelName);
    public Task<SaveResult> DeleteModelAsync(int id);

    // Vehicle classes
    public Task<IReadOnlyList<VehicleClassListItemVm>> GetVehicleClassListAsync();
    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id);
    public Task<SaveResult> CreateVehicleClassAsync(string vehicleClassName);
    public Task<SaveResult> UpdateVehicleClassAsync(int id, string vehicleClassName);
    public Task<SaveResult> DeleteVehicleClassAsync(int id);
}
