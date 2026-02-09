using LuxRentals.Models;

namespace LuxRentals.Repositories.Cars;

public interface ICarLookupRepository
{
    public Task<List<FuelType>> GetFuelTypesAsync();
    public Task<List<VehicleClass>> GetVehicleClassesAsync();
    public Task<List<CarStatus>> GetCarStatusesAsync();
    public Task<List<Make>> GetMakesAsync();
    public Task<List<Model>> GetModelsAsync(int? makeId = null);
}