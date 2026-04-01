using LuxRentals.Repositories.Cars;
using LuxRentals.Services.Cars;
using LuxRentals.Utils;
using LuxRentals.ViewModels.Cars;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers;

public class CarsController : Controller
{
    private const int PAGE_SIZE = 6;
    private const int DEFAULT_BOOKING_WINDOW_DAYS = 7;
    private const int MAX_SELECTABLE_SEATS = 10;
    private const int MAX_SELECTABLE_LUGGAGE = 10;
    private const decimal MAX_SELECTABLE_RATE = 1000m;

    private readonly ICarService _carService;

    public CarsController(ICarService carService) => _carService = carService;

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CarBrowseVm vm)
    {
        var tomorrow = BookingClock.Tomorrow();

        NormalizeBrowseState(vm, tomorrow);
        vm.MaxSelectableSeats = MAX_SELECTABLE_SEATS;
        vm.MaxSelectableLuggage = MAX_SELECTABLE_LUGGAGE;
        vm.MaxSelectableRate = MAX_SELECTABLE_RATE;
        vm.MinBrowseDate = tomorrow.ToString("yyyy-MM-dd");

        var hasInvalidBrowseDates = HasInvalidBrowseDates(vm, tomorrow);
        vm.EmptyStateMessage = hasInvalidBrowseDates
            ? "Adjust the date range to see available cars."
            : "No cars match your filters.";

        var criteria = ToCriteria(vm, hasInvalidBrowseDates);
        var pagedCars = await _carService.SearchAsync(criteria);

        vm.ApplyPagedResult(pagedCars);
        vm.MaxRate ??= MAX_SELECTABLE_RATE;

        await PopulateLookupOptionsAsync(vm);

        return View(vm);
    }

    private static void NormalizeBrowseState(CarBrowseVm vm, DateTime tomorrow)
    {
        vm.Page = Math.Max(1, vm.Page);

        if (!vm.StartDate.HasValue && !vm.EndDate.HasValue)
        {
            vm.StartDate = tomorrow;
            vm.EndDate = tomorrow.AddDays(DEFAULT_BOOKING_WINDOW_DAYS);
        }
    }

    private static bool HasInvalidBrowseDates(CarBrowseVm vm, DateTime tomorrow) =>
        vm.StartDate.HasValue && vm.StartDate.Value.Date < tomorrow ||
        vm.StartDate.HasValue &&
        vm.EndDate.HasValue &&
        vm.EndDate.Value.Date <= vm.StartDate.Value.Date;

    private static CarSearchCriteria ToCriteria(CarBrowseVm vm, bool hasInvalidBrowseDates)
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
            OnlyBookableCars = true, // public browse only shows cars that can be booked
            StartDate = hasInvalidBrowseDates ? null : vm.StartDate,
            EndDate = hasInvalidBrowseDates ? null : vm.EndDate,
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
