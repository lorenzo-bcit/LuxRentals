namespace LuxRentals.Services.Payment
{
    public class PaypalOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Environment { get; set; } = "sandbox"; // or "live"
    }
}
