using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories;

public static class CarAvailabilityQueryExtensions
{
    public static IQueryable<Car> WhereBookableForWindow(
        this IQueryable<Car> cars,
        IQueryable<Booking> bookings,
        DateTime startDate,
        DateTime endDate)
    {
        var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

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
        DateTime startDate,
        DateTime endDate)
    {
        return cars
            .Where(c => c.PkCarId == carId)
            .WhereBookableForWindow(bookings, startDate, endDate)
            .AnyAsync();
    }
}
