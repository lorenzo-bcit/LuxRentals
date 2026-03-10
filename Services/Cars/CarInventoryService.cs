using LuxRentals.Models;
using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Services.Cars;

public class CarInventoryService : ICarInventoryService
{
    private const string OUT_OF_SERVICE_STATUS = "Out of Service";

    private readonly ICarWriteRepository _carWriteRepo;
    private readonly ICarReadRepository _carReadRepo;
    private readonly ICarLookupRepository _carLookupRepo;

    public CarInventoryService(
        ICarWriteRepository carWriteRepo,
        ICarReadRepository carReadRepo,
        ICarLookupRepository carLookupRepo)
    {
        _carWriteRepo = carWriteRepo;
        _carReadRepo = carReadRepo;
        _carLookupRepo = carLookupRepo;
    }

    public async Task<SaveResult> CreateAsync(CarUpsertVm vm)
    {
        var errors = await ValidateAsync(vm);

        if (errors.Count > 0)
            return SaveResult.FailMany(errors);

        var car = new Car();
        vm.ApplyToEntity(car);

        await _carWriteRepo.AddAsync(car);

        return await TrySaveAsync("Car created.");
    }

    public async Task<SaveResult> UpdateAsync(int id, CarUpsertVm vm)
    {
        var existing = await _carReadRepo.GetByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail("", "Car not found.");

        var errors = await ValidateAsync(vm, id);

        if (errors.Count > 0)
            return SaveResult.FailMany(errors);

        vm.ApplyToEntity(existing);

        return await TrySaveAsync("Car updated.");
    }

    public async Task<SaveResult> DeleteAsync(int id)
    {
        var existing = await _carReadRepo.GetByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail("", "Car not found.");

        if (await _carReadRepo.HasBookingsAsync(id))
        {
            var outOfServiceStatusId = await _carLookupRepo.GetCarStatusIdByNameAsync(OUT_OF_SERVICE_STATUS);
            if (outOfServiceStatusId is null)
                return SaveResult.Fail("", "Out of Service status is missing. Seed car statuses and try again.");

            existing.FkCarStatusId = outOfServiceStatusId.Value;

            return await TrySaveAsync("Car has bookings, so it was marked Out of Service instead of being deleted.");
        }

        _carWriteRepo.Remove(existing);

        return await TrySaveAsync("Car deleted.");
    }

    private async Task<SaveResult> TrySaveAsync(string successMessage)
    {
        try
        {
            await _carWriteRepo.SaveChangesAsync();
            return SaveResult.Ok(successMessage);
        }
        catch (DbUpdateException)
        {
            return SaveResult.Fail("", "Save failed due to a database constraint. Please refresh and try again.");
        }
    }

    private async Task<List<(string Field, string Message)>> ValidateAsync(CarUpsertVm vm, int? excludeCarId = null)
    {
        var errors = new List<(string Field, string Message)>();

        if (await _carReadRepo.VinExistsAsync(vm.VinNumber, excludeCarId))
            errors.Add((nameof(vm.VinNumber), "VIN already exists."));

        if (await _carReadRepo.PlateExistsAsync(vm.LicencePlate, excludeCarId))
            errors.Add((nameof(vm.LicencePlate), "Licence plate already exists."));

        return errors;
    }
}