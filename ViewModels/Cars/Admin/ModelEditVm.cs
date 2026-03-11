using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuxRentals.ViewModels.Cars.Admin;

public class ModelEditVm
{
    public int ModelId { get; set; }

    [Display(Name = "Make")]
    [Required(ErrorMessage = "Make is required.")]
    public int? FkMakeId { get; set; }

    [Display(Name = "Model Name")]
    [Required(ErrorMessage = "Model name is required.")]
    [StringLength(255)]
    public string ModelName { get; set; } = string.Empty;

    public IReadOnlyList<SelectListItem> MakeOptions { get; set; } = [];

    public string? ReturnUrl { get; set; }
}
