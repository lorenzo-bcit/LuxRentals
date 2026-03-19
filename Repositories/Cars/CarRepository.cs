using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Cars;

public class CarRepository : ICarRepository
{
    private readonly LuxRentalsDbContext _db;
    public CarRepository(LuxRentalsDbContext db) => _db = db;

    // Cars
    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria)
    {
        var cars = BuildBaseQuery();
        cars = ApplyAttributeFilters(cars, criteria);
        cars = ApplyAvailabilityFilter(cars, criteria);
        cars = ApplySorting(cars, criteria);

        return PagedList<Car>.CreateAsync(cars, criteria.Page, criteria.PageSize);
    }

    private IQueryable<Car> BuildBaseQuery() =>
        _db.Cars
            .AsNoTracking()
            .Include(c => c.FkModel).ThenInclude(m => m.FkMake)
            .Include(c => c.FkFuelType)
            .Include(c => c.FkVehicleClass)
            .Include(c => c.FkCarStatus);

    private static IQueryable<Car> ApplyAttributeFilters(IQueryable<Car> cars, CarSearchCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.Trim();
            var pattern = $"%{searchTerm}%";

            cars = cars.Where(c =>
                EF.Functions.Like(c.LicencePlate, pattern) ||
                EF.Functions.Like(c.VinNumber, pattern) ||
                EF.Functions.Like(c.FkModel.ModelName, pattern) ||
                EF.Functions.Like(c.FkModel.FkMake.MakeName, pattern));
        }

        if (criteria.StatusId != null)
            cars = cars.Where(c => c.FkCarStatusId == criteria.StatusId);

        if (criteria.MakeId != null)
            cars = cars.Where(c => c.FkModel.FkMakeId == criteria.MakeId);

        if (criteria.ModelId != null)
            cars = cars.Where(c => c.FkModelId == criteria.ModelId);

        if (criteria.FuelTypeId != null)
            cars = cars.Where(c => c.FkFuelTypeId == criteria.FuelTypeId);

        if (criteria.VehicleClassId != null)
            cars = cars.Where(c => c.FkVehicleClassId == criteria.VehicleClassId);

        if (criteria.TransmissionType != null)
            cars = cars.Where(c => c.TransmissionType == criteria.TransmissionType);

        if (criteria.MinSeats != null)
            cars = cars.Where(c => c.PersonCap >= criteria.MinSeats);

        if (criteria.MinLuggage != null)
            cars = cars.Where(c => c.LuggageCap >= criteria.MinLuggage);

        if (criteria.MaxRate != null)
            cars = cars.Where(c => c.DailyRate <= criteria.MaxRate);

        return cars;
    }

    private IQueryable<Car> ApplyAvailabilityFilter(IQueryable<Car> cars, CarSearchCriteria criteria)
    {
        if (!criteria.AvailableOnly)
        {
            if (criteria.HasActiveOrUpcomingBookingsOnly)
            {
                var utcNow = DateTime.UtcNow;
                cars = cars.Where(c =>
                    _db.Bookings.Any(b =>
                        b.FkCarId == c.PkCarId &&
                        b.CancelledAt == null &&
                        b.EndDateTime > utcNow));
            }

            return cars;
        }

        cars = cars.Where(c => c.FkCarStatus.StatusFlag == "Available");

        if (criteria.StartDate is null || criteria.EndDate is null)
            return cars;

        var start = criteria.StartDate.Value;
        var end = criteria.EndDate.Value;

        return cars.Where(c =>
            !_db.Bookings.Any(b =>
                b.FkCarId == c.PkCarId &&
                b.CancelledAt == null &&
                b.StartDateTime < end &&
                b.EndDateTime > start));
    }

    private static IQueryable<Car> ApplySorting(IQueryable<Car> cars, CarSearchCriteria criteria)
    {
        var sortBy = criteria.SortBy?.ToLowerInvariant();
        var descending = criteria.SortDescending;

        return sortBy switch
        {
            "rate" => descending
                ? cars.OrderByDescending(c => c.DailyRate).ThenByDescending(c => c.PkCarId)
                : cars.OrderBy(c => c.DailyRate).ThenBy(c => c.PkCarId),

            "year" => descending
                ? cars.OrderByDescending(c => c.Year)
                    .ThenBy(c => c.FkModel.FkMake.MakeName)
                    .ThenBy(c => c.FkModel.ModelName)
                : cars.OrderBy(c => c.Year)
                    .ThenBy(c => c.FkModel.FkMake.MakeName)
                    .ThenBy(c => c.FkModel.ModelName),

            "make" => descending
                ? cars.OrderByDescending(c => c.FkModel.FkMake.MakeName)
                    .ThenByDescending(c => c.FkModel.ModelName)
                    .ThenByDescending(c => c.Year)
                : cars.OrderBy(c => c.FkModel.FkMake.MakeName)
                    .ThenBy(c => c.FkModel.ModelName)
                    .ThenByDescending(c => c.Year),

            "status" => descending
                ? cars.OrderByDescending(c => c.FkCarStatus.StatusFlag)
                    .ThenBy(c => c.FkModel.FkMake.MakeName)
                    .ThenBy(c => c.FkModel.ModelName)
                : cars.OrderBy(c => c.FkCarStatus.StatusFlag)
                    .ThenBy(c => c.FkModel.FkMake.MakeName)
                    .ThenBy(c => c.FkModel.ModelName),

            "id" => descending
                ? cars.OrderByDescending(c => c.PkCarId)
                : cars.OrderBy(c => c.PkCarId),

            _ => cars.OrderBy(c => c.PkCarId)
        };
    }

    public Task<Car?> GetByIdAsync(int id)
    {
        return _db.Cars
            .Include(c => c.FkModel).ThenInclude(m => m.FkMake)
            .Include(c => c.FkFuelType)
            .Include(c => c.FkVehicleClass)
            .Include(c => c.FkCarStatus)
            .FirstOrDefaultAsync(c => c.PkCarId == id);
    }

    public Task<bool> VinExistsAsync(string vin, int? excludeCarId) =>
        _db.Cars.AnyAsync(c => c.VinNumber == vin && (excludeCarId == null || c.PkCarId != excludeCarId));

    public Task<bool> PlateExistsAsync(string plate, int? excludeCarId) =>
        _db.Cars.AnyAsync(c => c.LicencePlate == plate && (excludeCarId == null || c.PkCarId != excludeCarId));

    public Task<bool> HasBookingsAsync(int carId) =>
        _db.Bookings.AnyAsync(b => b.FkCarId == carId);

    public Task<int> CountActiveOrUpcomingBookingsAsync(int carId, DateTime utcNow) =>
        _db.Bookings.CountAsync(b =>
            b.FkCarId == carId &&
            b.CancelledAt == null &&
            b.EndDateTime > utcNow);

    public Task<List<Booking>> GetActiveOrUpcomingBookingsAsync(int carId, DateTime utcNow) =>
        _db.Bookings
            .AsNoTracking()
            .Include(b => b.FkCustomer)
            .Where(b =>
                b.FkCarId == carId &&
                b.CancelledAt == null &&
                b.EndDateTime > utcNow)
            .OrderBy(b => b.StartDateTime)
            .ToListAsync();

    public Task AddAsync(Car car) => _db.Cars.AddAsync(car).AsTask();

    public void Remove(Car car) => _db.Cars.Remove(car);

    // Lookup data
    public Task<List<FuelType>> GetFuelTypesAsync() =>
        _db.FuelTypes
            .AsNoTracking()
            .OrderBy(x => x.FuelType1)
            .ToListAsync();

    public Task<List<VehicleClass>> GetVehicleClassesAsync() =>
        _db.VehicleClasses
            .AsNoTracking()
            .OrderBy(x => x.VehicleClass1)
            .ToListAsync();

    public Task<List<CarStatus>> GetCarStatusesAsync() =>
        _db.CarStatuses
            .AsNoTracking()
            .OrderBy(x => x.StatusFlag)
            .ToListAsync();

    public async Task<int?> GetCarStatusIdByNameAsync(string statusName)
    {
        return await _db.CarStatuses
            .AsNoTracking()
            .Where(x => x.StatusFlag == statusName)
            .Select(x => (int?)x.PkCarStatusId)
            .FirstOrDefaultAsync();
    }

    // Makes
    public Task<List<Make>> GetMakesAsync() =>
        _db.Makes
            .AsNoTracking()
            .Include(x => x.Models)
            .OrderBy(x => x.MakeName)
            .ToListAsync();

    public Task<Make?> GetMakeByIdAsync(int id) =>
        _db.Makes.FirstOrDefaultAsync(x => x.PkMakeId == id);

    public Task<bool> MakeExistsAsync(int makeId) =>
        _db.Makes.AnyAsync(x => x.PkMakeId == makeId);

    public Task<bool> MakeNameExistsAsync(string makeName, int? excludeMakeId = null) =>
        _db.Makes.AnyAsync(x =>
            x.PkMakeId != excludeMakeId &&
            x.MakeName.ToUpper() == makeName.ToUpper());

    public Task<bool> MakeHasModelsAsync(int makeId) =>
        _db.Models.AnyAsync(x => x.FkMakeId == makeId);

    public Task AddMakeAsync(Make make) =>
        _db.Makes.AddAsync(make).AsTask();

    public void RemoveMake(Make make) => _db.Makes.Remove(make);

    // Models
    public Task<List<Model>> GetModelsAsync(int? makeId = null)
    {
        var q = _db.Models
            .AsNoTracking()
            .Include(x => x.FkMake)
            .Include(x => x.Cars)
            .AsQueryable();

        if (makeId is not null)
            q = q.Where(x => x.FkMakeId == makeId);

        return q.OrderBy(x => x.FkMake.MakeName)
            .ThenBy(x => x.ModelName)
            .ToListAsync();
    }

    public Task<Model?> GetModelByIdAsync(int id) =>
        _db.Models.FirstOrDefaultAsync(x => x.PkModelId == id);

    public Task<bool> ModelNameExistsAsync(int makeId, string modelName, int? excludeModelId = null) =>
        _db.Models.AnyAsync(x =>
            x.PkModelId != excludeModelId &&
            x.FkMakeId == makeId &&
            x.ModelName.ToUpper() == modelName.ToUpper());

    public Task<bool> ModelHasCarsAsync(int modelId) =>
        _db.Cars.AnyAsync(x => x.FkModelId == modelId);

    public Task AddModelAsync(Model model) =>
        _db.Models.AddAsync(model).AsTask();

    public void RemoveModel(Model model) => _db.Models.Remove(model);

    // Vehicle classes
    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id) =>
        _db.VehicleClasses.FirstOrDefaultAsync(x => x.PkVehicleClassId == id);

    public Task<bool> VehicleClassNameExistsAsync(string vehicleClassName, int? excludeVehicleClassId = null) =>
        _db.VehicleClasses.AnyAsync(x =>
            x.PkVehicleClassId != excludeVehicleClassId &&
            x.VehicleClass1.ToUpper() == vehicleClassName.ToUpper());

    public Task<bool> VehicleClassHasCarsAsync(int vehicleClassId) =>
        _db.Cars.AnyAsync(x => x.FkVehicleClassId == vehicleClassId);

    public Task AddVehicleClassAsync(VehicleClass vehicleClass) =>
        _db.VehicleClasses.AddAsync(vehicleClass).AsTask();

    public void RemoveVehicleClass(VehicleClass vehicleClass) => _db.VehicleClasses.Remove(vehicleClass);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
