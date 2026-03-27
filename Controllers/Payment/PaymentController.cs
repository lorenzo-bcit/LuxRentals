using Azure.Core;
using LuxRentals.Data;
using LuxRentals.Repositories.Bookings;
using LuxRentals.Services.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LuxRentals.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly BookingRepo _bookingRepo;
        private readonly PaypalOptions _paypalOptions;
        private readonly ILogger<IPaymentService> _logger;

        public PaymentController(
            IPaymentService paymentService,
            BookingRepo bookingRepo,
            IOptions<PaypalOptions> paypalOptions,
            ILogger<IPaymentService> logger)
        {
            _paymentService = paymentService;
            _bookingRepo = bookingRepo;
            _paypalOptions = paypalOptions.Value;
            _logger = logger;
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Checkout(string orderId)
        {
            try
            {
                var sessionOrderId = HttpContext.Session.GetString("OrderId");

                if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(sessionOrderId) || orderId != sessionOrderId)
                {
                    TempData["Error"] = "Invalid checkout session. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;
                int? carId = HttpContext.Session.GetInt32("CarId");
                string startDateStr = HttpContext.Session.GetString("StartDate");
                string endDateStr = HttpContext.Session.GetString("EndDate");

                if (customerId == 0)
                {
                    TempData["Error"] = "Session expired. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                if (carId == null || startDateStr == null || endDateStr == null)
                {
                    TempData["Error"] = "Booking session expired.";
                    return RedirectToAction("Index", "Home");
                }

                var clientId = _paypalOptions.ClientId;
                if (string.IsNullOrEmpty(clientId))
                {
                    _logger.LogError("PayPal ClientId missing.");

                    TempData["Error"] = "Payment system is currently unavailable.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.PayPalClientId = clientId;

                // Parse dates with RoundtripKind to preserve UTC
                if (!DateTime.TryParse(startDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime startDate) ||
                    !DateTime.TryParse(endDateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime endDate))
                {
                    TempData["Error"] = "Invalid booking dates.";
                    return RedirectToAction("Index", "Home");
                }

                var canBook = await _bookingRepo.CheckBooking(customerId, (int)carId, startDate, endDate);
                if (!canBook.Success)
                {
                    TempData["Error"] = canBook.Message;
                    return RedirectToAction("Index", "Home");
                }

                var pricestr = HttpContext.Session.GetString("Price");
                if (pricestr == null || !decimal.TryParse(pricestr, out decimal sessionPrice))
                {
                    TempData["Error"] = "Booking session expired. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                var price = await _bookingRepo.CalculateBookingPrice(carId.Value, startDate, endDate);
                if (Math.Abs(sessionPrice - price) > 0.01m)
                {
                    _logger.LogWarning("Checkout failed: OrderId mismatch or Price mismatch for customer {CustomerId}", customerId);
                    TempData["Error"] = "Price mismatch. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Price = sessionPrice;
                ViewBag.OrderId = orderId;
                ViewBag.StartDate = startDate.ToString("MMM dd, yyyy");
                ViewBag.EndDate = endDate.ToString("MMM dd, yyyy");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                TempData["Error"] = "An error occurred during checkout. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capture([FromBody] CaptureRequest request)
        {
            try
            {
                var sessionOrderId = HttpContext.Session.GetString("OrderId");

                if (string.IsNullOrEmpty(request?.OrderId) ||
                    string.IsNullOrEmpty(sessionOrderId) ||
                    request.OrderId != sessionOrderId)
                {
                    _logger.LogWarning("OrderId mismatch.");
                    return Json(new { success = false, message = "Invalid order." });
                }

                var carId = HttpContext.Session.GetInt32("CarId");
                var customerId = HttpContext.Session.GetInt32("CustomerId");

                if (!carId.HasValue || !customerId.HasValue)
                {
                    return Json(new { success = false, message = "Session expired." });
                }

                DateTime startDate = DateTime.Parse(
                    HttpContext.Session.GetString("StartDate")!,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind);

                DateTime endDate = DateTime.Parse(
                    HttpContext.Session.GetString("EndDate")!,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind);

                var validation = await _bookingRepo.CheckBooking(
                    customerId.Value,
                    carId.Value,
                    startDate,
                    endDate);

                if (!validation.Success)
                {
                    return Json(new { success = false, message = validation.Message });
                }

                var priceStr = HttpContext.Session.GetString("Price");
                if (priceStr == null || !decimal.TryParse(priceStr, out decimal sessionPrice))
                {
                    return Json(new { success = false, message = "Session expired." });
                }

                var actualPrice = await _bookingRepo.CalculateBookingPrice(carId.Value, startDate, endDate);

                if (Math.Abs(sessionPrice - actualPrice) > 0.01m)
                {
                    _logger.LogWarning("Price mismatch for OrderId {OrderId}", sessionOrderId);
                    return Json(new { success = false, message = "Price mismatch." });
                }

                bool paymentSuccess = await _paymentService.CaptureOrderAsync(sessionOrderId);

                if (!paymentSuccess)
                {
                    return Json(new { success = false, message = "Payment failed." });
                }

                await _bookingRepo.CreateBooking(
                    carId.Value,
                    customerId.Value,
                    startDate,
                    endDate,
                    sessionOrderId);

                // Clean session
                HttpContext.Session.Remove("CarId");
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("StartDate");
                HttpContext.Session.Remove("EndDate");
                HttpContext.Session.Remove("OrderId");
                HttpContext.Session.Remove("Price");

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("MyBookings", "Booking")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during payment capture");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
        public class CaptureRequest
        {
            public string OrderId { get; set; }
        }
    }
}