using LuxRentals.Models;
using LuxRentals.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data.Seeding;

public static class CarSeeder
{
    public static async Task EnsureCoreCarLookupsSeededAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LuxRentalsDbContext>();

        await EnsureCarStatusesAsync(db);
        await EnsureFuelTypesAsync(db);
    }

    public static async Task EnsureDemoCarCatalogSeededAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(CarSeeder));

        var db = services.GetRequiredService<LuxRentalsDbContext>();
        var demoCars = GetDemoCars();

        var carStatusIds = await EnsureCarStatusesAsync(db);
        var fuelTypeIds = await EnsureFuelTypesAsync(db);
        var vehicleClassIds = await EnsureVehicleClassesAsync(db, demoCars);
        var modelIds = await EnsureMakesAndModelsAsync(db, demoCars);

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
        logger.LogInformation("Inserted {Count} demo fleet cars for browse testing.", inserted);
    }

    private static async Task<Dictionary<string, int>> EnsureCarStatusesAsync(LuxRentalsDbContext db)
    {
        var required = new[]
        {
            CarStatusNames.AVAILABLE,
            CarStatusNames.BOOKING_HOLD,
            CarStatusNames.MAINTENANCE,
            CarStatusNames.OUT_OF_SERVICE
        };

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

    private static async Task<Dictionary<string, int>> EnsureVehicleClassesAsync(
        LuxRentalsDbContext db,
        IReadOnlyCollection<DemoCarSeed> demoCars)
    {
        var required = demoCars
            .Select(x => x.VehicleClass)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

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

    private static async Task<Dictionary<string, int>> EnsureMakesAndModelsAsync(
        LuxRentalsDbContext db,
        IReadOnlyCollection<DemoCarSeed> demoCars)
    {
        var requiredModels = demoCars
            .Select(x => (x.Make, x.Model))
            .Distinct()
            .OrderBy(x => x.Make)
            .ThenBy(x => x.Model)
            .ToArray();

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
        new("SUV", "BMW", "X7", "Gasoline", CarStatusNames.AVAILABLE, 1, 2023, "Carbon Black", "1HGBH41JXMN300001", "LUX-3001", 7, 5, 289.90m, "/images/demo-cars/bmw-x7-2023.jpg"),
        new("SUV", "Bentley", "Bentayga", "Gasoline", CarStatusNames.AVAILABLE, 1, 2016, "Copper Bronze", "1HGBH41JXMN300002", "LUX-3002", 5, 4, 449.90m, "/images/demo-cars/bentley-bentayga-2016.jpg"),
        new("Coupe", "Jaguar", "F-TYPE", "Gasoline", CarStatusNames.AVAILABLE, 1, 2021, "Carpathian Grey", "1HGBH41JXMN300003", "LUX-3003", 2, 1, 279.90m, "/images/demo-cars/jaguar-f-type-2021.jpg"),
        new("SUV", "Tesla", "Model Y", "Electric", CarStatusNames.AVAILABLE, 1, 2025, "Quicksilver", "1HGBH41JXMN300004", "LUX-3004", 5, 4, 199.90m, "/images/demo-cars/tesla-model-y-2025.jpg"),
        new("Coupe", "Lotus", "Elise", "Gasoline", CarStatusNames.AVAILABLE, 0, 2011, "Arctic White", "1HGBH41JXMN300005", "LUX-3005", 2, 1, 239.90m, "/images/demo-cars/lotus-elise-2011.jpg"),
        new("SUV", "Porsche", "Macan S", "Gasoline", CarStatusNames.AVAILABLE, 1, 2022, "Volcano Grey", "1HGBH41JXMN300006", "LUX-3006", 5, 4, 259.90m, "/images/demo-cars/porsche-macan-s-2022.jpg"),
        new("Coupe", "Ford", "GT", "Gasoline", CarStatusNames.AVAILABLE, 1, 2017, "Competition Yellow", "1HGBH41JXMN300007", "LUX-3007", 2, 1, 699.90m, "/images/demo-cars/ford-gt-2017.jpg"),
        new("Coupe", "Audi", "R8 Coupe", "Gasoline", CarStatusNames.AVAILABLE, 1, 2019, "Suzuka Grey", "1HGBH41JXMN300008", "LUX-3008", 2, 1, 449.90m, "/images/demo-cars/audi-r8-coupe-2019.jpg"),
        new("Coupe", "BMW", "M2", "Gasoline", CarStatusNames.AVAILABLE, 0, 2025, "Skyscraper Grey", "1HGBH41JXMN300009", "LUX-3009", 4, 2, 239.90m, "/images/demo-cars/bmw-m2-2025.jpg"),
        new("Coupe", "Ford", "Mustang GT Fastback", "Gasoline", CarStatusNames.AVAILABLE, 0, 2018, "Race Red", "1HGBH41JXMN300010", "LUX-3010", 4, 2, 229.90m, "/images/demo-cars/ford-mustang-gt-2018.jpg"),
        new("Convertible", "Rolls-Royce", "Dawn", "Gasoline", CarStatusNames.AVAILABLE, 1, 2017, "Burgundy Red", "1HGBH41JXMN300011", "LUX-3011", 4, 2, 469.90m, "/images/demo-cars/rolls-royce-dawn-2017.jpg"),
        new("SUV", "Lincoln", "Aviator", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Silver Radiance", "1HGBH41JXMN300012", "LUX-3012", 6, 4, 239.90m, "/images/demo-cars/lincoln-aviator-2025.jpg"),
        new("Sedan", "Porsche", "Taycan", "Electric", CarStatusNames.AVAILABLE, 1, 2025, "Frozen Blue", "1HGBH41JXMN300013", "LUX-3013", 4, 2, 329.90m, "/images/demo-cars/porsche-taycan-2025.jpg"),
        new("Coupe", "Ferrari", "Amalfi", "Gasoline", CarStatusNames.AVAILABLE, 1, 2026, "Teal Green", "1HGBH41JXMN300014", "LUX-3014", 2, 1, 549.90m, "/images/demo-cars/ferrari-amalfi-2026.jpg"),
        new("Sedan", "Rolls-Royce", "Ghost Series II", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Peacock Blue", "1HGBH41JXMN300015", "LUX-3015", 5, 3, 489.90m, "/images/demo-cars/rolls-royce-ghost-series-ii-2025.jpg"),
        new("Sedan", "Bentley", "Flying Spur", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Ice Silver", "1HGBH41JXMN300016", "LUX-3016", 5, 3, 399.90m, "/images/demo-cars/bentley-flying-spur-2025.jpg"),
        new("Sedan", "Mercedes-Benz", "AMG E 53", "Hybrid", CarStatusNames.AVAILABLE, 1, 2025, "Polar White", "1HGBH41JXMN300026", "LUX-3026", 5, 3, 289.90m, "/images/demo-cars/mercedes-benz-e53-amg-hybrid-2025.jpg"),
        new("Sedan", "Porsche", "Panamera", "Diesel", CarStatusNames.AVAILABLE, 1, 2014, "Sapphire Blue", "1HGBH41JXMN300027", "LUX-3027", 4, 3, 279.90m, "/images/demo-cars/porsche-panamera-diesel-2014.jpg"),
        new("Coupe", "Porsche", "911 Carrera", "Gasoline", CarStatusNames.AVAILABLE, 1, 2016, "GT Silver", "1HGBH41JXMN300017", "LUX-3017", 4, 2, 319.90m, "/images/demo-cars/porsche-911-carrera-2016.jpg"),
        new("SUV", "Lincoln", "Navigator", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Pristine White", "1HGBH41JXMN300018", "LUX-3018", 7, 6, 299.90m, "/images/demo-cars/lincoln-navigator-2025.jpg"),
        new("Sedan", "Tesla", "Model S", "Electric", CarStatusNames.AVAILABLE, 1, 2017, "Pearl White", "1HGBH41JXMN300019", "LUX-3019", 5, 3, 279.90m, "/images/demo-cars/tesla-model-s-2017.jpg"),
        new("SUV", "Audi", "Q8 e-tron", "Electric", CarStatusNames.AVAILABLE, 1, 2024, "Florett Silver", "1HGBH41JXMN300020", "LUX-3020", 5, 4, 269.90m, "/images/demo-cars/audi-q8-e-tron-quattro-2024.jpg"),
        new("Sedan", "Tesla", "Model 3", "Electric", CarStatusNames.MAINTENANCE, 1, 2024, "Ultra Red", "1HGBH41JXMN300021", "LUX-3021", 5, 3, 179.90m, "/images/demo-cars/tesla-model-3-2024.jpg"),
        new("Coupe", "Lotus", "Emira", "Gasoline", CarStatusNames.AVAILABLE, 1, 2023, "Hethel Black", "1HGBH41JXMN300022", "LUX-3022", 2, 1, 289.90m, "/images/demo-cars/lotus-emira-2023.jpg"),
        new("SUV", "Jaguar", "F-PACE", "Gasoline", CarStatusNames.AVAILABLE, 1, 2021, "Eiger Grey", "1HGBH41JXMN300023", "LUX-3023", 5, 4, 249.90m, "/images/demo-cars/jaguar-f-pace-2021.jpg"),
        new("SUV", "Porsche", "Cayenne Turbo E-Hybrid", "Hybrid", CarStatusNames.AVAILABLE, 1, 2024, "Carrara White", "1HGBH41JXMN300028", "LUX-3028", 5, 4, 339.90m, "/images/demo-cars/porsche-cayenne-turbo-e-hybrid-2024.jpg"),
        new("Coupe", "Jaguar", "F-TYPE SVR Coupe", "Gasoline", CarStatusNames.AVAILABLE, 1, 2017, "Firesand Orange", "1HGBH41JXMN300024", "LUX-3024", 2, 1, 329.90m, "/images/demo-cars/jaguar-f-type-svr-coupe-2017.jpg"),
        new("Coupe", "Ferrari", "458 Italia", "Gasoline", CarStatusNames.AVAILABLE, 1, 2011, "Rosso Corsa", "1HGBH41JXMN300025", "LUX-3025", 2, 1, 589.90m, "/images/demo-cars/ferrari-458-italia-2011.jpg")
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