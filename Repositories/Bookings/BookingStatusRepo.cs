using LuxRentals.Data;

namespace LuxRentals.Repositories.BookingStatus
{
    public class BookingStatusRepo
    {
        private readonly LuxRentalsDbContext _context;

        public BookingStatusRepo(LuxRentalsDbContext context)
        {
            _context = context;
        }

        public int GetStatusIdByName(string statusName)
        {
            var status = _context.BookingStatuses
                .FirstOrDefault(s => s.BookingStatus1 == statusName);

            if (status == null)
                throw new Exception($"Booking status '{statusName}' not found.");

            return status.PkBookingStatusId;
        }

        public void UpdateBookingStatus(int bookingId, string statusName)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.PkBookingId == bookingId);

            if (booking == null)
                throw new Exception("Booking not found.");

            var statusId = GetStatusIdByName(statusName);

            booking.FkBookingStatusId = statusId;

        }
    }
}