using LuxRentals.Data;
using LuxRentals.Repositories.Bookings;
using LuxRentals.Repositories.BookingStatus;
using LuxRentals.Services.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LuxRentals.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly BookingRepo _bookingRepo;
        private readonly LuxRentalsDbContext _db;
        private readonly PaypalOptions _paypalOptions;
        private readonly ILogger<IPaymentService> _logger;
        private readonly BookingStatusRepo _bookingStatusRepo;

        public PaymentController(
            IPaymentService paymentService,
            BookingRepo bookingRepo,
            LuxRentalsDbContext context,
            IOptions<PaypalOptions> paypalOptions,
            ILogger<IPaymentService> logger,
            BookingStatusRepo bookingStatusRepo)
        {
            _paymentService = paymentService;
            _bookingRepo = bookingRepo;
            _db = context;
            _paypalOptions = paypalOptions.Value;
            _logger = logger;
            _bookingStatusRepo = bookingStatusRepo;
        }

        public IActionResult Checkout(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                return RedirectToAction("Index", "Home");
            }

            int? carId = HttpContext.Session.GetInt32("CarId");
            string startDateStr = HttpContext.Session.GetString("StartDate");
            string endDateStr = HttpContext.Session.GetString("EndDate");

            if (carId == null || startDateStr == null || endDateStr == null)
            {
                TempData["Error"] = "Booking session expired.";
                return RedirectToAction("Index", "Home");
            }

            var clientId = _paypalOptions.ClientId;

            if (string.IsNullOrEmpty(clientId))
            {
                throw new Exception("PayPal ClientId not configured.");
            }

            ViewBag.PayPalClientId = clientId;

            if (!DateTime.TryParse(startDateStr, out DateTime startDate) ||
                !DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                TempData["Error"] = "Invalid booking dates.";
                return RedirectToAction("Index", "Home");
            }

            var price = _bookingRepo.CalculateBookingPrice(carId.Value, startDate, endDate);

            ViewBag.OrderId = orderId;
            ViewBag.StartDate = startDate.ToShortDateString();
            ViewBag.EndDate = endDate.ToShortDateString();
            ViewBag.Price = price;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Capture([FromBody] CaptureRequest request)
        {
            if (string.IsNullOrEmpty(request?.OrderId))
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid payment request."
                });
            }

            try
            {
                // ✅ STEP 1: Capture PayPal payment
                var captureId = await _paymentService.CaptureOrderAsync(request.OrderId);

                if (captureId == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Payment was not completed. Please try again."
                    });
                }

                // ✅ STEP 2: Validate session
                int? carId = HttpContext.Session.GetInt32("CarId");
                int? customerId = HttpContext.Session.GetInt32("CustomerId");
                string startStr = HttpContext.Session.GetString("StartDate");
                string endStr = HttpContext.Session.GetString("EndDate");

                if (carId == null || customerId == null || startStr == null || endStr == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired. Please try booking again.",
                        redirectUrl = Url.Action("Create", "Booking")
                    });
                }

                DateTime startDate = DateTime.Parse(startStr);
                DateTime endDate = DateTime.Parse(endStr);

                // ✅ STEP 3: Create booking
                try
                {
                    var booking = await _bookingRepo.CreateBooking(
                        carId.Value,
                        customerId.Value,
                        startDate,
                        endDate,
                        captureId
                    );

                    HttpContext.Session.Clear();

                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("MyBookings", "Booking")
                    });
                }
                catch (Exception bookingEx)
                {
                    _logger.LogError(bookingEx, "Booking failed AFTER successful payment");

                    return Json(new
                    {
                        success = false,
                        message = "Payment succeeded, but booking failed: " + bookingEx.Message,
                        redirectUrl = Url.Action("Create", "Booking", new { carId = carId })
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment capture failed");

                return Json(new
                {
                    success = false,
                    message = "Unexpected error during payment. Please try again."
                });
            }
        }
        public class CaptureRequest
        {
            public string OrderId { get; set; }
        }
    }
}