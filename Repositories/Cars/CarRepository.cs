using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Cars;

public class CarRepository : ICarReadRepository, ICarWriteRepository
{
    private readonly LuxRentalsDbContext _db;
    public CarRepository(LuxRentalsDbContext db) => _db = db;

    // READ
    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria)
    {
        var cars = BuildBaseQuery();
        cars = ApplyAttributeFilters(cars, criteria);
        cars = ApplyAvailabilityFilter(cars, criteria);
        cars = cars.OrderBy(c => c.PkCarId);

        return PagedList<Car>.CreateAsync(cars, criteria.Page, criteria.PageSize);
    }

    private IQueryable<Car> BuildBaseQuery() =>
        _db.Cars
            .AsNoTracking()
            .Include(c => c.FkModel).ThenInclude(m => m.FkMake)
            .Include(c => c.FkFuelType)
            .Include(c => c.FkVehicleClass)
            .Include(c => c.FkCarStatus);

    private static IQueryable<Car> ApplyAttributeFilters(
        IQueryable<Car> cars,
        CarSearchCriteria criteria)
    {
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

        return cars;
    }

    private IQueryable<Car> ApplyAvailabilityFilter(
        IQueryable<Car> cars,
        CarSearchCriteria criteria)
    {
        if (criteria.StartDate is null || criteria.EndDate is null)
            return cars;

        var start = criteria.StartDate.Value;
        var end = criteria.EndDate.Value;

        // Only operational cars when searching by dates
        cars = cars.Where(c => c.FkCarStatus.StatusFlag == "Available");

        return cars.Where(c =>
            !_db.Bookings.Any(b =>
                b.FkCarId == c.PkCarId &&
                b.CancelledAt == null &&
                b.StartDateTime < end &&
                b.EndDateTime > start));
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

    // WRITE
    public Task AddAsync(Car car) => _db.Cars.AddAsync(car).AsTask();
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}