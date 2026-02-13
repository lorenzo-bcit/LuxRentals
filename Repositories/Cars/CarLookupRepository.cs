using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Cars;

public class CarLookupRepository : ICarLookupRepository
{
    private readonly LuxRentalsDbContext _db;
    public CarLookupRepository(LuxRentalsDbContext db) => _db = db;

    public Task<List<FuelType>> GetFuelTypesAsync() =>
        _db.FuelTypes
            .OrderBy(x => x.FuelType1)
            .ToListAsync();

    public Task<List<VehicleClass>> GetVehicleClassesAsync() =>
        _db.VehicleClasses
            .OrderBy(x => x.VehicleClass1)
            .ToListAsync();

    public Task<List<CarStatus>> GetCarStatusesAsync() =>
        _db.CarStatuses
            .OrderBy(x => x.StatusFlag)
            .ToListAsync();

    public Task<List<Make>> GetMakesAsync() =>
        _db.Makes
            .OrderBy(x => x.MakeName)
            .ToListAsync();

    public Task<List<Model>> GetModelsAsync(int? makeId = null)
    {
        var q = _db.Models.AsQueryable();

        if (makeId is not null)
            q = q.Where(m => m.FkMakeId == makeId);

        return q.OrderBy(m => m.ModelName).ToListAsync();
    }
}