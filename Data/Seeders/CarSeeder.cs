using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data.Seeders;

public static class CarSeeder
{
    public static async Task EnsureCarCatalogSeededAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(CarSeeder));

        var db = services.GetRequiredService<LuxRentalsDbContext>();

        var carStatusIds = await EnsureCarStatusesAsync(db);
        var fuelTypeIds = await EnsureFuelTypesAsync(db);
        var vehicleClassIds = await EnsureVehicleClassesAsync(db);
        var modelIds = await EnsureMakesAndModelsAsync(db);

        var demoCars = GetDemoCars();

        var candidateVins = demoCars.Select(x => x.VinNumber).ToList();
        var candidatePlates = demoCars.Select(x => x.LicencePlate).ToList();

        var existingVins = (await db.Cars
                .Where(c => candidateVins.Contains(c.VinNumber))
                .Select(c => c.VinNumber)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingPlates = (await db.Cars
                .Where(c => candidatePlates.Contains(c.LicencePlate))
                .Select(c => c.LicencePlate)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        foreach (var seed in demoCars)
        {
            if (existingVins.Contains(seed.VinNumber) || existingPlates.Contains(seed.LicencePlate))
                continue;

            var modelKey = BuildModelKey(seed.Make, seed.Model);

            if (!vehicleClassIds.TryGetValue(seed.VehicleClass, out var vehicleClassId) ||
                !fuelTypeIds.TryGetValue(seed.FuelType, out var fuelTypeId) ||
                !carStatusIds.TryGetValue(seed.CarStatus, out var carStatusId) ||
                !modelIds.TryGetValue(modelKey, out var modelId))
            {
                continue;
            }

            db.Cars.Add(new Car
            {
                Colour = seed.Colour,
                TransmissionType = seed.TransmissionType,
                Year = seed.Year,
                CarThumbnail = seed.CarThumbnail,
                VinNumber = seed.VinNumber,
                LicencePlate = seed.LicencePlate,
                PersonCap = seed.PersonCap,
                LuggageCap = seed.LuggageCap,
                DailyRate = seed.DailyRate,
                FkVehicleClassId = vehicleClassId,
                FkCarStatusId = carStatusId,
                FkModelId = modelId,
                FkFuelTypeId = fuelTypeId
            });

            inserted++;
        }

        if (inserted == 0)
            return;

        await db.SaveChangesAsync();
        logger.LogInformation("Inserted {Count} demo cars for browse testing.", inserted);
    }

    private static async Task<Dictionary<string, int>> EnsureCarStatusesAsync(LuxRentalsDbContext db)
    {
        var required = new[] { "Available", "Maintenance", "Out of Service" };

        var existing = await db.CarStatuses.ToListAsync();
        var existingNames = existing
            .Select(x => x.StatusFlag)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in required)
        {
            if (!existingNames.Contains(name))
                db.CarStatuses.Add(new CarStatus { StatusFlag = name });
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        var all = await db.CarStatuses.AsNoTracking().ToListAsync();
        return all.ToDictionary(x => x.StatusFlag, x => x.PkCarStatusId, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, int>> EnsureFuelTypesAsync(LuxRentalsDbContext db)
    {
        var required = new[] { "Gasoline", "Electric", "Hybrid", "Diesel" };

        var existing = await db.FuelTypes.ToListAsync();
        var existingNames = existing
            .Select(x => x.FuelType1)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in required)
        {
            if (!existingNames.Contains(name))
                db.FuelTypes.Add(new FuelType { FuelType1 = name });
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        var all = await db.FuelTypes.AsNoTracking().ToListAsync();
        return all.ToDictionary(x => x.FuelType1, x => x.PkFuelTypeId, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, int>> EnsureVehicleClassesAsync(LuxRentalsDbContext db)
    {
        var required = new[] { "Economy", "Compact SUV", "Midsize SUV AWD", "Full Size" };

        var existing = await db.VehicleClasses.ToListAsync();
        var existingNames = existing
            .Select(x => x.VehicleClass1)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var name in required)
        {
            if (!existingNames.Contains(name))
                db.VehicleClasses.Add(new VehicleClass { VehicleClass1 = name });
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        var all = await db.VehicleClasses.AsNoTracking().ToListAsync();
        return all.ToDictionary(x => x.VehicleClass1, x => x.PkVehicleClassId, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<Dictionary<string, int>> EnsureMakesAndModelsAsync(LuxRentalsDbContext db)
    {
        var requiredModels = new[]
        {
            (Make: "Ford", Model: "Escape"),
            (Make: "Kia", Model: "Seltos"),
            (Make: "Hyundai", Model: "Elantra"),
            (Make: "Chevrolet", Model: "Malibu"),
            (Make: "Toyota", Model: "Corolla"),
            (Make: "Nissan", Model: "Rogue")
        };

        var requiredMakes = requiredModels
            .Select(x => x.Make)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingMakes = await db.Makes.ToListAsync();
        var existingMakeNames = existingMakes
            .Select(x => x.MakeName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var makeName in requiredMakes)
        {
            if (!existingMakeNames.Contains(makeName))
                db.Makes.Add(new Make { MakeName = makeName });
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        var allMakes = await db.Makes.AsNoTracking().ToListAsync();
        var makeIdByName = allMakes.ToDictionary(x => x.MakeName, x => x.PkMakeId, StringComparer.OrdinalIgnoreCase);

        var existingModels = await db.Models.AsNoTracking().ToListAsync();
        foreach (var (make, model) in requiredModels)
        {
            var makeId = makeIdByName[make];

            var modelExists = existingModels.Any(x =>
                x.FkMakeId == makeId &&
                string.Equals(x.ModelName, model, StringComparison.OrdinalIgnoreCase));

            if (modelExists)
                continue;

            db.Models.Add(new Model
            {
                FkMakeId = makeId,
                ModelName = model
            });
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync();

        var allModels = await db.Models
            .AsNoTracking()
            .Include(x => x.FkMake)
            .ToListAsync();

        return allModels.ToDictionary(
            x => BuildModelKey(x.FkMake.MakeName, x.ModelName),
            x => x.PkModelId,
            StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildModelKey(string make, string model) => $"{make}|{model}";

    private static List<DemoCarSeed> GetDemoCars() =>
    [
        new("Midsize SUV AWD", "Ford", "Escape", "Gasoline", "Available", 1, 2024, "Grey", "1HGBH41JXMN100001", "LUX-1001", 5, 4, 69.90m, null),
        new("Compact SUV", "Kia", "Seltos", "Gasoline", "Available", 1, 2023, "Silver", "1HGBH41JXMN100002", "LUX-1002", 5, 3, 59.90m, null),
        new("Economy", "Hyundai", "Elantra", "Hybrid", "Available", 1, 2024, "White", "1HGBH41JXMN100003", "LUX-1003", 5, 2, 49.90m, null),
        new("Full Size", "Chevrolet", "Malibu", "Gasoline", "Available", 1, 2022, "Black", "1HGBH41JXMN100004", "LUX-1004", 5, 4, 89.90m, null),
        new("Economy", "Toyota", "Corolla", "Gasoline", "Available", 0, 2021, "Blue", "1HGBH41JXMN100005", "LUX-1005", 5, 2, 44.90m, null),
        new("Compact SUV", "Nissan", "Rogue", "Gasoline", "Available", 1, 2022, "Red", "1HGBH41JXMN100006", "LUX-1006", 5, 3, 64.90m, null),
        new("Midsize SUV AWD", "Ford", "Escape", "Hybrid", "Maintenance", 1, 2023, "White", "1HGBH41JXMN100007", "LUX-1007", 5, 4, 74.90m, null),
        new("Full Size", "Chevrolet", "Malibu", "Gasoline", "Available", 1, 2020, "Graphite", "1HGBH41JXMN100008", "LUX-1008", 5, 4, 79.90m, null)
    ];

    private sealed record DemoCarSeed(
        string VehicleClass,
        string Make,
        string Model,
        string FuelType,
        string CarStatus,
        byte TransmissionType,
        int Year,
        string Colour,
        string VinNumber,
        string LicencePlate,
        int PersonCap,
        int LuggageCap,
        decimal DailyRate,
        string? CarThumbnail);
}