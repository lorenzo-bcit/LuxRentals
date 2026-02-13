using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repos
{
    public class BookingRepo
    {
        private readonly LuxRentalsDbContext _context;

        // Booking Status IDs
        private const int STATUS_UNBOOKED = 1;
        private const int STATUS_BOOKED = 2;
        private const int STATUS_CANCELLED = 3;

        public BookingRepo(LuxRentalsDbContext context)
        {
            _context = context;
        }

        // Create booking
        public void CreateBooking(int carId, int customerId,
            DateTime startDate, DateTime endDate)
        {
            try
            {
                // Validation checks
                if (endDate <= startDate)
                {
                    throw new ArgumentException("End date must be after start date.");
                }

                if (startDate <= DateTime.Now)
                {
                    throw new ArgumentException("Start date must be in the future.");
                }

                bool isCarAvailable = IsCarAvailable(carId, startDate, endDate);

                if (!isCarAvailable)
                {
                    throw new InvalidOperationException("The car is not available for the selected dates.");
                }

                bool hasConflictingBooking = HasConflictingBooking(customerId, startDate, endDate);

                if (hasConflictingBooking)
                {
                    throw new InvalidOperationException("You have another booking that conflicts with the selected dates.");
                }

                var booking = new Booking
                {
                    FkCarId = carId,
                    FkCustomerId = customerId,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    CreatedAt = DateTime.UtcNow,
                    FkBookingStatusId = STATUS_BOOKED,
                    CancelledAt = null
                };

                _context.Bookings.Add(booking);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        // Cancel Booking
        public void CancelBooking(
            int bookingId,
            int customerId,
            bool isAdminOrEmployee)
        {
            try
            {
                var booking = GetBookingById(bookingId);
                if (booking == null)
                {
                    throw new ArgumentException("Booking not found.");
                }

                // Check authorization
                // TODO: Customer should not be able to see other customer bookings.
                if (booking.FkCustomerId != customerId && !isAdminOrEmployee)
                {
                    throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");
                }

                if (!CanCancelBooking(booking, isAdminOrEmployee))
                {
                    throw new InvalidOperationException("This booking cannot be cancelled. Cancellations must be made at least 48 hours before the start time.");
                }

                booking.CancelledAt = DateTime.UtcNow;
                booking.FkBookingStatusId = STATUS_CANCELLED;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        // Get booking by ID
        public Booking? GetBookingById(int bookingId)
        {
            return _context.Bookings
                .Include(b => b.FkBookingStatus)
                .Include(b => b.FkCar)
                    .ThenInclude(c => c.FkModel)
                        .ThenInclude(m => m.FkMake)
                .Include(b => b.FkCustomer)
                .FirstOrDefault(b => b.PkBookingId == bookingId);
        }

        // Get all bookings for a customer
        public List<Booking> GetBookingsForCustomer(int customerId)
        {
            return _context.Bookings
                .Include(b => b.FkBookingStatus)
                .Include(b => b.FkCar)
                    .ThenInclude(c => c.FkModel)
                        .ThenInclude(m => m.FkMake)
                .Where(b => b.FkCustomerId == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
        }

        // Helper Methods

        // Can booking be cancelled?
        public bool CanCancelBooking(Booking booking, bool isAdminOrEmployee)
        {
            if (booking.CancelledAt != null)
            {
                return false;
            }

            if (isAdminOrEmployee)
            {
                return true;
            }

            var timeUntilStart = booking.StartDateTime - DateTime.UtcNow;
            return timeUntilStart.TotalHours >= 48;
        }

        // Check if car is available for date range
        private bool IsCarAvailable(int carId, DateTime startDate, DateTime endDate)
        {
            // TODO: Check if Car if is in service
            return !_context.Bookings.Any(b =>
                b.FkCarId == carId &&
                b.CancelledAt == null &&
                b.FkBookingStatusId == STATUS_BOOKED &&
                ((startDate >= b.StartDateTime && startDate < b.EndDateTime) ||
                 (endDate > b.StartDateTime && endDate <= b.EndDateTime) ||
                 (startDate <= b.StartDateTime && endDate >= b.EndDateTime)));
        }

        // Check if customer has conflicting booking
        private bool HasConflictingBooking(int customerId, DateTime startDate, DateTime endDate)
        {
            return _context.Bookings.Any(b =>
                b.FkCustomerId == customerId &&
                b.CancelledAt == null &&
                b.FkBookingStatusId == STATUS_BOOKED &&
                ((startDate >= b.StartDateTime && startDate < b.EndDateTime) ||
                 (endDate > b.StartDateTime && endDate <= b.EndDateTime) ||
                 (startDate <= b.StartDateTime && endDate >= b.EndDateTime)));
        }
    }
}