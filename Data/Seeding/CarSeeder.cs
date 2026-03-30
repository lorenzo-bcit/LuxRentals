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
        logger.LogInformation("Inserted {Count} luxury demo cars for browse testing.", inserted);
    }

    private static async Task<Dictionary<string, int>> EnsureCarStatusesAsync(LuxRentalsDbContext db)
    {
        var required = new[]
        {
            CarStatusNames.AVAILABLE,
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

    private static async Task<Dictionary<string, int>> EnsureVehicleClassesAsync(LuxRentalsDbContext db)
    {
        var required = new[] { "Luxury Sedan", "Luxury SUV", "Premium EV", "Executive SUV", "Grand Touring Coupe" };

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
            (Make: "BMW", Model: "5 Series"),
            (Make: "BMW", Model: "X5"),
            (Make: "BMW", Model: "8 Series"),
            (Make: "Mercedes-Benz", Model: "E-Class"),
            (Make: "Mercedes-Benz", Model: "GLE 450"),
            (Make: "Mercedes-Benz", Model: "EQS"),
            (Make: "Audi", Model: "A6"),
            (Make: "Audi", Model: "Q7"),
            (Make: "Audi", Model: "e-tron GT"),
            (Make: "Lexus", Model: "ES 350"),
            (Make: "Lexus", Model: "RX 500h"),
            (Make: "Genesis", Model: "G80"),
            (Make: "Porsche", Model: "Panamera"),
            (Make: "Porsche", Model: "Cayenne"),
            (Make: "Tesla", Model: "Model S"),
            (Make: "Tesla", Model: "Model X"),
            (Make: "Jaguar", Model: "F-PACE"),
            (Make: "Alfa Romeo", Model: "Stelvio"),
            (Make: "Volvo", Model: "XC90"),
            (Make: "Land Rover", Model: "Range Rover Sport"),
            (Make: "Cadillac", Model: "Escalade"),
            (Make: "Bentley", Model: "Bentayga")
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
        new("Luxury Sedan", "BMW", "5 Series", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Obsidian Black", "1HGBH41JXMN200001", "LUX-2001", 5, 3, 149.90m, null),
        new("Luxury Sedan", "Mercedes-Benz", "E-Class", "Hybrid", CarStatusNames.AVAILABLE, 1, 2025, "Selenite Grey", "1HGBH41JXMN200002", "LUX-2002", 5, 3, 169.90m, null),
        new("Luxury Sedan", "Audi", "A6", "Gasoline", CarStatusNames.AVAILABLE, 1, 2024, "Glacier White", "1HGBH41JXMN200003", "LUX-2003", 5, 3, 159.90m, null),
        new("Luxury Sedan", "Lexus", "ES 350", "Hybrid", CarStatusNames.AVAILABLE, 1, 2024, "Deep Blue", "1HGBH41JXMN200004", "LUX-2004", 5, 3, 139.90m, null),
        new("Luxury Sedan", "Genesis", "G80", "Gasoline", CarStatusNames.AVAILABLE, 1, 2024, "Matte Grey", "1HGBH41JXMN200005", "LUX-2005", 5, 3, 154.90m, null),
        new("Luxury Sedan", "Porsche", "Panamera", "Gasoline", CarStatusNames.AVAILABLE, 1, 2024, "Jet Black", "1HGBH41JXMN200006", "LUX-2006", 4, 3, 249.90m, null),
        new("Premium EV", "Audi", "e-tron GT", "Electric", CarStatusNames.AVAILABLE, 1, 2025, "Daytona Grey", "1HGBH41JXMN200007", "LUX-2007", 4, 2, 279.90m, null),
        new("Premium EV", "Mercedes-Benz", "EQS", "Electric", CarStatusNames.AVAILABLE, 1, 2025, "Polar White", "1HGBH41JXMN200008", "LUX-2008", 5, 3, 299.90m, null),
        new("Premium EV", "Tesla", "Model S", "Electric", CarStatusNames.AVAILABLE, 1, 2025, "Pearl White", "1HGBH41JXMN200009", "LUX-2009", 5, 3, 269.90m, null),
        new("Grand Touring Coupe", "BMW", "8 Series", "Gasoline", CarStatusNames.AVAILABLE, 1, 2024, "Carbon Black", "1HGBH41JXMN200010", "LUX-2010", 4, 2, 259.90m, null),

        new("Luxury SUV", "BMW", "X5", "Hybrid", CarStatusNames.AVAILABLE, 1, 2025, "Mineral White", "1HGBH41JXMN200011", "LUX-2011", 5, 4, 199.90m, null),
        new("Luxury SUV", "Mercedes-Benz", "GLE 450", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Graphite", "1HGBH41JXMN200012", "LUX-2012", 5, 4, 214.90m, null),
        new("Luxury SUV", "Audi", "Q7", "Diesel", CarStatusNames.AVAILABLE, 1, 2024, "Navarra Blue", "1HGBH41JXMN200013", "LUX-2013", 7, 5, 209.90m, null),
        new("Luxury SUV", "Lexus", "RX 500h", "Hybrid", CarStatusNames.AVAILABLE, 1, 2025, "Caviar", "1HGBH41JXMN200014", "LUX-2014", 5, 4, 189.90m, null),
        new("Luxury SUV", "Jaguar", "F-PACE", "Gasoline", CarStatusNames.AVAILABLE, 1, 2024, "British Racing Green", "1HGBH41JXMN200015", "LUX-2015", 5, 4, 219.90m, null),
        new("Luxury SUV", "Alfa Romeo", "Stelvio", "Gasoline", CarStatusNames.MAINTENANCE, 1, 2023, "Rosso Red", "1HGBH41JXMN200016", "LUX-2016", 5, 4, 179.90m, null),
        new("Luxury SUV", "Volvo", "XC90", "Hybrid", CarStatusNames.AVAILABLE, 1, 2024, "Onyx Black", "1HGBH41JXMN200017", "LUX-2017", 7, 5, 194.90m, null),
        new("Luxury SUV", "Porsche", "Cayenne", "Gasoline", CarStatusNames.AVAILABLE, 1, 2025, "Carrara White", "1HGBH41JXMN200018", "LUX-2018", 5, 4, 259.90m, null),
        new("Executive SUV", "Land Rover", "Range Rover Sport", "Diesel", CarStatusNames.AVAILABLE, 1, 2024, "Santorini Black", "1HGBH41JXMN200019", "LUX-2019", 5, 5, 289.90m, null),
        new("Executive SUV", "Cadillac", "Escalade", "Gasoline", CarStatusNames.AVAILABLE, 1, 2024, "Crystal White", "1HGBH41JXMN200020", "LUX-2020", 7, 6, 309.90m, null),

        new("Premium EV", "Tesla", "Model X", "Electric", CarStatusNames.AVAILABLE, 1, 2025, "Solid Black", "1HGBH41JXMN200021", "LUX-2021", 6, 5, 319.90m, null),
        new("Executive SUV", "Bentley", "Bentayga", "Gasoline", CarStatusNames.OUT_OF_SERVICE, 1, 2023, "Moonbeam Silver", "1HGBH41JXMN200022", "LUX-2022", 5, 4, 449.90m, null),
        new("Luxury SUV", "BMW", "X5", "Gasoline", CarStatusNames.AVAILABLE, 1, 2023, "Dark Graphite", "1HGBH41JXMN200023", "LUX-2023", 5, 4, 189.90m, null),
        new("Luxury SUV", "Mercedes-Benz", "GLE 450", "Hybrid", CarStatusNames.AVAILABLE, 1, 2024, "Diamond White", "1HGBH41JXMN200024", "LUX-2024", 5, 4, 224.90m, null),
        new("Luxury SUV", "Audi", "Q7", "Gasoline", CarStatusNames.AVAILABLE, 1, 2023, "Mythos Black", "1HGBH41JXMN200025", "LUX-2025", 7, 5, 199.90m, null),
        new("Luxury SUV", "Porsche", "Cayenne", "Hybrid", CarStatusNames.AVAILABLE, 1, 2024, "Arctic Grey", "1HGBH41JXMN200026", "LUX-2026", 5, 4, 269.90m, null),
        new("Premium EV", "Tesla", "Model S", "Electric", CarStatusNames.MAINTENANCE, 1, 2024, "Midnight Silver", "1HGBH41JXMN200027", "LUX-2027", 5, 3, 259.90m, null),
        new("Premium EV", "Mercedes-Benz", "EQS", "Electric", CarStatusNames.AVAILABLE, 1, 2024, "Nautical Blue", "1HGBH41JXMN200028", "LUX-2028", 5, 3, 289.90m, null),
        new("Luxury Sedan", "BMW", "5 Series", "Diesel", CarStatusNames.AVAILABLE, 1, 2023, "Frozen Grey", "1HGBH41JXMN200029", "LUX-2029", 5, 3, 144.90m, null),
        new("Luxury Sedan", "Lexus", "ES 350", "Gasoline", CarStatusNames.AVAILABLE, 1, 2023, "Atomic Silver", "1HGBH41JXMN200030", "LUX-2030", 5, 3, 134.90m, null),
        new("Luxury Sedan", "Genesis", "G80", "Hybrid", CarStatusNames.AVAILABLE, 1, 2025, "Makalu Grey", "1HGBH41JXMN200031", "LUX-2031", 5, 3, 164.90m, null),
        new("Executive SUV", "Cadillac", "Escalade", "Diesel", CarStatusNames.AVAILABLE, 1, 2025, "Black Raven", "1HGBH41JXMN200032", "LUX-2032", 7, 6, 324.90m, null)
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