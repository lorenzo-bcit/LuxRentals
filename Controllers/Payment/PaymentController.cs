using LuxRentals.Repositories.Bookings;
using LuxRentals.Services.Payment;
using Microsoft.AspNetCore.Mvc;


namespace LuxRentals.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly BookingRepo _bookingRepo;

        public PaymentController(IPaymentService paymentService, BookingRepo bookingRepo)
        {
            _paymentService = paymentService;
            _bookingRepo = bookingRepo;
        }

        public IActionResult Checkout(int bookingId, string orderId)
        {
            var booking = _bookingRepo.GetBookingById(bookingId);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var price = _bookingRepo.CalculateBookingPrice(
                booking.FkCarId,
                booking.StartDateTime,
                booking.EndDateTime);

            ViewBag.BookingId = booking.PkBookingId;
            ViewBag.OrderId = orderId;
            ViewBag.StartDate = booking.StartDateTime.ToShortDateString();
            ViewBag.EndDate = booking.EndDateTime.ToShortDateString();
            ViewBag.Price = price;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Capture(string orderId, int bookingId)
        {
            await _paymentService.CaptureOrderAsync(orderId, bookingId);
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
