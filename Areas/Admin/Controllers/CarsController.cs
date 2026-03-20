using LuxRentals.Repositories.Cars;
using LuxRentals.Services.Cars;
using LuxRentals.Utils;
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

        var result = await _carService.CreateAsync(vm);
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

        var vm = CarEditVm.FromEntity(car);
        await PopulateActiveOrUpcomingBookingsAsync(vm, id);
        await PopulateOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CarEditVm vm)
    {
        if (vm.CarId != id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateActiveOrUpcomingBookingsAsync(vm, id);
            await PopulateOptionsAsync(vm);
            return View(vm);
        }

        var result = await _carService.UpdateAsync(id, vm);
        if (!result.IsSuccess)
        {
            AddErrorsToModelState(result);
            await PopulateActiveOrUpcomingBookingsAsync(vm, id);
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
            var key = string.IsNullOrWhiteSpace(error.Field)
                ? string.Empty
                : error.Field == nameof(CarEditVm.ImageFile)
                    ? error.Field
                    : error.Field;
            ModelState.AddModelError(key, error.Message);
        }
    }

    private async Task PopulateOptionsAsync(CarEditVm vm)
    {
        var fuelTypes = await _carService.GetFuelTypesAsync();
        vm.FuelTypeOptions = SelectListItems.Build(
            fuelTypes,
            x => x.FuelType1,
            x => x.PkFuelTypeId.ToString(),
            x => x.PkFuelTypeId == vm.FkFuelTypeId);

        var classes = await _carService.GetVehicleClassesAsync();
        vm.VehicleClassOptions = SelectListItems.Build(
            classes,
            x => x.VehicleClass1,
            x => x.PkVehicleClassId.ToString(),
            x => x.PkVehicleClassId == vm.FkVehicleClassId);

        var statuses = await _carService.GetCarStatusesAsync();
        vm.CarStatusOptions = SelectListItems.Build(
            statuses,
            x => x.StatusFlag,
            x => x.PkCarStatusId.ToString(),
            x => x.PkCarStatusId == vm.FkCarStatusId);

        var models = await _carService.GetModelsAsync();
        vm.ModelOptions = SelectListItems.Build(
            models,
            x => $"{x.FkMake.MakeName} {x.ModelName}",
            x => x.PkModelId.ToString(),
            x => x.PkModelId == vm.FkModelId);

        vm.TransmissionOptions =
        [
            new SelectListItem("Manual", "0", vm.TransmissionType == 0),
            new SelectListItem("Automatic", "1", vm.TransmissionType == 1)
        ];
    }

    private async Task PopulateActiveOrUpcomingBookingsAsync(CarEditVm vm, int carId)
    {
        var bookingToday = BookingClock.Today();
        var bookings = await _carService.GetActiveOrUpcomingBookingsAsync(carId);
        vm.ActiveOrUpcomingBookings = bookings
            .Select(b => CarBookingSummaryVm.FromEntity(b, bookingToday))
            .ToList();
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
            HasActiveOrUpcomingBookingsOnly = vm.HasActiveOrUpcomingBookingsOnly,
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

    private async Task PopulateIndexOptionsAsync(CarIndexVm vm)
    {
        var statuses = await _carService.GetCarStatusesAsync();
        vm.StatusOptions = SelectListItems.Build(
            statuses,
            x => x.StatusFlag,
            x => x.PkCarStatusId.ToString(),
            x => x.PkCarStatusId == vm.StatusId,
            emptyText: "Any status",
            emptySelected: vm.StatusId == null);

        var classes = await _carService.GetVehicleClassesAsync();
        vm.VehicleClassOptions = SelectListItems.Build(
            classes,
            x => x.VehicleClass1,
            x => x.PkVehicleClassId.ToString(),
            x => x.PkVehicleClassId == vm.VehicleClassId,
            emptyText: "Any class",
            emptySelected: vm.VehicleClassId == null);

        var makes = await _carService.GetMakeOptionsAsync();
        vm.MakeOptions = SelectListItems.Build(
            makes,
            x => x.MakeName,
            x => x.PkMakeId.ToString(),
            x => x.PkMakeId == vm.MakeId,
            emptyText: "Any make",
            emptySelected: vm.MakeId == null);

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
