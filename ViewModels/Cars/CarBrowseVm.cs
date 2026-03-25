using LuxRentals.Models;
using LuxRentals.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.ViewModels.Cars;

public class CarBrowseVm
{
    public int? FuelTypeId { get; set; }

    public int? VehicleClassId { get; set; }

    public int? MakeId { get; set; }

    public int? TransmissionType { get; set; }

    public int MinSeats { get; set; }

    public int MinLuggage { get; set; }

    public decimal? MaxRate { get; set; }

    public decimal MaxSelectableRate { get; set; }

    public int MaxSelectableSeats { get; set; }

    public int MaxSelectableLuggage { get; set; }

    public int Page { get; set; } = 1;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string SortBy { get; set; } = "price_asc";

    public string MinBrowseDate { get; set; } = string.Empty;

    public string EmptyStateMessage { get; set; } = "No cars match your filters.";

    public IReadOnlyList<CarCardVm> Cars { get; set; } = [];

    public IReadOnlyList<SelectListItem> FuelTypeOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> VehicleClassOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> MakeOptions { get; set; } = [];

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage { get; set; }

    public bool HasNextPage { get; set; }

    public void ApplyPagedResult(PagedList<Car> pagedCars)
    {
        Cars = pagedCars.Items.Select(CarCardVm.FromEntity).ToList();
        Page = pagedCars.Page;
        TotalCount = pagedCars.TotalCount;
        TotalPages = pagedCars.TotalPages;
        HasNextPage = pagedCars.HasNextPage;
        HasPreviousPage = pagedCars.HasPreviousPage;
    }
}