using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminVehicleClassIndexVm
{
    [Display(Name = "Vehicle Class")]
    [Required(ErrorMessage = "Vehicle class is required.")]
    [StringLength(255)]
    public string VehicleClassName { get; set; } = string.Empty;

    public IReadOnlyList<AdminVehicleClassListItemVm> VehicleClasses { get; set; } = [];

    public string? ReturnUrl { get; set; }
}
