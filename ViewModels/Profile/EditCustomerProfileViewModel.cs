using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Profile;

public class EditCustomerProfileViewModel
{
    [Required]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 40 characters.")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$",
        ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = "";

    [Required]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 40 characters.")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$",
        ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = "";

    [Required]
    [Phone]
    [RegularExpression(@"^(\+1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}$",
        ErrorMessage = "Please enter a valid Canadian phone number.")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = "";

    // Read-only fields shown on the edit page (not editable).
    public string Email { get; set; } = "";
    public string DriverLicenceNo { get; set; } = "";
    public bool? LicenceVerified { get; set; }
}