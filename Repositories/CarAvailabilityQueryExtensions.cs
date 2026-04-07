using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories;

public static class CarAvailabilityQueryExtensions
{
    // Public "bookable" availability is stricter than "no overlapping booking". The car must also
    // currently be in the Available status, and the booking overlap test uses a half-open window:
    // [start, end), so adjacent bookings that touch at the boundary are allowed.
    public static IQueryable<Car> WhereBookableForWindow(
        this IQueryable<Car> cars,
        IQueryable<Booking> bookings,
        DateTime? startDate,
        DateTime? endDate)
    {
        if (!startDate.HasValue || !endDate.HasValue || endDate.Value.Date <= startDate.Value.Date)
            return cars.Where(_ => false);

        var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);

        return cars
            .Where(c => c.FkCarStatus.StatusFlag == CarStatusNames.AVAILABLE)
            .Where(c =>
                !bookings.Any(b =>
                    b.FkCarId == c.PkCarId &&
                    b.CancelledAt == null &&
                    b.FkBookingStatusId == BookingStatusIds.BOOKED &&
                    b.StartDateTime < end &&
                    b.EndDateTime > start));
    }

    public static Task<bool> IsBookableForWindowAsync(
        this IQueryable<Car> cars,
        IQueryable<Booking> bookings,
        int carId,
        DateTime? startDate,
        DateTime? endDate)
    {
        return cars
            .Where(c => c.PkCarId == carId)
            .WhereBookableForWindow(bookings, startDate, endDate)
            .AnyAsync();
    }
}
