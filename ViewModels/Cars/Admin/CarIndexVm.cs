using LuxRentals.Models;
using LuxRentals.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Cars.Admin;

public class CarIndexVm
{
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Status")]
    public int? StatusId { get; set; }

    [Display(Name = "Class")]
    public int? VehicleClassId { get; set; }

    [Display(Name = "Make")]
    public int? MakeId { get; set; }

    [Display(Name = "Model")]
    public int? ModelId { get; set; }

    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "id_desc";

    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> VehicleClassOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> MakeOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> ModelOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> SortOptions { get; set; } = [];

    public IReadOnlyList<CarListItemVm> Cars { get; set; } = [];
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    public void ApplyPagedResult(PagedList<Car> pagedCars)
    {
        Cars = pagedCars.Items.Select(CarListItemVm.FromEntity).ToList();
        Page = pagedCars.Page;
        TotalPages = pagedCars.TotalPages;
        TotalCount = pagedCars.TotalCount;
        HasPreviousPage = pagedCars.HasPreviousPage;
        HasNextPage = pagedCars.HasNextPage;
    }
}
