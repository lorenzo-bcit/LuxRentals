using LuxRentals.Repositories.Cars;
using LuxRentals.Services.Cars;
using LuxRentals.Utils;
using LuxRentals.ViewModels.Cars;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers;

public class CarsController : Controller
{
    private const int PAGE_SIZE = 5;
    private const int DEFAULT_BOOKING_WINDOW_DAYS = 7;
    private const int MAX_SEATS_FILTER = 10;
    private const int MAX_LUGGAGE_FILTER = 10;
    private const decimal MAX_RATE_FILTER = 500m;

    private readonly ICarService _carService;

    public CarsController(ICarService carService) => _carService = carService;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CarBrowseVm vm)
    {
        NormalizeBrowseState(vm);
        vm.MaxSeatsFilter = MAX_SEATS_FILTER;
        vm.MaxLuggageFilter = MAX_LUGGAGE_FILTER;
        vm.MaxRateFilter = MAX_RATE_FILTER;

        var hasInvalidDateRange =
            vm.StartDate.HasValue &&
            vm.EndDate.HasValue &&
            vm.EndDate.Value <= vm.StartDate.Value;

        if (hasInvalidDateRange)
            ModelState.AddModelError(nameof(vm.EndDate), "Drop-off date must be after pick-up date.");

        var criteria = ToCriteria(vm, hasInvalidDateRange);
        var pagedCars = await _carService.SearchAsync(criteria);

        vm.ApplyPagedResult(pagedCars);
        vm.MaxRate ??= MAX_RATE_FILTER;

        await PopulateLookupOptionsAsync(vm);

        return View(vm);
    }

    private static void NormalizeBrowseState(CarBrowseVm vm)
    {
        vm.Page = Math.Max(1, vm.Page);

        var today = DateTime.Today;

        if (!vm.StartDate.HasValue && !vm.EndDate.HasValue)
        {
            vm.StartDate = today;
            vm.EndDate = today.AddDays(DEFAULT_BOOKING_WINDOW_DAYS);
            return;
        }

        if (!vm.StartDate.HasValue)
            vm.StartDate = vm.EndDate?.Date.AddDays(-DEFAULT_BOOKING_WINDOW_DAYS) ?? today;

        if (!vm.EndDate.HasValue)
            vm.EndDate = vm.StartDate.Value.Date.AddDays(DEFAULT_BOOKING_WINDOW_DAYS);
    }

    private static CarSearchCriteria ToCriteria(CarBrowseVm vm, bool hasInvalidDateRange)
    {
        var criteria = new CarSearchCriteria
        {
            MakeId = vm.MakeId,
            FuelTypeId = vm.FuelTypeId,
            VehicleClassId = vm.VehicleClassId,
            TransmissionType = vm.TransmissionType.HasValue ? (byte)vm.TransmissionType.Value : null,
            MinSeats = vm.MinSeats,
            MinLuggage = vm.MinLuggage,
            MaxRate = vm.MaxRate,
            AvailableOnly = true, // this controller is public-facing, we only want to show available
            StartDate = hasInvalidDateRange ? null : vm.StartDate,
            EndDate = hasInvalidDateRange ? null : vm.EndDate,
            Page = vm.Page,
            PageSize = PAGE_SIZE
        };

        var sortBy = vm.SortBy?.ToLowerInvariant();

        switch (sortBy)
        {
            case "price_asc":
                vm.SortBy = "price_asc";
                criteria.SortBy = "rate";
                criteria.SortDescending = false;
                break;
            case "price_desc":
                vm.SortBy = "price_desc";
                criteria.SortBy = "rate";
                criteria.SortDescending = true;
                break;
            case "year_desc":
                vm.SortBy = "year_desc";
                criteria.SortBy = "year";
                criteria.SortDescending = true;
                break;
            case "year_asc":
                vm.SortBy = "year_asc";
                criteria.SortBy = "year";
                criteria.SortDescending = false;
                break;
            default:
                vm.SortBy = "price_asc";
                criteria.SortBy = "rate";
                criteria.SortDescending = false;
                break;
        }

        return criteria;
    }

    private async Task PopulateLookupOptionsAsync(CarBrowseVm vm)
    {
        var makes = await _carService.GetMakeOptionsAsync();
        vm.MakeOptions = SelectListItems.Build(
            makes,
            x => x.MakeName,
            x => x.PkMakeId.ToString(),
            x => x.PkMakeId == vm.MakeId,
            emptyText: "Any",
            emptySelected: vm.MakeId == null);

        var fuelTypes = await _carService.GetFuelTypesAsync();
        vm.FuelTypeOptions = SelectListItems.Build(
            fuelTypes,
            x => x.FuelType1,
            x => x.PkFuelTypeId.ToString(),
            x => x.PkFuelTypeId == vm.FuelTypeId,
            emptyText: "Any",
            emptySelected: vm.FuelTypeId == null);

        var classes = await _carService.GetVehicleClassesAsync();
        vm.VehicleClassOptions = SelectListItems.Build(
            classes,
            x => x.VehicleClass1,
            x => x.PkVehicleClassId.ToString(),
            x => x.PkVehicleClassId == vm.VehicleClassId,
            emptyText: "Any",
            emptySelected: vm.VehicleClassId == null);
    }
}
