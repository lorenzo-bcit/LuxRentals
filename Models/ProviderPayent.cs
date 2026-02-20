namespace LuxRentals.Models
{
    public class Payment
    {
        public int PkPaymentId { get; set; }
        public int FkTransactionId { get; set; }
        public Transaction Transaction { get; set; } = null!;
        public string PaymentProvider { get; set; } = "PayPal";
        public string PaymentProviderOrderId { get; set; } = null!;
        public string PaymentProviderCaptureId { get; set; } = null!;
        public PaymentStatus Status { get; set; }
        public DateTime RecivedAt { get; set; }
        public string? RawWebHookJson { get; set; }
    }
}
