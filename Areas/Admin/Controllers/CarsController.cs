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
    public async Task<IActionResult> Index(int page = 1)
    {
        var criteria = new CarSearchCriteria
        {
            AvailableOnly = false,
            Page = Math.Max(1, page),
            PageSize = PAGE_SIZE
        };

        var pagedCars = await _carService.SearchAsync(criteria);
        var vm = new AdminCarIndexVm();
        vm.ApplyPagedResult(pagedCars);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new AdminCarEditVm();
        await PopulateOptionsAsync(vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCarEditVm vm)
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

        var vm = new AdminCarEditVm
        {
            Car = CarUpsertVm.FromEntity(car)
        };

        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminCarEditVm vm)
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

    private async Task PopulateOptionsAsync(AdminCarEditVm vm)
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
}
