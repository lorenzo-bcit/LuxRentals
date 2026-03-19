using LuxRentals.Data;
using LuxRentals.Models;
using LuxRentals.Utils;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Repositories.Bookings
{
    public class BookingRepo
    {
        private readonly LuxRentalsDbContext _context;
        private readonly ILogger<BookingRepo> _logger;

        // Booking Status IDs
        private const int STATUS_UNBOOKED = 1;
        private const int STATUS_BOOKED = 2;
        private const int STATUS_CANCELLED = 3;
        private const string CAR_STATUS_AVAILABLE = "Available";

        public BookingRepo(LuxRentalsDbContext context, ILogger<BookingRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Create booking
        public async Task<Booking> CreateBooking(int carId, int customerId,
            DateTime startDate, DateTime endDate, string transactionId)
        {
            // Normalize to midnight UTC
            startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc);

            var tomorrow = BookingClock.Tomorrow();

            // Validation checks (date-only comparison)
            if (endDate <= startDate)
            {
                throw new ArgumentException("End date must be after start date.");
            }

            if (startDate.Date < tomorrow)
            {
                throw new ArgumentException("Start date must be at least one day in the future.");
            }

            bool isCarAvailable = await IsCarAvailable(carId, startDate, endDate);
            if (!isCarAvailable)
            {
                throw new InvalidOperationException("The car is not available for the selected dates.");
            }

            bool hasConflictingBooking = await HasConflictingBooking(customerId, startDate, endDate);
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
                CancelledAt = null,
                TransactionId = transactionId
            };

            _logger.LogInformation(
                "Creating booking for Car ID {CarId} from {StartDate} to {EndDate} for Customer ID {CustomerId}",
                carId, startDate, endDate, customerId);

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        // Cancel Booking
        public async Task CancelBooking(int bookingId, int customerId, bool isAdminOrEmployee)
        {
            var booking = await GetBookingById(bookingId);
            if (booking == null)
            {
                throw new ArgumentException("Booking not found.");
            }

            if (booking.FkCustomerId != customerId && !isAdminOrEmployee)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this booking.");
            }

            if (!CanCancelBooking(booking, isAdminOrEmployee))
            {
                throw new InvalidOperationException("This booking cannot be cancelled. Cancellations must be made at least 2 days before the pickup date.");
            }

            booking.CancelledAt = DateTime.UtcNow;
            booking.FkBookingStatusId = STATUS_CANCELLED;

            await _context.SaveChangesAsync();
        }

        // Get booking by ID
        public async Task<Booking?> GetBookingById(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.FkBookingStatus)
                .Include(b => b.FkCar)
                    .ThenInclude(c => c.FkModel)
                        .ThenInclude(m => m.FkMake)
                .Include(b => b.FkCustomer)
                .FirstOrDefaultAsync(b => b.PkBookingId == bookingId);
        }

        // Get customer ID by email
        public async Task<int> GetCustomerIdByEmail(string email)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            return customer?.PkCustomerId ?? 0;
        }

        // Get all bookings for a customer
        public async Task<List<Booking>> GetBookingsForCustomer(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.FkBookingStatus)
                .Include(b => b.FkCar)
                    .ThenInclude(c => c.FkModel)
                        .ThenInclude(m => m.FkMake)
                .Where(b => b.FkCustomerId == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        // Get all customers who have made bookings
        public async Task<List<Customer>> GetAllCustomersWithBookings()
        {
            return await _context.Customers
                .Include(c => c.Bookings)
                .Where(c => c.Bookings.Any())
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
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

            var pickupDate = booking.StartDateTime.Date;
            return pickupDate > BookingClock.Today().AddDays(1);
        }

        // Check if car is available for date range
        private async Task<bool> IsCarAvailable(int carId, DateTime startDate, DateTime endDate)
        {
            var overlappingBookingExists = await _context.Bookings.AnyAsync(b =>
                b.FkCarId == carId &&
                b.CancelledAt == null &&
                b.FkBookingStatusId == STATUS_BOOKED &&
                startDate < b.EndDateTime &&
                endDate > b.StartDateTime
            );

            return !overlappingBookingExists;
        }

        // Check if customer has conflicting booking
        private async Task<bool> HasConflictingBooking(int customerId, DateTime startDate, DateTime endDate)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.FkCustomerId == customerId &&
                b.CancelledAt == null &&
                b.FkBookingStatusId == STATUS_BOOKED &&
                startDate < b.EndDateTime &&
                endDate > b.StartDateTime
            );
        }

        // Calculate booking price
        public async Task<decimal> CalculateBookingPrice(int carId, DateTime start, DateTime end)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.PkCarId == carId);

            if (car == null)
                throw new Exception("Car not found.");

            int days = (end.Date - start.Date).Days;

            if (days <= 0)
                days = 1;

            return car.DailyRate * days;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}