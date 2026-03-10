using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminCarEditVm
{
    public CarUpsertVm Car { get; set; } = new();

    public IReadOnlyList<SelectListItem> TransmissionOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> FuelTypeOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> VehicleClassOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> CarStatusOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> ModelOptions { get; set; } = [];

    public bool IsEditMode => Car.CarId.HasValue;
}
