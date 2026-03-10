using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Cars.Admin;

public class AdminMakeEditVm
{
    public int MakeId { get; set; }

    [Display(Name = "Make Name")]
    [Required(ErrorMessage = "Make name is required.")]
    [StringLength(255)]
    public string MakeName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
