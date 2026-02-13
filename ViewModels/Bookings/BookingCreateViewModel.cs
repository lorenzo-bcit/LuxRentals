using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.BookingViewModels
{
    public class BookingCreateViewModel
    {
        [Required]
        [Display(Name = "Pickup Date")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "Return Date")]
        public DateTime EndDateTime { get; set; }


    }
}
