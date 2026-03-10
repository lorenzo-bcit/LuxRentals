using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminVehicleClassEditVm
{
    public int VehicleClassId { get; set; }

    [Display(Name = "Vehicle Class")]
    [Required(ErrorMessage = "Vehicle class is required.")]
    [StringLength(255)]
    public string VehicleClassName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
