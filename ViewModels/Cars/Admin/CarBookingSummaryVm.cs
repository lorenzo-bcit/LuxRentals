using LuxRentals.Models;

namespace LuxRentals.ViewModels.Cars.Admin;

public class CarBookingSummaryVm
{
    public int BookingId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public static CarBookingSummaryVm FromEntity(Booking booking, DateTime utcNow) => new()
    {
        BookingId = booking.PkBookingId,
        CustomerId = booking.FkCustomerId,
        CustomerName = $"{booking.FkCustomer.FirstName} {booking.FkCustomer.LastName}".Trim(),
        StartDate = booking.StartDateTime,
        EndDate = booking.EndDateTime,
        IsActive = booking.StartDateTime <= utcNow && booking.EndDateTime > utcNow
    };
}
