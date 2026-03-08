using System.ComponentModel.DataAnnotations;

namespace LuxRentals.ViewModels.Bookings
{
    public class BookingCancellationViewModel
    {

        public int PkBookingId { get; set; }

        [Display(Name = "Pickup Date")]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "Return Date")]
        public DateTime EndDateTime { get; set; }

        [Display(Name = "Booking Status")]
        public bool BookingStatus { get; set; }


        public bool CanCancel { get; set; }

        public string? Message { get; set; }
    }
}
