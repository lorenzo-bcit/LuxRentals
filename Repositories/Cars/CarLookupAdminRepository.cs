using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Cars;

public class CarLookupAdminRepository : ICarLookupAdminRepository
{
    private readonly LuxRentalsDbContext _db;

    public CarLookupAdminRepository(LuxRentalsDbContext db) => _db = db;

    public Task<List<Make>> GetMakesAsync() =>
        _db.Makes
            .AsNoTracking()
            .Include(x => x.Models)
            .OrderBy(x => x.MakeName)
            .ToListAsync();

    public Task<Make?> GetMakeByIdAsync(int id) =>
        _db.Makes.FirstOrDefaultAsync(x => x.PkMakeId == id);

    public Task<bool> MakeNameExistsAsync(string makeName, int? excludeMakeId = null) =>
        _db.Makes.AnyAsync(x =>
            x.PkMakeId != excludeMakeId &&
            x.MakeName.ToUpper() == makeName.ToUpper());

    public Task<bool> MakeHasModelsAsync(int makeId) =>
        _db.Models.AnyAsync(x => x.FkMakeId == makeId);

    public Task AddMakeAsync(Make make) =>
        _db.Makes.AddAsync(make).AsTask();

    public void RemoveMake(Make make) => _db.Makes.Remove(make);

    public Task<List<Model>> GetModelsAsync() =>
        _db.Models
            .AsNoTracking()
            .Include(x => x.FkMake)
            .Include(x => x.Cars)
            .OrderBy(x => x.FkMake.MakeName)
            .ThenBy(x => x.ModelName)
            .ToListAsync();

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

    public Task<List<VehicleClass>> GetVehicleClassesAsync() =>
        _db.VehicleClasses
            .AsNoTracking()
            .Include(x => x.Cars)
            .OrderBy(x => x.VehicleClass1)
            .ToListAsync();

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

    public Task<bool> MakeExistsAsync(int makeId) =>
        _db.Makes.AnyAsync(x => x.PkMakeId == makeId);

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
