using LuxRentals.Repositories.Bookings;
using LuxRentals.Services.Payment;
using LuxRentals.ViewModels.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers.Booking
{
    public class BookingController : Controller
    {
        private readonly BookingRepo _bookingRepo;
        private readonly IPaymentService _paymentService;

        public BookingController(BookingRepo bookingRepo, IPaymentService paymentService)
        {
            _bookingRepo = bookingRepo;
            _paymentService = paymentService;
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
        public async Task<IActionResult> Create(int carId, BookingCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CarId = carId;
                return View(model);
            }

            try
            {
                int customerId = await GetCustomerId();

                if (customerId == 0)
                {
                    TempData["Error"] = "You must be logged in to make a booking.";
                    return RedirectToAction("Login", "Account");
                }

                var price = await _bookingRepo.CalculateBookingPrice(
                    carId,
                    model.StartDateTime,
                    model.EndDateTime);

                HttpContext.Session.SetInt32("CarId", carId);
                HttpContext.Session.SetInt32("CustomerId", customerId);
                HttpContext.Session.SetString("StartDate", model.StartDateTime.ToString());
                HttpContext.Session.SetString("EndDate", model.EndDateTime.ToString());

                var orderId = await _paymentService.CreateOrderAsync(price, "CAD");

                return RedirectToAction(
                    "Checkout",
                    "Payment",
                    new
                    {
                        orderId = orderId
                    });
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
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                int customerId = await GetCustomerId();

                if (customerId == 0)
                {
                    TempData["Error"] = "You must be logged in to view bookings.";
                    return RedirectToAction("Login", "Account");
                }

                var bookings = await _bookingRepo.GetBookingsForCustomer(customerId);
                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load your bookings.";
                return View(new List<Models.Booking>());
            }
        }

        // Admin/Employee can see list of all customers with bookings
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> CustomerList()
        {
            try
            {
                var customers = await _bookingRepo.GetAllCustomersWithBookings();

                var viewModel = customers.Select(c => new CustomerListViewModel
                {
                    CustomerId = c.PkCustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    TotalBookings = c.Bookings.Count,
                    ActiveBookings = c.Bookings.Count(b =>
                        b.CancelledAt == null &&
                        b.StartDateTime <= DateTime.UtcNow &&
                        b.EndDateTime > DateTime.UtcNow)
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load customer list.";
                return View(new List<CustomerListViewModel>());
            }
        }


        // Admin/Employee can view any customer's booking history
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> ViewCustomerBookings(int customerId)
        {
            try
            {
                var bookings = await _bookingRepo.GetBookingsForCustomer(customerId);
                ViewBag.CustomerId = customerId;
                return View("MyBookings", bookings);

            }
            catch (Exception ex)
            {

                TempData["Error"] = "Unable to load customer bookings.";
                return View("MyBookings", new List<Models.Booking>());
            }
        }


        // Show cancellation info (no validation necessary)
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingById(id);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("MyBookings");
                }

                bool isAdminOrEmployee = User.IsInRole("Admin") || User.IsInRole("Employee");

                // Verify Customer owns Booking (unless Admin/Employee)
                if (!isAdminOrEmployee)
                {
                    int customerId = await GetCustomerId();
                    if (booking.FkCustomerId != customerId)
                    {
                        TempData["Error"] = "You are not authorized to view this booking.";
                        return RedirectToAction("MyBookings");
                    }
                }

                var canCancel = _bookingRepo.CanCancelBooking(booking, isAdminOrEmployee);

                // Build customer-specific message for admin
                string customerName = $"{booking.FkCustomer.FirstName} {booking.FkCustomer.LastName}";
                string message;

                if (canCancel)
                {
                    message = isAdminOrEmployee
                        ? $"Are you sure you want to cancel {customerName}'s booking?"
                        : "Are you sure you want to cancel this booking?";
                }
                else
                {
                    message = booking.CancelledAt != null
                        ? "This booking has already been cancelled."
                        : "Cannot cancel within 48 hours of start date.";
                }

                var viewModel = new BookingCancellationViewModel
                {
                    PkBookingId = id,
                    StartDateTime = booking.StartDateTime,
                    EndDateTime = booking.EndDateTime,
                    CanCancel = canCancel,
                    Message = message
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load cancellation page.";
                return RedirectToAction("MyBookings");
            }
        }

        // Cancels booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int bookingId)
        {
            try
            {
                int customerId = await GetCustomerId();
                bool isAdminOrEmployee = User.IsInRole("Admin") || User.IsInRole("Employee");

                var booking = await _bookingRepo.GetBookingById(bookingId);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("MyBookings");
                }

                int bookingCustomerId = booking.FkCustomerId;

                await _bookingRepo.CancelBooking(bookingId, customerId, isAdminOrEmployee);

                TempData["Success"] = "Booking cancelled successfully.";

                if (isAdminOrEmployee)
                {
                    return RedirectToAction("ViewCustomerBookings", customerId);
                }
                else
                {
                    return RedirectToAction("MyBookings");
                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Cancel");
            }
        }


        // Helper Methods
        private async Task<int> GetCustomerId()
        {
            // Get email of logged-in user
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.Identity.Name;

                if (!string.IsNullOrEmpty(email))
                {
                    return await _bookingRepo.GetCustomerIdByEmail(email);
                }
            }

            return 0;
        }
    }
}