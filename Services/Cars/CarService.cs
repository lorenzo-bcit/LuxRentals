using LuxRentals.Models;
using LuxRentals.Repositories;
using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Services.Cars;

public class CarService : ICarService
{
    private const string IMAGE_UPLOAD_FAILED_MESSAGE = "Image upload failed. Please try again.";

    private readonly ICarRepository _repo;
    private readonly ICarImageStorage _carImageStorage;
    private readonly ILogger<CarService> _logger;

    public CarService(ICarRepository repo, ICarImageStorage carImageStorage, ILogger<CarService> logger)
    {
        _repo = repo;
        _carImageStorage = carImageStorage;
        _logger = logger;
    }

    public Task<PagedList<Car>> SearchAsync(CarSearchCriteria criteria) => _repo.SearchAsync(criteria);

    public Task<Car?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<IReadOnlyList<Booking>> GetActiveOrUpcomingBookingsAsync(int carId) =>
        await _repo.GetActiveOrUpcomingBookingsAsync(carId);

    public async Task<SaveResult> CreateAsync(CarEditVm vm)
    {
        var errors = await ValidateAsync(vm);

        if (errors.Count > 0)
            return SaveResult.FailMany(errors);

        var car = new Car();
        vm.ApplyToEntity(car);

        string? uploadedThumbnailPath = null;

        if (vm.ImageFile is not null)
        {
            uploadedThumbnailPath = await _carImageStorage.SaveNewAsync(vm.ImageFile);
            if (uploadedThumbnailPath is null)
            {
                _logger.LogWarning(
                    "Car creation aborted because image upload failed for VIN {Vin} and plate {Plate}.",
                    vm.VinNumber,
                    vm.LicencePlate);
                return SaveResult.Fail(nameof(CarEditVm.ImageFile), IMAGE_UPLOAD_FAILED_MESSAGE);
            }

            car.CarThumbnail = uploadedThumbnailPath;
        }

        await _repo.AddAsync(car);

        var result = await TrySaveAsync($"Car {car.VinNumber} created.");
        if (!result.IsSuccess && uploadedThumbnailPath is not null)
            await _carImageStorage.DeleteAsync(uploadedThumbnailPath);

        return result;
    }

    // Once a car has active or upcoming bookings, edits must not change the terms or identity of
    // those reservations. Only operational status is allowed to toggle between Available and Booking Hold.
    public async Task<SaveResult> UpdateAsync(int id, CarEditVm vm)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Attempted to update car {CarId}, but it was not found.", id);
            return SaveResult.Fail("", $"Car {id} was not found.");
        }

        var errors = await ValidateAsync(vm, id);

        if (errors.Count > 0)
            return SaveResult.FailMany(errors);

        var hasActiveOrUpcomingBookings = await _repo.CountActiveOrUpcomingBookingsAsync(id) > 0;
        if (hasActiveOrUpcomingBookings)
        {
            if (HasLockedFieldChanges(vm, existing))
            {
                _logger.LogWarning(
                    "Blocked update for car {CarId} ({Vin}) because locked fields changed while bookings are active or upcoming.",
                    existing.PkCarId,
                    existing.VinNumber);
                return SaveResult.Fail(
                    string.Empty,
                    $"Car {existing.VinNumber} has active or upcoming bookings. Resolve those bookings before changing car details.");
            }

            var bookedCarStatusChange = await ValidateBookedCarStatusChangeAsync(vm.FkCarStatusId, existing);
            if (!bookedCarStatusChange.IsSuccess)
                return bookedCarStatusChange;
        }

        var previousThumbnailPath = existing.CarThumbnail;
        string? uploadedThumbnailPath = null;

        vm.ApplyToEntity(existing);

        if (vm.ImageFile is not null)
        {
            uploadedThumbnailPath = await _carImageStorage.SaveNewAsync(vm.ImageFile);
            if (uploadedThumbnailPath is null)
            {
                _logger.LogWarning(
                    "Car update aborted because image upload failed for car {CarId} ({Vin}).",
                    existing.PkCarId,
                    existing.VinNumber);
                existing.CarThumbnail = previousThumbnailPath;
                return SaveResult.Fail(nameof(CarEditVm.ImageFile), IMAGE_UPLOAD_FAILED_MESSAGE);
            }

            existing.CarThumbnail = uploadedThumbnailPath;
        }

        var result = await TrySaveAsync($"Car {existing.VinNumber} updated.");
        if (!result.IsSuccess)
        {
            if (uploadedThumbnailPath is not null)
                await _carImageStorage.DeleteAsync(uploadedThumbnailPath);

            existing.CarThumbnail = previousThumbnailPath;
            return result;
        }

        if (uploadedThumbnailPath is not null &&
            !string.Equals(previousThumbnailPath, uploadedThumbnailPath, StringComparison.OrdinalIgnoreCase))
        {
            await _carImageStorage.DeleteAsync(previousThumbnailPath);
        }

        return result;
    }

    // Cars with historical bookings are preserved for referential integrity. If history exists, the
    // delete action decommissions the car by marking it Out of Service instead of removing the row.
    public async Task<SaveResult> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Attempted to delete car {CarId}, but it was not found.", id);
            return SaveResult.Fail("", $"Car {id} was not found.");
        }

        var activeOrUpcomingBookingCount = await _repo.CountActiveOrUpcomingBookingsAsync(id);
        if (activeOrUpcomingBookingCount > 0)
        {
            _logger.LogWarning(
                "Blocked delete for car {CarId} ({Vin}) because {BookingCount} active or upcoming bookings still exist.",
                existing.PkCarId,
                existing.VinNumber,
                activeOrUpcomingBookingCount);
            return SaveResult.Fail(
                string.Empty,
                $"Car {existing.VinNumber} has active or upcoming bookings. Resolve them before deleting it.");
        }

        if (await _repo.HasBookingsAsync(id))
        {
            var outOfServiceStatusId = await _repo.GetCarStatusIdByNameAsync(CarStatusNames.OUT_OF_SERVICE);
            if (outOfServiceStatusId is null)
            {
                _logger.LogError(
                    "Car {CarId} ({Vin}) has booking history but the Out of Service status is missing.",
                    existing.PkCarId,
                    existing.VinNumber);
                return SaveResult.Fail("", "Out of Service status is missing. Seed car statuses and try again.");
            }

            existing.FkCarStatusId = outOfServiceStatusId.Value;
            _logger.LogInformation(
                "Car {CarId} ({Vin}) has booking history and will be marked Out of Service instead of deleted.",
                existing.PkCarId,
                existing.VinNumber);

            return await TrySaveAsync($"Car {existing.VinNumber} has booking history, so it was marked Out of Service instead of being deleted.");
        }

        var thumbnailPath = existing.CarThumbnail;
        _repo.Remove(existing);

        var result = await TrySaveAsync($"Car {existing.VinNumber} deleted.");
        if (result.IsSuccess)
            await _carImageStorage.DeleteAsync(thumbnailPath);

        return result;
    }

    // ------------------------------------------------------------
    // Lookup data
    // ------------------------------------------------------------
    public Task<List<FuelType>> GetFuelTypesAsync() => _repo.GetFuelTypesAsync();

    public Task<List<VehicleClass>> GetVehicleClassesAsync() => _repo.GetVehicleClassesAsync();

    public Task<List<CarStatus>> GetCarStatusesAsync() => _repo.GetCarStatusesAsync();

    // ------------------------------------------------------------
    // Makes
    // ------------------------------------------------------------
    public async Task<IReadOnlyList<MakeListItemVm>> GetMakeListAsync()
    {
        var makes = await _repo.GetMakesAsync();
        return makes
            .Select(x => new MakeListItemVm
            {
                MakeId = x.PkMakeId,
                MakeName = x.MakeName,
                CanDelete = x.Models.Count == 0
            })
            .ToList();
    }

    public async Task<IReadOnlyList<Make>> GetMakeOptionsAsync()
    {
        var makes = await _repo.GetMakesAsync();
        return makes.Select(x => new Make
        {
            PkMakeId = x.PkMakeId,
            MakeName = x.MakeName
        }).ToList();
    }

    public Task<Make?> GetMakeByIdAsync(int id) => _repo.GetMakeByIdAsync(id);

    public async Task<SaveResult> CreateMakeAsync(string makeName)
    {
        var normalized = NormalizeName(makeName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(MakeIndexVm.MakeName), "Make name is required.");

        if (await _repo.MakeNameExistsAsync(normalized))
            return SaveResult.Fail(nameof(MakeIndexVm.MakeName), "Make already exists.");

        await _repo.AddMakeAsync(new Make { MakeName = normalized });
        return await TrySaveAsync("Make added.");
    }

    public async Task<SaveResult> UpdateMakeAsync(int id, string makeName)
    {
        var existing = await _repo.GetMakeByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail(string.Empty, "Make not found.");

        var normalized = NormalizeName(makeName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(MakeEditVm.MakeName), "Make name is required.");

        if (await _repo.MakeNameExistsAsync(normalized, id))
            return SaveResult.Fail(nameof(MakeEditVm.MakeName), "Make already exists.");

        existing.MakeName = normalized;
        return await TrySaveAsync("Make updated.");
    }

    public async Task<SaveResult> DeleteMakeAsync(int id)
    {
        var existing = await _repo.GetMakeByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail(string.Empty, "Make not found.");

        if (await _repo.MakeHasModelsAsync(id))
            return SaveResult.Fail(string.Empty, "Make cannot be deleted because it still has models.");

        _repo.RemoveMake(existing);
        return await TrySaveAsync("Make deleted.");
    }

    // ------------------------------------------------------------
    // Models
    // ------------------------------------------------------------
    public Task<List<Model>> GetModelsAsync(int? makeId = null) => _repo.GetModelsAsync(makeId);

    public async Task<IReadOnlyList<ModelListItemVm>> GetModelListAsync()
    {
        var models = await _repo.GetModelsAsync();
        return models
            .Select(x => new ModelListItemVm
            {
                ModelId = x.PkModelId,
                MakeId = x.FkMakeId,
                MakeName = x.FkMake.MakeName,
                ModelName = x.ModelName,
                CanDelete = x.Cars.Count == 0
            })
            .ToList();
    }

    public Task<Model?> GetModelByIdAsync(int id) => _repo.GetModelByIdAsync(id);

    public async Task<SaveResult> CreateModelAsync(int? makeId, string modelName)
    {
        if (makeId is null)
            return SaveResult.Fail(nameof(ModelIndexVm.FkMakeId), "Make is required.");

        if (!await _repo.MakeExistsAsync(makeId.Value))
            return SaveResult.Fail(nameof(ModelIndexVm.FkMakeId), "Selected make was not found.");

        var normalized = NormalizeName(modelName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(ModelIndexVm.ModelName), "Model name is required.");

        if (await _repo.ModelNameExistsAsync(makeId.Value, normalized))
            return SaveResult.Fail(nameof(ModelIndexVm.ModelName), "That model already exists for the selected make.");

        await _repo.AddModelAsync(new Model
        {
            FkMakeId = makeId.Value,
            ModelName = normalized
        });

        return await TrySaveAsync("Model added.");
    }

    public async Task<SaveResult> UpdateModelAsync(int id, int? makeId, string modelName)
    {
        var existing = await _repo.GetModelByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail(string.Empty, "Model not found.");

        if (makeId is null)
            return SaveResult.Fail(nameof(ModelEditVm.FkMakeId), "Make is required.");

        if (!await _repo.MakeExistsAsync(makeId.Value))
            return SaveResult.Fail(nameof(ModelEditVm.FkMakeId), "Selected make was not found.");

        var normalized = NormalizeName(modelName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(ModelEditVm.ModelName), "Model name is required.");

        if (await _repo.ModelNameExistsAsync(makeId.Value, normalized, id))
            return SaveResult.Fail(nameof(ModelEditVm.ModelName), "That model already exists for the selected make.");

        if (existing.FkMakeId != makeId.Value && await _repo.ModelHasCarsAsync(id))
        {
            return SaveResult.Fail(
                nameof(ModelEditVm.FkMakeId),
                "Make cannot be changed because this model is already assigned to cars.");
        }

        existing.FkMakeId = makeId.Value;
        existing.ModelName = normalized;

        return await TrySaveAsync("Model updated.");
    }

    public async Task<SaveResult> DeleteModelAsync(int id)
    {
        var existing = await _repo.GetModelByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail(string.Empty, "Model not found.");

        if (await _repo.ModelHasCarsAsync(id))
            return SaveResult.Fail(string.Empty, "Model cannot be deleted because it is assigned to cars.");

        _repo.RemoveModel(existing);
        return await TrySaveAsync("Model deleted.");
    }

    // ------------------------------------------------------------
    // Vehicle classes
    // ------------------------------------------------------------
    public async Task<IReadOnlyList<VehicleClassListItemVm>> GetVehicleClassListAsync()
    {
        var vehicleClasses = await _repo.GetVehicleClassesAsync();
        return vehicleClasses
            .Select(x => new VehicleClassListItemVm
            {
                VehicleClassId = x.PkVehicleClassId,
                VehicleClassName = x.VehicleClass1,
                CanDelete = x.Cars.Count == 0
            })
            .ToList();
    }

    public Task<VehicleClass?> GetVehicleClassByIdAsync(int id) => _repo.GetVehicleClassByIdAsync(id);

    public async Task<SaveResult> CreateVehicleClassAsync(string vehicleClassName)
    {
        var normalized = NormalizeName(vehicleClassName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(VehicleClassIndexVm.VehicleClassName), "Vehicle class is required.");

        if (await _repo.VehicleClassNameExistsAsync(normalized))
            return SaveResult.Fail(nameof(VehicleClassIndexVm.VehicleClassName), "Vehicle class already exists.");

        await _repo.AddVehicleClassAsync(new VehicleClass { VehicleClass1 = normalized });
        return await TrySaveAsync("Vehicle class added.");
    }

    public async Task<SaveResult> UpdateVehicleClassAsync(int id, string vehicleClassName)
    {
        var existing = await _repo.GetVehicleClassByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail(string.Empty, "Vehicle class not found.");

        var normalized = NormalizeName(vehicleClassName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(VehicleClassEditVm.VehicleClassName), "Vehicle class is required.");

        if (await _repo.VehicleClassNameExistsAsync(normalized, id))
            return SaveResult.Fail(nameof(VehicleClassEditVm.VehicleClassName), "Vehicle class already exists.");

        existing.VehicleClass1 = normalized;
        return await TrySaveAsync("Vehicle class updated.");
    }

    public async Task<SaveResult> DeleteVehicleClassAsync(int id)
    {
        var existing = await _repo.GetVehicleClassByIdAsync(id);
        if (existing is null)
            return SaveResult.Fail(string.Empty, "Vehicle class not found.");

        if (await _repo.VehicleClassHasCarsAsync(id))
            return SaveResult.Fail(string.Empty, "Vehicle class cannot be deleted because it is assigned to cars.");

        _repo.RemoveVehicleClass(existing);
        return await TrySaveAsync("Vehicle class deleted.");
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------
    private async Task<SaveResult> TrySaveAsync(string successMessage)
    {
        try
        {
            await _repo.SaveChangesAsync();
            _logger.LogInformation("{Message}", successMessage);
            return SaveResult.Ok(successMessage);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Car-related save operation failed due to a database constraint.");
            return SaveResult.Fail(string.Empty, "Save failed due to a database constraint. Please refresh and try again.");
        }
    }

    private async Task<List<(string Field, string Message)>> ValidateAsync(CarEditVm vm, int? excludeCarId = null)
    {
        var errors = _carImageStorage.Validate(vm.ImageFile)
            .Select(message => (nameof(CarEditVm.ImageFile), message))
            .ToList();

        if (await _repo.VinExistsAsync(vm.VinNumber, excludeCarId))
            errors.Add((nameof(vm.VinNumber), "VIN already exists."));

        if (await _repo.PlateExistsAsync(vm.LicencePlate, excludeCarId))
            errors.Add((nameof(vm.LicencePlate), "Licence plate already exists."));

        return errors;
    }

    // Booked cars are allowed to flip between customer-visible availability states, but not into
    // maintenance or other statuses that would contradict already-accepted reservations. Booking
    // hold is useful because it temporarily removes the car from new public bookings without
    // invalidating reservations that already exist for that vehicle. This prevents the situation
    // where a car is booked far into the future, which would otherwise block admin edits.
    private async Task<SaveResult> ValidateBookedCarStatusChangeAsync(int newStatusId, Car existing)
    {
        if (newStatusId == existing.FkCarStatusId)
            return SaveResult.Ok(string.Empty);

        var availableStatusId = await _repo.GetCarStatusIdByNameAsync(CarStatusNames.AVAILABLE);
        var bookingHoldStatusId = await _repo.GetCarStatusIdByNameAsync(CarStatusNames.BOOKING_HOLD);

        if (availableStatusId is null || bookingHoldStatusId is null)
        {
            _logger.LogError(
                "Status validation for car {CarId} ({Vin}) failed because required statuses are missing.",
                existing.PkCarId,
                existing.VinNumber);
            return SaveResult.Fail(
                string.Empty,
                "Required car statuses are missing. Seed car statuses and try again.");
        }

        var isAllowedTransition =
            existing.FkCarStatusId == availableStatusId.Value && newStatusId == bookingHoldStatusId.Value ||
            existing.FkCarStatusId == bookingHoldStatusId.Value && newStatusId == availableStatusId.Value;

        if (isAllowedTransition)
            return SaveResult.Ok(string.Empty);

        _logger.LogWarning(
            "Blocked status change for car {CarId} ({Vin}) from status {CurrentStatusId} to {NewStatusId} while bookings are active or upcoming.",
            existing.PkCarId,
            existing.VinNumber,
            existing.FkCarStatusId,
            newStatusId);
        return SaveResult.Fail(
            nameof(CarEditVm.FkCarStatusId),
            "While a car has active or upcoming bookings, status can only switch between Available and Booking Hold.");
    }

    // These fields affect the identity, pricing, or characteristics a customer booked against, so
    // they are treated as locked once a reservation exists for the car.
    private static bool HasLockedFieldChanges(CarEditVm vm, Car car) =>
        vm.Year != car.Year ||
        !string.Equals(vm.Colour, car.Colour, StringComparison.Ordinal) ||
        vm.FkModelId != car.FkModelId ||
        vm.FkFuelTypeId != car.FkFuelTypeId ||
        vm.FkVehicleClassId != car.FkVehicleClassId ||
        vm.TransmissionType != car.TransmissionType ||
        vm.DailyRate != car.DailyRate ||
        vm.PersonCap != car.PersonCap ||
        vm.LuggageCap != car.LuggageCap ||
        !string.Equals(vm.LicencePlate, car.LicencePlate, StringComparison.Ordinal) ||
        !string.Equals(vm.VinNumber, car.VinNumber, StringComparison.Ordinal);

    private static string NormalizeName(string? value) => value?.Trim() ?? string.Empty;
}
