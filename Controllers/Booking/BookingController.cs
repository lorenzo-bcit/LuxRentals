using LuxRentals.Repositories.Bookings;
using LuxRentals.ViewModels.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers.Booking
{
    public class BookingController : Controller
    {
        private readonly BookingRepo _bookingRepo;

        public BookingController(BookingRepo bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }


        // Shows booking creation form
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Create(int carId)
        {
            ViewBag.CarId = carId;

            return View(new BookingCreateViewModel());
        }

        // Creates the booking
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int carId, BookingCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CarId = carId;
                return View(model);
            }

            try
            {

                int customerId = GetCustomerId();

                if (customerId == 0)
                {
                    TempData["Error"] = "You must be logged in to make a booking.";
                    return RedirectToAction("Login", "Account");
                }

                _bookingRepo.CreateBooking(
                    carId,
                    customerId,
                    model.StartDateTime.ToUniversalTime(),
                    model.EndDateTime.ToUniversalTime()
                    );

                TempData["Success"] = "Booking created successfully!";

                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.CarId = carId;
                return View(model);
            }
        }

        // Allows customer to see their OWN bookings
        [HttpGet]
        public IActionResult MyBookings()
        {
            try
            {
                int customerId = GetCustomerId();

                
               if (customerId == 0)
                {
                    TempData["Error"] = "You must be logged in to view bookings.";
                    return RedirectToAction("Login", "Account");
                }

                var bookings = _bookingRepo.GetBookingsForCustomer(customerId);
                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load your bookings.";
                return View(new List<Models.Booking>());
            }
        }

        // TODO: Need to verify this works after roles are implemented
        // Admin/Employee can view any customer's booking history
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult ViewCustomerBookings(int customerId)
        {
            try
            {
                var bookings = _bookingRepo.GetBookingsForCustomer(customerId);
                ViewBag.CustomerId = customerId;
                return View("MyBookings", bookings);

            } catch (Exception ex) {

                TempData["Error"] = "Unable to load customer bookings.";
                return View("MyBookings", new List<Models.Booking>());
            }
        }


        // Show cancellation info (no validation necessary)
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            try
            {
                var booking = _bookingRepo.GetBookingById(id);
                int customerId = GetCustomerId();
                bool isAdminOrEmployee = User.IsInRole("Admin") || User.IsInRole("Employee");


                var timeUntilStart = booking.StartDateTime - DateTime.UtcNow;
                var canCancel = (booking.CancelledAt == null) &&
                (isAdminOrEmployee || timeUntilStart.TotalHours >= 48);

                var viewModel = new BookingCancellationViewModel
                {
                    PkBookingId = id,
                    StartDateTime = booking.StartDateTime,
                    CanCancel = canCancel,
                    Message = canCancel
                        ? "Are you sure you want to cancel this booking?"
                        : booking.CancelledAt != null
                            ? "This booking has already been cancelled."
                            : "Cannot cancel within 48 hours of start date."
                };

                return View(viewModel);
            } catch (Exception ex)
            {
                TempData["Error"] = "Unable to load cancellation page.";
                return RedirectToAction("MyBookings");

            }
        }

        // Cancels booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelConfirmed(int bookingId)
        {
            try
            {
                int customerId = GetCustomerId();
                bool isAdminOrEmployee = User.IsInRole("Admin") || User.IsInRole("Employee");

                var booking = _bookingRepo.GetBookingById(bookingId);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("MyBookings");
                }

                int bookingCustomerId = booking.FkCustomerId;

                _bookingRepo.CancelBooking(bookingId, customerId, isAdminOrEmployee);

                TempData["Success"] = "Booking cancelled successfully.";

                if (isAdminOrEmployee)
                {
                    return RedirectToAction("ViewCustomerBookings", customerId);
                } else
                {
                    return RedirectToAction("MyBookings");
                }

            } catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Cancel");
            }
        }


        // Helper Methods
        private int GetCustomerId()
        {
            return 2;

            // TODO: Refactor this back in when Customer roles is working again.
            var customerIdClaim = User.Claims
                .FirstOrDefault(c => c.Type == "CustomerId");

            if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
            {
                // TODO: Redirect to login page
                return 0;
            }

            return customerId;
        }
    }
}
