using LuxRentals.Repositories.Cars;
using LuxRentals.ViewModels.Cars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.Controllers;

public class CarsController : Controller
{
    private const int PAGE_SIZE = 5;

    private readonly ICarReadRepository _carReadRepository;
    private readonly ICarLookupRepository _carLookupRepository;

    public CarsController(ICarReadRepository carReadRepository, ICarLookupRepository carLookupRepository)
    {
        _carReadRepository = carReadRepository;
        _carLookupRepository = carLookupRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CarBrowseVm vm)
    {
        NormalizePage(vm);

        var hasInvalidDateRange =
            vm.StartDate.HasValue &&
            vm.EndDate.HasValue &&
            vm.EndDate.Value <= vm.StartDate.Value;

        if (hasInvalidDateRange)
            ModelState.AddModelError(nameof(vm.EndDate), "Drop-off date must be after pick-up date.");

        var criteria = ToCriteria(vm, hasInvalidDateRange);
        var pagedCars = await _carReadRepository.SearchAsync(criteria);

        vm.ApplyPagedResult(pagedCars);

        await PopulateLookupOptionsAsync(vm);

        return View(vm);
    }

    private static void NormalizePage(CarBrowseVm vm)
    {
        vm.Page = Math.Max(1, vm.Page);
    }

    private static CarSearchCriteria ToCriteria(CarBrowseVm vm, bool hasInvalidDateRange)
    {
        var criteria = new CarSearchCriteria
        {
            FuelTypeId = vm.FuelTypeId,
            VehicleClassId = vm.VehicleClassId,
            TransmissionType = vm.TransmissionType.HasValue ? (byte)vm.TransmissionType.Value : null,
            MinSeats = vm.MinSeats,
            MinLuggage = vm.MinLuggage,
            AvailableOnly = vm.AvailableOnly,
            StartDate = hasInvalidDateRange ? null : vm.StartDate,
            EndDate = hasInvalidDateRange ? null : vm.EndDate,
            Page = vm.Page,
            PageSize = PAGE_SIZE
        };

        var sortBy = vm.SortBy?.ToLowerInvariant();

        switch (sortBy)
        {
            case "price_asc":
                criteria.SortBy = "rate";
                criteria.SortDescending = false;
                break;
            case "price_desc":
                criteria.SortBy = "rate";
                criteria.SortDescending = true;
                break;
            default:
                vm.SortBy = "featured";
                criteria.SortBy = null;
                criteria.SortDescending = false;
                break;
        }

        return criteria;
    }

    private async Task PopulateLookupOptionsAsync(CarBrowseVm vm)
    {
        var fuelTypes = await _carLookupRepository.GetFuelTypesAsync();
        vm.FuelTypeOptions = fuelTypes
            .Select(x => new SelectListItem(x.FuelType1, x.PkFuelTypeId.ToString(), x.PkFuelTypeId == vm.FuelTypeId))
            .ToList();

        var classes = await _carLookupRepository.GetVehicleClassesAsync();
        vm.VehicleClassOptions = classes
            .Select(x => new SelectListItem(x.VehicleClass1, x.PkVehicleClassId.ToString(), x.PkVehicleClassId == vm.VehicleClassId))
            .ToList();
    }
}
