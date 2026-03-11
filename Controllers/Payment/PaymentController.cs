using LuxRentals.Data;
using LuxRentals.Repositories.Bookings;
using LuxRentals.Services.Payment;
using Microsoft.AspNetCore.Mvc;

namespace LuxRentals.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly BookingRepo _bookingRepo;
        private readonly LuxRentalsDbContext _db;

        public PaymentController(IPaymentService paymentService, BookingRepo bookingRepo, LuxRentalsDbContext context)
        {
            _paymentService = paymentService;
            _bookingRepo = bookingRepo;
            _db = context;
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

            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

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
                int carId = HttpContext.Session.GetInt32("CarId") ?? 0;
                int customerId = HttpContext.Session.GetInt32("CustomerId") ?? 0;
                if (carId == 0 || customerId == 0)
                    return BadRequest("Session expired or invalid.");

                DateTime startDate = DateTime.Parse(HttpContext.Session.GetString("StartDate")!);
                DateTime endDate = DateTime.Parse(HttpContext.Session.GetString("EndDate")!);

                var booking = _bookingRepo.CreateBooking(carId, customerId, startDate, endDate);

                var captureId = await _paymentService.CaptureOrderAsync(request.OrderId, booking.PkBookingId);

                if (string.IsNullOrEmpty(captureId))
                {
                    return BadRequest("Payment failed.");
                }


                booking.FkBookingStatusId = 2; 
                _bookingRepo.SaveChanges();     

                // 5️⃣ Clear session
                HttpContext.Session.Remove("CarId");
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("StartDate");
                HttpContext.Session.Remove("EndDate");

                TempData["Success"] = "Your booking was completed successfully!";

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("MyBookings", "Booking")
                });
            }
            catch (Exception ex)
            {
                // Optional: log exception
                return BadRequest(ex.Message);
            }
        }
    }

    public class CaptureRequest
    {
        public string OrderId { get; set; }
    }
}