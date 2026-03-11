using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Cars.Admin;

public class MakeIndexVm
{
    [Display(Name = "Make Name")]
    [Required(ErrorMessage = "Make name is required.")]
    [StringLength(255)]
    public string MakeName { get; set; } = string.Empty;

    public IReadOnlyList<MakeListItemVm> Makes { get; set; } = [];

    public string? ReturnUrl { get; set; }
}
