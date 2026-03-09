using Microsoft.AspNetCore.Mvc;
using LuxRentals.Services.Payment;


namespace LuxRentals.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public IActionResult Checkout(string orderId, int bookingId)
        {
            ViewBag.OrderId = orderId;
            ViewBag.BookingId = bookingId;
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
