namespace LuxRentals.Models;

public class BookingStatus
{
    public int PkBookingStatusId { get; set; }

    public string BookingStatus1 { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
