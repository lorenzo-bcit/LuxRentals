using LuxRentals.Models;
using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Services.Cars;

public class CarInventoryService : ICarInventoryService
{
    private readonly ICarWriteRepository _carWriteRepo;
    private readonly ICarReadRepository _carReadRepo;

    public CarInventoryService(ICarWriteRepository carWriteRepo, ICarReadRepository carReadRepo)
    {
        _carWriteRepo = carWriteRepo;
        _carReadRepo = carReadRepo;
    }

    public async Task<SaveResult> CreateAsync(CarUpsertVm vm)
    {
        var errors = await ValidateAsync(vm);

        if (errors.Count > 0)
            return SaveResult.FailMany(errors);

        var car = new Car();
        vm.ApplyToEntity(car);

        await _carWriteRepo.AddAsync(car);

        return await TrySaveAsync();
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

        return await TrySaveAsync();
    }

    private async Task<SaveResult> TrySaveAsync()
    {
        try
        {
            await _carWriteRepo.SaveChangesAsync();
            return SaveResult.Ok();
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