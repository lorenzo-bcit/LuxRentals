using LuxRentals.Models;
using LuxRentals.Repositories;

namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminCarIndexVm
{
    public IReadOnlyList<AdminCarListItemVm> Cars { get; set; } = [];
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    public void ApplyPagedResult(PagedList<Car> pagedCars)
    {
        Cars = pagedCars.Items.Select(AdminCarListItemVm.FromEntity).ToList();
        Page = pagedCars.Page;
        TotalPages = pagedCars.TotalPages;
        TotalCount = pagedCars.TotalCount;
        HasPreviousPage = pagedCars.HasPreviousPage;
        HasNextPage = pagedCars.HasNextPage;
    }
}
