using LuxRentals.Models;
using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Services.Cars;

public class CarLookupAdminService : ICarLookupAdminService
{
    private readonly ICarLookupAdminRepository _repo;

    public CarLookupAdminService(ICarLookupAdminRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<AdminMakeListItemVm>> GetMakesAsync()
    {
        var makes = await _repo.GetMakesAsync();
        return makes
            .Select(x => new AdminMakeListItemVm
            {
                MakeId = x.PkMakeId,
                MakeName = x.MakeName,
                CanDelete = x.Models.Count == 0
            })
            .ToList();
    }

    public Task<Make?> GetMakeByIdAsync(int id) => _repo.GetMakeByIdAsync(id);

    public async Task<SaveResult> CreateMakeAsync(string makeName)
    {
        var normalized = NormalizeName(makeName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(AdminMakeIndexVm.MakeName), "Make name is required.");

        if (await _repo.MakeNameExistsAsync(normalized))
            return SaveResult.Fail(nameof(AdminMakeIndexVm.MakeName), "Make already exists.");

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
            return SaveResult.Fail(nameof(AdminMakeEditVm.MakeName), "Make name is required.");

        if (await _repo.MakeNameExistsAsync(normalized, id))
            return SaveResult.Fail(nameof(AdminMakeEditVm.MakeName), "Make already exists.");

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

    public async Task<IReadOnlyList<AdminModelListItemVm>> GetModelsAsync()
    {
        var models = await _repo.GetModelsAsync();
        return models
            .Select(x => new AdminModelListItemVm
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
            return SaveResult.Fail(nameof(AdminModelIndexVm.FkMakeId), "Make is required.");

        if (!await _repo.MakeExistsAsync(makeId.Value))
            return SaveResult.Fail(nameof(AdminModelIndexVm.FkMakeId), "Selected make was not found.");

        var normalized = NormalizeName(modelName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(AdminModelIndexVm.ModelName), "Model name is required.");

        if (await _repo.ModelNameExistsAsync(makeId.Value, normalized))
            return SaveResult.Fail(nameof(AdminModelIndexVm.ModelName), "That model already exists for the selected make.");

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
            return SaveResult.Fail(nameof(AdminModelEditVm.FkMakeId), "Make is required.");

        if (!await _repo.MakeExistsAsync(makeId.Value))
            return SaveResult.Fail(nameof(AdminModelEditVm.FkMakeId), "Selected make was not found.");

        var normalized = NormalizeName(modelName);
        if (string.IsNullOrWhiteSpace(normalized))
            return SaveResult.Fail(nameof(AdminModelEditVm.ModelName), "Model name is required.");

        if (await _repo.ModelNameExistsAsync(makeId.Value, normalized, id))
            return SaveResult.Fail(nameof(AdminModelEditVm.ModelName), "That model already exists for the selected make.");

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

    public async Task<IReadOnlyList<Make>> GetMakeOptionsAsync()
    {
        var makes = await _repo.GetMakesAsync();
        return makes.Select(x => new Make
        {
            PkMakeId = x.PkMakeId,
            MakeName = x.MakeName
        }).ToList();
    }

    public async Task<IReadOnlyList<AdminVehicleClassListItemVm>> GetVehicleClassesAsync()
    {
        var vehicleClasses = await _repo.GetVehicleClassesAsync();
        return vehicleClasses
            .Select(x => new AdminVehicleClassListItemVm
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
            return SaveResult.Fail(nameof(AdminVehicleClassIndexVm.VehicleClassName), "Vehicle class is required.");

        if (await _repo.VehicleClassNameExistsAsync(normalized))
            return SaveResult.Fail(nameof(AdminVehicleClassIndexVm.VehicleClassName), "Vehicle class already exists.");

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
            return SaveResult.Fail(nameof(AdminVehicleClassEditVm.VehicleClassName), "Vehicle class is required.");

        if (await _repo.VehicleClassNameExistsAsync(normalized, id))
            return SaveResult.Fail(nameof(AdminVehicleClassEditVm.VehicleClassName), "Vehicle class already exists.");

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

    private async Task<SaveResult> TrySaveAsync(string successMessage)
    {
        try
        {
            await _repo.SaveChangesAsync();
            return SaveResult.Ok(successMessage);
        }
        catch (DbUpdateException)
        {
            return SaveResult.Fail(string.Empty, "Save failed due to a database constraint. Please refresh and try again.");
        }
    }

    private static string NormalizeName(string? value) => value?.Trim() ?? string.Empty;
}
