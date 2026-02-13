using LuxRentals.Data;
using LuxRentals.Models;
using LuxRentals.ViewModels.Cars;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Cars;

public class CarRepository : ICarReadRepository, ICarWriteRepository
{
    private readonly LuxRentalsDbContext _db;
    public CarRepository(LuxRentalsDbContext db) => _db = db;

    // READ
    public Task<PagedList<Car>> SearchAsync(CarSearchVm vm)
    {
        var q = _db.Cars
            .AsNoTracking()
            .Include(c => c.FkModel).ThenInclude(m => m.FkMake)
            .Include(c => c.FkFuelType)
            .Include(c => c.FkVehicleClass)
            .Include(c => c.FkCarStatus)
            .AsQueryable();

        if (vm.AvailableOnly) q = q.Where(c => c.FkCarStatus.StatusFlag == "Available");
        if (vm.FuelTypeId != null) q = q.Where(c => c.FkFuelTypeId == vm.FuelTypeId);
        if (vm.VehicleClassId != null) q = q.Where(c => c.FkVehicleClassId == vm.VehicleClassId);
        if (vm.TransmissionType != null) q = q.Where(c => c.TransmissionType == vm.TransmissionType);
        if (vm.MinSeats != null) q = q.Where(c => c.PersonCap >= vm.MinSeats);
        if (vm.MinLuggage != null) q = q.Where(c => c.LuggageCap >= vm.MinLuggage);

        q = q.OrderBy(c => c.PkCarId);

        return PagedList<Car>.CreateAsync(q, vm.Page, vm.PageSize);
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