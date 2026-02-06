namespace LuxRentals.Models;

public class Transaction
{
    public int PkTransactionId { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; }

    public int FkBookingId { get; set; }

    public virtual Booking FkBooking { get; set; } = null!;
}
