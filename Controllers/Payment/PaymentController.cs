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
            try
            {
                var transactionId = await _paymentService.CaptureOrderAsync(request.OrderId);

                if (transactionId == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Payment failed."
                    });
                }

                // Get session data
                var carId = HttpContext.Session.GetInt32("CarId");
                var customerId = HttpContext.Session.GetInt32("CustomerId");
                var startDateStr = HttpContext.Session.GetString("StartDate");
                var endDateStr = HttpContext.Session.GetString("EndDate");

                if (carId == null || customerId == null || startDateStr == null || endDateStr == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired."
                    });
                }

                var startDate = DateTime.Parse(startDateStr);
                var endDate = DateTime.Parse(endDateStr);
                

                // CREATE BOOKING ONLY AFTER PAYMENT SUCCESS
                var booking = _bookingRepo.CreateBooking(
                    carId.Value,
                    customerId.Value,
                    startDate,
                    endDate,
                    transactionId);

                // Set status to Paid
                _bookingStatusRepo.SetBookingStatus(booking, "booked");

                await _db.SaveChangesAsync();

                HttpContext.Session.Clear();

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("MyBookings", "Booking")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Booking failed: " + ex.Message
                });
            }
        }
        public class CaptureRequest
        {
            public string OrderId { get; set; }
        }
    }
}