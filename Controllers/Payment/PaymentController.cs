using LuxRentals.Data;
using LuxRentals.Repositories.Bookings;
using LuxRentals.Repositories.BookingStatus;
using LuxRentals.Services.Payment;
using LuxRentals.ViewModels.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            // Try session fallback if carId not in query
            if (carId == null)
                carId = HttpContext.Session.GetInt32("CarId");

            if (carId == null)
            {
                TempData["Error"] = "Booking session expired.";
                return RedirectToAction("Create", "Booking");
            }

            var car = _db.Cars.FirstOrDefault(c => c.PkCarId == carId.Value);
            if (car == null)
            {
                TempData["Error"] = "Selected car does not exist.";
                return RedirectToAction("Index", "Home");
            }

            string startStr = HttpContext.Session.GetString("StartDate");
            string endStr = HttpContext.Session.GetString("EndDate");

            // Parse dates with RoundtripKind to preserve UTC
            if (!DateTime.TryParse(startDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime startDate) ||
                !DateTime.TryParse(endDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime endDate))
            {
                TempData["Error"] = "Invalid booking dates.";
                return RedirectToAction("Create", "Booking");
            }

            var price = await _bookingRepo.CalculateBookingPriceAsync(carId.Value, startDate, endDate);

            ViewBag.Price = price;
            ViewBag.CarId = carId.Value;
            ViewBag.OrderId = orderId;
            ViewBag.StartDate = startDate.ToShortDateString();
            ViewBag.EndDate = endDate.ToShortDateString();
            ViewBag.PayPalClientId = _paypalOptions.ClientId;
            ViewBag.StartDate = startDate.ToString("MMM dd, yyyy");
            ViewBag.EndDate = endDate.ToString("MMM dd, yyyy");

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request body is missing." });
            }

            try
            {
                var orderId = await _paymentService.CreateOrderAsync(request.Amount, "CAD");

                // Store pending info
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

                // Retrieve booking info from session
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
                        message = "Booking session expired. Please try again.",
                        redirectUrl = Url.Action("Create", "Booking")
                    });
                }

                if (!DateTime.TryParse(startStr, out DateTime startDate) ||
                    !DateTime.TryParse(endStr, out DateTime endDate))
                {
                    return Json(new { success = false, message = "Invalid booking dates." });
                }

                // Prevent duplicate booking
                var existingBooking = _db.Bookings.FirstOrDefault(b => b.TransactionId == captureId);
                if (existingBooking != null)
                    return Json(new { success = true, redirectUrl = Url.Action("MyBookings", "Booking") });

                // Validate payment amount
                var expectedAmount = await _bookingRepo.CalculateBookingPriceAsync(carId.Value, startDate, endDate);
                if (amountPaid != expectedAmount)
                {
                    _logger.LogWarning("Payment mismatch: expected {expected}, got {actual}", expectedAmount, amountPaid);
                    return Json(new { success = false, message = "Payment verification failed." });
                }

                // FIXED: Parse dates with RoundtripKind to preserve UTC from session
                DateTime startDate = DateTime.Parse(
                    HttpContext.Session.GetString("StartDate"),
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind);

                DateTime endDate = DateTime.Parse(
                    HttpContext.Session.GetString("EndDate"),
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind);

                var booking = await _bookingRepo.CreateBooking(carId, customerId, startDate, endDate, request.OrderId);

                HttpContext.Session.Clear();

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
        private int GetCustomerId()
        {
            return HttpContext.Session.GetInt32("CustomerId") ?? 0;
        }
    }
}