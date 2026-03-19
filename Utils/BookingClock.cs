namespace LuxRentals.Utils;

public static class BookingClock
{
    private static readonly TimeZoneInfo BookingTimeZone = ResolveBookingTimeZone();

    public static DateTime Today() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BookingTimeZone).Date;

    public static DateTime Tomorrow() => Today().AddDays(1);

    private static TimeZoneInfo ResolveBookingTimeZone()
    {
        foreach (var timeZoneId in new[] { "America/Vancouver", "Pacific Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}