using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Bookings
{
    public class CustomerListViewModel
    {
        [Required]
        [Display(Name = "Customer Id")]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Number of Bookings")]
        public int TotalBookings { get; set; }

        [Required]
        [Display(Name = "Number of Active Bookings")]
        public int ActiveBookings { get; set; }
    }
}