using LuxRentals.Data;
using LuxRentals.Repositories.Bookings;
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

        public PaymentController(
            IPaymentService paymentService,
            BookingRepo bookingRepo,
            LuxRentalsDbContext context,
            IOptions<PaypalOptions> paypalOptions,
            ILogger<IPaymentService> logger)   
        {
            _paymentService = paymentService;
            _bookingRepo = bookingRepo;
            _db = context;
            _paypalOptions = paypalOptions.Value;
            _logger = logger;
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
                bool paymentSuccess = await _paymentService.CaptureOrderAsync(request.OrderId);

                if (!paymentSuccess)
                {
                    _logger.LogError("Payment failed {Status Code}: ", Response.StatusCode);
                    return Json(new { success = false, message = "Payment failed." });
                }

                int carId = HttpContext.Session.GetInt32("CarId").Value;
                int customerId = HttpContext.Session.GetInt32("CustomerId").Value;

                DateTime startDate = DateTime.Parse(HttpContext.Session.GetString("StartDate"));
                DateTime endDate = DateTime.Parse(HttpContext.Session.GetString("EndDate"));

                var booking = _bookingRepo.CreateBooking(carId, customerId, startDate, endDate, request.OrderId);

                HttpContext.Session.Clear();

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("MyBookings", "Booking")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class CaptureRequest
    {
        public string OrderId { get; set; }
    }
}