using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Roles
{
    public class ProfileVM
    {
        [Key]
        [HiddenInput]
        public int PkCustomerId { get; set; }

        [Required]
        [HiddenInput]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Driver licence number is required.")]
        [StringLength(30, ErrorMessage = "Driver licence number cannot exceed 30 characters.")]
        [Display(Name = "Driver Licence Number")]
        public string DriverLicenceNo { get; set; } = null!;
    }
}