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

    public async Task<SaveResult> UpsertAsync(CarUpsertVm vm)
    {
        var errors = new List<(string Field, string Message)>();

        if (await _carReadRepo.VinExistsAsync(vm.VinNumber, excludeCarId: vm.CarId))
            errors.Add((nameof(vm.VinNumber), "VIN already exists."));

        if (await _carReadRepo.PlateExistsAsync(vm.LicencePlate, excludeCarId: vm.CarId))
            errors.Add((nameof(vm.LicencePlate), "Licence plate already exists."));

        if (errors.Count > 0)
            return SaveResult.FailMany(errors);

        if (vm.CarId is null)
        {
            // Create
            var car = new Car();
            vm.ApplyToEntity(car);

            await _carWriteRepo.AddAsync(car);

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

        // Update
        var existing = await _carReadRepo.GetByIdAsync(vm.CarId.Value);
        if (existing is null)
            return SaveResult.Fail("", "Car not found.");

        vm.ApplyToEntity(existing);

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
}