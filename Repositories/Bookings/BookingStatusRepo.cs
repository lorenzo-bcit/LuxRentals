using LuxRentals.Data;
using LuxRentals.Models;

namespace LuxRentals.Repositories.BookingStatus
{
    public class BookingStatusRepo
    {
        private readonly LuxRentalsDbContext _context;

        public BookingStatusRepo(LuxRentalsDbContext context)
        {
            _context = context;
        }

        public void SetBookingStatus(Booking booking, string statusName)
        {
            var statusId = GetStatusIdByName(statusName);
            booking.FkBookingStatusId = statusId;
        }

        public int GetStatusIdByName(string statusName)
        {
            var status = _context.BookingStatuses
                .FirstOrDefault(s => s.BookingStatus1 == statusName);

            if (status == null)
                throw new Exception($"Booking status '{statusName}' not found.");

            return status.PkBookingStatusId;
        }
    }
}