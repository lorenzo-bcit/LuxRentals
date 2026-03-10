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

    public Task<List<Make>> GetMakesAsync() =>
        _db.Makes
            .AsNoTracking()
            .OrderBy(x => x.MakeName)
            .ToListAsync();

    public Task<List<Model>> GetModelsAsync(int? makeId = null)
    {
        var q = _db.Models
            .AsNoTracking()
            .Include(m => m.FkMake)
            .AsQueryable();

        if (makeId is not null)
            q = q.Where(m => m.FkMakeId == makeId);

        return q.OrderBy(m => m.ModelName).ToListAsync();
    }

    public async Task<int?> GetCarStatusIdByNameAsync(string statusName)
    {
        return await _db.CarStatuses
            .AsNoTracking()
            .Where(x => x.StatusFlag == statusName)
            .Select(x => (int?)x.PkCarStatusId)
            .FirstOrDefaultAsync();
    }
}