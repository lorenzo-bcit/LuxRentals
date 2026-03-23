using LuxRentals.Models;

namespace LuxRentals.Repositories.Cars;

public interface ICarRepository
{
    // Cars
    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria);
    public Task<Car?> GetByIdAsync(int id);
    public Task<bool> VinExistsAsync(string vin, int? excludeCarId);
    public Task<bool> PlateExistsAsync(string plate, int? excludeCarId);
    public Task<bool> HasBookingsAsync(int carId);
    public Task<int> CountActiveOrUpcomingBookingsAsync(int carId);
    public Task<List<Booking>> GetActiveOrUpcomingBookingsAsync(int carId);
    public Task AddAsync(Car car);
    public void Remove(Car car);

    // Lookup data
    public Task<List<FuelType>> GetFuelTypesAsync();
    public Task<List<VehicleClass>> GetVehicleClassesAsync();
    public Task<List<CarStatus>> GetCarStatusesAsync();
    public Task<int?> GetCarStatusIdByNameAsync(string statusName);

    // Makes
    public Task<List<Make>> GetMakesAsync();
    public Task<Make?> GetMakeByIdAsync(int id);
    public Task<bool> MakeExistsAsync(int makeId);
    public Task<bool> MakeNameExistsAsync(string makeName, int? excludeMakeId = null);
    public Task<bool> MakeHasModelsAsync(int makeId);
    public Task AddMakeAsync(Make make);
    public void RemoveMake(Make make);

    // Models
    public Task<List<Model>> GetModelsAsync(int? makeId = null);
    public Task<Model?> GetModelByIdAsync(int id);
    public Task<bool> ModelNameExistsAsync(int makeId, string modelName, int? excludeModelId = null);
    public Task<bool> ModelHasCarsAsync(int modelId);
    public Task AddModelAsync(Model model);
    public void RemoveModel(Model model);

    // Vehicle classes
    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id);
    public Task<bool> VehicleClassNameExistsAsync(string vehicleClassName, int? excludeVehicleClassId = null);
    public Task<bool> VehicleClassHasCarsAsync(int vehicleClassId);
    public Task AddVehicleClassAsync(VehicleClass vehicleClass);
    public void RemoveVehicleClass(VehicleClass vehicleClass);

    public Task SaveChangesAsync();
}
