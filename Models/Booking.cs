using System;
using System.Collections.Generic;

namespace LuxRentals.Models;

public partial class Booking
{
    public int PkBookingId { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int FkBookingStatusId { get; set; }

    public int FkCarId { get; set; }

    public int FkCustomerId { get; set; }

    public virtual BookingStatus FkBookingStatus { get; set; } = null!;

    public virtual Car FkCar { get; set; } = null!;

    public virtual Customer FkCustomer { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
