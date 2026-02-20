using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.ViewModels.Cars;

public class CarBrowseVm
{
    public int? FuelTypeId { get; set; }

    public int? VehicleClassId { get; set; }

    public int? TransmissionType { get; set; }

    public int? MinSeats { get; set; }

    public int? MinLuggage { get; set; }

    public bool AvailableOnly { get; set; } = true;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string SortBy { get; set; } = "featured";

    public IReadOnlyList<CarCardVm> Cars { get; set; } = [];

    public IReadOnlyList<SelectListItem> FuelTypeOptions { get; set; } = [];

    public IReadOnlyList<SelectListItem> VehicleClassOptions { get; set; } = [];

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
