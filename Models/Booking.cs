namespace LuxRentals.Models;

public class Booking
{
    public int PkBookingId { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int FkBookingStatusId { get; set; }

    public int FkCarId { get; set; }

    public int FkCustomerId { get; set; }

    public BookingStatus FkBookingStatus { get; set; } = null!;

    public Car FkCar { get; set; } = null!;

    public Customer FkCustomer { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
