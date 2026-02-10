using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.BookingViewModels
{
    public class BookingCreateViewModel
    {
        [Required]
        [Display(Name = "Pickup Date")]
        private DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "Return Date")]
        private DateTime EndDateTime { get; set; }


    }
}
