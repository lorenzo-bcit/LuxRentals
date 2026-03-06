using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Profile;

public class EditIdentityProfileViewModel
{
    // Display-only
    public string UserName { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Phone]
    [RegularExpression(@"^(\+1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}$",
        ErrorMessage = "Please enter a valid Canadian phone number.")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
}