using LuxRentals.Data;
using LuxRentals.Models;
using LuxRentals.Repositories.Bookings;
using LuxRentals.Repositories.BookingStatus;
using LuxRentals.Services.Payment;
using LuxRentals.ViewModels.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace LuxRentals.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly BookingRepo _bookingRepo;
        private readonly LuxRentalsDbContext _db;
        private readonly PaypalOptions _paypalOptions;
        private readonly ILogger<PaymentController> _logger;
        private readonly BookingStatusRepo _bookingStatusRepo;

        public PaymentController(
            IPaymentService paymentService,
            BookingRepo bookingRepo,
            LuxRentalsDbContext db,
            IOptions<PaypalOptions> paypalOptions,
            ILogger<PaymentController> logger,
            BookingStatusRepo bookingStatusRepo)
        {
            _paymentService = paymentService;
            _bookingRepo = bookingRepo;
            _db = db;
            _paypalOptions = paypalOptions.Value;
            _logger = logger;
            _bookingStatusRepo = bookingStatusRepo;
        }

        [Authorize]
        public async Task<IActionResult> Checkout(string orderId, int? carId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                TempData["Error"] = "Invalid order.";
                return RedirectToAction("Index", "Home");
            }

            // Use session fallback if carId missing
            carId ??= HttpContext.Session.GetInt32("CarId");
            if (carId == null)
            {
                TempData["Error"] = "Booking session expired.";
                return RedirectToAction("Create", "Booking");
            }

            // Load car async
            var car = await _bookingRepo.CarExists((int)carId);
            if (!car)
            {
                TempData["Error"] = "Selected car does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // Parse dates from session
            string startStr = HttpContext.Session.GetString("StartDate");
            string endStr = HttpContext.Session.GetString("EndDate");

            if (!DateTime.TryParse(startStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var startDate) ||
                !DateTime.TryParse(endStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var endDate))
            {
                TempData["Error"] = "Invalid booking dates.";
                return RedirectToAction("Create", "Booking");
            }

            var price = await _bookingRepo.CalculateBookingPriceAsync(carId.Value, startDate, endDate);

            ViewBag.OrderId = orderId;
            ViewBag.CarId = carId.Value;
            ViewBag.Price = price;
            ViewBag.StartDate = startDate.ToString("MMM dd, yyyy");
            ViewBag.EndDate = endDate.ToString("MMM dd, yyyy");
            ViewBag.PayPalClientId = _paypalOptions.ClientId;

            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is missing." });

            try
            {
                int customerId = await GetCustomerId();

                if (customerId == 0)
                {
                    return Json(new
                    {
                        error = "You must be logged in.",
                        redirectUrl = Url.Action("Login", "Account")
                    });
                }

                var startStr = HttpContext.Session.GetString("StartDate");
                var endStr = HttpContext.Session.GetString("EndDate");

                if (string.IsNullOrEmpty(startStr) || string.IsNullOrEmpty(endStr))
                {
                    return Json(new { error = "Session expired. Please restart booking." });
                }

                if (!DateTime.TryParse(startStr, out var startDate) ||
                    !DateTime.TryParse(endStr, out var endDate))
                {
                    return Json(new { error = "Invalid booking dates." });
                }

                var car = await _db.Cars.FindAsync(request.CarId);
                if (car == null)
                {
                    return Json(new { error = "Car does not exist." });
                }

                var (canOrder, errorMessage) =
                    await _bookingRepo.CheckBookingAsync(customerId, startDate, endDate, request.CarId);

                if (!canOrder)
                {
                    return Json(new
                    {
                        error = errorMessage,
                        redirectUrl = Url.Action("Create", "Booking", new { carId = request.CarId })
                    });
                }

                var amount = await _bookingRepo.CalculateBookingPriceAsync(
                    request.CarId, startDate, endDate);

                var orderId = await _paymentService.CreateOrderAsync(amount, "CAD");

                HttpContext.Session.SetInt32("CarId", request.CarId);
                HttpContext.Session.SetString("PendingOrderId", orderId);

                return Json(new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayPal order");
                return StatusCode(500, new { error = "Failed to create PayPal order" });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capture([FromBody] CaptureRequest request)
        {
            if (string.IsNullOrEmpty(request?.OrderId))
                return Json(new { success = false, message = "Invalid order." });

            try
            {

                var (captureId, amountPaid) = await _paymentService.CaptureOrderAsync(request.OrderId);

                if (captureId == null)
                    return Json(new { success = false, message = "Payment failed." });

                int? carId = HttpContext.Session.GetInt32("CarId");
                int? customerId = HttpContext.Session.GetInt32("CustomerId");
                string startStr = HttpContext.Session.GetString("StartDate");
                string endStr = HttpContext.Session.GetString("EndDate");

                if (carId == null || customerId == null ||
                    string.IsNullOrEmpty(startStr) || string.IsNullOrEmpty(endStr))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Booking session expired.",
                        redirectUrl = Url.Action("Create", "Booking")
                    });
                }

                DateTime startDateUtc = DateTime.Parse(startStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                DateTime endDateUtc = DateTime.Parse(endStr, null, System.Globalization.DateTimeStyles.RoundtripKind);

                var existingBooking = _db.Bookings.FirstOrDefault(b => b.TransactionId == captureId);
                if (existingBooking != null)
                    return Json(new { success = true, redirectUrl = Url.Action("MyBookings", "Booking") });

                var expectedAmount = await _bookingRepo.CalculateBookingPriceAsync(carId.Value, startDateUtc, endDateUtc);

                if (amountPaid != expectedAmount)
                {
                    _logger.LogWarning("Payment mismatch: expected {expected}, got {actual}", expectedAmount, amountPaid);
                    return Json(new { success = false, message = "Payment verification failed." });
                }

                var (canStillBook, _) =
                    await _bookingRepo.CheckBookingAsync(customerId.Value, startDateUtc, endDateUtc, carId.Value);

                if (!canStillBook)
                {
                    return Json(new { success = false, message = "Car is no longer available." });
                }

                var booking = await _bookingRepo.CreateBooking(
                    carId.Value,
                    customerId.Value,
                    startDateUtc,
                    endDateUtc,
                    captureId
                );

                HttpContext.Session.Remove("CarId");
                HttpContext.Session.Remove("StartDate");
                HttpContext.Session.Remove("EndDate");
                HttpContext.Session.Remove("PendingOrderId");

                return Json(new { success = true, redirectUrl = Url.Action("MyBookings", "Booking") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayPal capture failed");
                return Json(new { success = false, message = "Payment capture error." });
            }
        }

        public class CreateOrderRequest
        {
            public int CarId { get; set; }
            public decimal Amount { get; set; }
        }

        public class CaptureRequest
        {
            [Required]
            public string OrderId { get; set; }
        }
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