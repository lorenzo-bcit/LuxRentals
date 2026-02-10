using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.BookingViewModels
{
    public class BookingCancellationViewModel
    {

        private int PkBookingId { get; set; }

        // TODO: Add more details to the view model if needed, such as car details etc

        [Display(Name = "Pickup Date")]
        private DateTime StartDateTime { get; set; }

        [Display(Name = "Return Date")]
        private DateTime EndDateTime { get; set; }

        [Display(Name = "Booking Status")]
        private bool BookingStatus { get; set; }


        public bool CanCancel { get; set; }

        public string? Message { get; set; }
    }
}
