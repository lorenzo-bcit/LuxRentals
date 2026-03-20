using LuxRentals.Data;
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
        private readonly LuxRentalsDbContext _db;
        private readonly ILogger<BookingController> _logger;

        public BookingController(BookingRepo bookingRepo, IPaymentService paymentService, LuxRentalsDbContext db, ILogger<BookingController> logger)
        {
            _bookingRepo = bookingRepo;
            _paymentService = paymentService;
            _db = db;
            _logger = logger;
        }


        // Shows booking creation form
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public IActionResult Create(int carId)
        {
            ViewBag.CarId = carId;

            return View(new BookingCreateViewModel());
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(int carId, BookingCreateViewModel model)
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

                // Validate car exists
                var car = _db.Cars.FirstOrDefault(c => c.PkCarId == carId);
                if (car == null)
                {
                    TempData["Error"] = "Selected car does not exist.";
                    return RedirectToAction("Index", "Home");
                }

                // Calculate price
                var price = _bookingRepo.CalculateBookingPrice(carId, model.StartDateTime, model.EndDateTime);

                // Check if booking is possible
                var canOrder = _bookingRepo.CheckBooking(customerId, model.StartDateTime, model.EndDateTime, carId);
                if (!canOrder)
                {
                    TempData["PaymentError"] = "Car is unavailable or you have a conflicting booking.";
                    return RedirectToAction("Create", "Booking", new { carId });
                }

                // Store session info (optional, fallback for Checkout)
                HttpContext.Session.SetInt32("CarId", carId);
                HttpContext.Session.SetInt32("CustomerId", customerId);
                HttpContext.Session.SetString("StartDate", model.StartDateTime.ToString("o")); // ISO format
                HttpContext.Session.SetString("EndDate", model.EndDateTime.ToString("o"));

                // Create PayPal order
                var orderId = await _paymentService.CreateOrderAsync(price, "CAD");

                // Redirect to Checkout, pass carId as query param
                return RedirectToAction("Checkout", "Payment", new { orderId, carId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create booking");
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

        // Admin/Employee can see list of all customers with bookings
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public IActionResult CustomerList()
        {
            try
            {
                var customers = _bookingRepo.GetAllCustomersWithBookings();

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

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("MyBookings");
                }

                bool isAdminOrEmployee = User.IsInRole("Admin") || User.IsInRole("Employee");

                // Verify Customer owns Booking (unless Admin/Employee)
                if (!isAdminOrEmployee)
                {
                    int customerId = GetCustomerId();
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
            // Get email of logged-in user
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.Identity.Name; 

                if (!string.IsNullOrEmpty(email))
                {
                    return _bookingRepo.GetCustomerIdByEmail(email);
                }
            }

            return 0;
        }
    }
}
