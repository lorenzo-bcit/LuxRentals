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
        [StringLength(40, ErrorMessage = "First name cannot exceed 40 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(40, ErrorMessage = "Last name cannot exceed 40 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Please enter a valid phone number (10-15 digits).")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Driver licence number is required.")]
        [StringLength(10, ErrorMessage = "Driver licence number cannot exceed 10 characters.")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Driver licence number can only contain uppercase letters and numbers.")]
        [Display(Name = "Driver Licence Number")]
        public string DriverLicenceNo { get; set; } = null!;
    }
}