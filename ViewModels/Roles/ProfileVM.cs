using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Roles
{
    public class ProfileVm
    {
        [Key]
        [HiddenInput]
        public int PkCustomerId { get; set; }

        [Required]
        [HiddenInput]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 40 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 40 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [RegularExpression(@"^(\+1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}$",
            ErrorMessage = "Please enter a valid Canadian phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Driver licence number is required.")]
        [StringLength(20, MinimumLength = 7,
            ErrorMessage = "Driver's license number must be between 7 and 20 characters.")]
        [Display(Name = "Driver Licence Number")]
        public string DriverLicenceNo { get; set; } = null!;
    }
}