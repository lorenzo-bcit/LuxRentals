using LuxRentals.Repositories.Cars;
using LuxRentals.Services.Cars;
using LuxRentals.ViewModels.Cars;
using LuxRentals.ViewModels.Cars.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CarsController : Controller
{
    private const int PAGE_SIZE = 10;

    private readonly ICarService _carService;

    public CarsController(ICarService carService) => _carService = carService;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CarIndexVm vm)
    {
        NormalizePage(vm);
        await NormalizeModelFilterAsync(vm);
        var criteria = ToCriteria(vm);

        var pagedCars = await _carService.SearchAsync(criteria);
        vm.ApplyPagedResult(pagedCars);
        await PopulateIndexOptionsAsync(vm);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CarEditVm();
        await PopulateOptionsAsync(vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CarEditVm vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(vm);
            return View(vm);
        }

        var result = await _carService.CreateAsync(vm.Car);
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            await PopulateOptionsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var car = await _carService.GetByIdAsync(id);
        if (car is null)
            return NotFound();

        var vm = new CarEditVm
        {
            Car = CarUpsertVm.FromEntity(car)
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CarEditVm vm)
    {
        if (vm.Car.CarId != id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(vm);
            return View(vm);
        }

        var result = await _carService.UpdateAsync(id, vm.Car);
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            await PopulateOptionsAsync(vm);
            return View(vm);
        }

        TempData["StatusMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _carService.DeleteAsync(id);
        TempData[result.IsSuccess ? "StatusMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    private void AddErrorsToModelState(SaveResult result)
    {
        foreach (var error in result.Errors)
        {
            var key = string.IsNullOrWhiteSpace(error.Field) ? string.Empty : $"Car.{error.Field}";
            ModelState.AddModelError(key, error.Message);
        }
    }

    private async Task PopulateOptionsAsync(CarEditVm vm)
    {
        var fuelTypes = await _carService.GetFuelTypesAsync();
        vm.FuelTypeOptions = fuelTypes
            .Select(x => new SelectListItem(x.FuelType1, x.PkFuelTypeId.ToString(), x.PkFuelTypeId == vm.Car.FkFuelTypeId))
            .ToList();

        var classes = await _carService.GetVehicleClassesAsync();
        vm.VehicleClassOptions = classes
            .Select(x => new SelectListItem(x.VehicleClass1, x.PkVehicleClassId.ToString(), x.PkVehicleClassId == vm.Car.FkVehicleClassId))
            .ToList();

        var statuses = await _carService.GetCarStatusesAsync();
        vm.CarStatusOptions = statuses
            .Select(x => new SelectListItem(x.StatusFlag, x.PkCarStatusId.ToString(), x.PkCarStatusId == vm.Car.FkCarStatusId))
            .ToList();

        var models = await _carService.GetModelsAsync();
        vm.ModelOptions = models
            .Select(x => new SelectListItem($"{x.FkMake.MakeName} {x.ModelName}", x.PkModelId.ToString(), x.PkModelId == vm.Car.FkModelId))
            .ToList();

        vm.TransmissionOptions =
        [
            new SelectListItem("Manual", "0", vm.Car.TransmissionType == 0),
            new SelectListItem("Automatic", "1", vm.Car.TransmissionType == 1)
        ];
    }

    private static void NormalizePage(CarIndexVm vm)
    {
        vm.Page = Math.Max(1, vm.Page);
        vm.SortBy = string.IsNullOrWhiteSpace(vm.SortBy) ? "id_desc" : vm.SortBy;
        vm.SearchTerm = string.IsNullOrWhiteSpace(vm.SearchTerm) ? null : vm.SearchTerm.Trim();
    }

    private static CarSearchCriteria ToCriteria(CarIndexVm vm)
    {
        var criteria = new CarSearchCriteria
        {
            AvailableOnly = false,
            SearchTerm = vm.SearchTerm,
            StatusId = vm.StatusId,
            VehicleClassId = vm.VehicleClassId,
            MakeId = vm.MakeId,
            ModelId = vm.ModelId,
            Page = vm.Page,
            PageSize = PAGE_SIZE
        };

        switch (vm.SortBy.ToLowerInvariant())
        {
            case "year_desc":
                criteria.SortBy = "year";
                criteria.SortDescending = true;
                break;
            case "year_asc":
                criteria.SortBy = "year";
                criteria.SortDescending = false;
                break;
            case "rate_desc":
                criteria.SortBy = "rate";
                criteria.SortDescending = true;
                break;
            case "rate_asc":
                criteria.SortBy = "rate";
                criteria.SortDescending = false;
                break;
            case "make_desc":
                criteria.SortBy = "make";
                criteria.SortDescending = true;
                break;
            case "make_asc":
                criteria.SortBy = "make";
                criteria.SortDescending = false;
                break;
            case "status_desc":
                criteria.SortBy = "status";
                criteria.SortDescending = true;
                break;
            case "status_asc":
                criteria.SortBy = "status";
                criteria.SortDescending = false;
                break;
            default:
                vm.SortBy = "id_desc";
                criteria.SortBy = "id";
                criteria.SortDescending = true;
                break;
        }

        return criteria;
    }

    private async Task NormalizeModelFilterAsync(CarIndexVm vm)
    {
        if (vm.ModelId is null)
            return;

        if (vm.MakeId is null)
        {
            vm.ModelId = null;
            return;
        }

        var models = await _carService.GetModelsAsync(vm.MakeId);
        if (models.All(x => x.PkModelId != vm.ModelId.Value))
            vm.ModelId = null;
    }

    private async Task PopulateIndexOptionsAsync(CarIndexVm vm)
    {
        var statuses = await _carService.GetCarStatusesAsync();
        vm.StatusOptions = new List<SelectListItem>
        {
            new("Any status", "", vm.StatusId == null)
        }
        .Concat(statuses.Select(x =>
            new SelectListItem(x.StatusFlag, x.PkCarStatusId.ToString(), x.PkCarStatusId == vm.StatusId)))
        .ToList();

        var classes = await _carService.GetVehicleClassesAsync();
        vm.VehicleClassOptions = new List<SelectListItem>
        {
            new("Any class", "", vm.VehicleClassId == null)
        }
        .Concat(classes.Select(x =>
            new SelectListItem(x.VehicleClass1, x.PkVehicleClassId.ToString(), x.PkVehicleClassId == vm.VehicleClassId)))
        .ToList();

        var makes = await _carService.GetMakeOptionsAsync();
        vm.MakeOptions = new List<SelectListItem>
        {
            new("Any make", "", vm.MakeId == null)
        }
        .Concat(makes.Select(x =>
            new SelectListItem(x.MakeName, x.PkMakeId.ToString(), x.PkMakeId == vm.MakeId)))
        .ToList();

        var models = vm.MakeId.HasValue
            ? await _carService.GetModelsAsync(vm.MakeId)
            : [];

        vm.ModelOptions = new List<SelectListItem>
        {
            new(vm.MakeId.HasValue ? "Any model" : "Select make first", "", vm.ModelId == null)
        }
        .Concat(models.Select(x =>
            new SelectListItem(x.ModelName, x.PkModelId.ToString(), x.PkModelId == vm.ModelId)))
        .ToList();

        vm.SortOptions =
        [
            new SelectListItem("Recently added", "id_desc", vm.SortBy == "id_desc"),
            new SelectListItem("Newest year", "year_desc", vm.SortBy == "year_desc"),
            new SelectListItem("Oldest year", "year_asc", vm.SortBy == "year_asc"),
            new SelectListItem("Rate: high to low", "rate_desc", vm.SortBy == "rate_desc"),
            new SelectListItem("Rate: low to high", "rate_asc", vm.SortBy == "rate_asc"),
            new SelectListItem("Make / model A-Z", "make_asc", vm.SortBy == "make_asc"),
            new SelectListItem("Make / model Z-A", "make_desc", vm.SortBy == "make_desc"),
            new SelectListItem("Status A-Z", "status_asc", vm.SortBy == "status_asc"),
            new SelectListItem("Status Z-A", "status_desc", vm.SortBy == "status_desc")
        ];
    }
}
