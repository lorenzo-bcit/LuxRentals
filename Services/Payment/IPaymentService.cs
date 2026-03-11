using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LuxRentals.Services.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(decimal amount, string currency);
        Task<string> CaptureOrderAsync(string orderId, int bookingId);
    }

    public class PayPalPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaypalOptions _options;
        private readonly ILogger<PayPalPaymentService> _logger;
        private readonly LuxRentalsDbContext _db;

        public PayPalPaymentService(
            HttpClient httpClient,
            IOptions<PaypalOptions> options,
            ILogger<PayPalPaymentService> logger,
            LuxRentalsDbContext db)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            _db = db;
        }

        // Create a PayPal order
        public async Task<string> CreateOrderAsync(decimal amount, string currency)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                request.Content = JsonContent.Create(new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                currency_code = currency,
                                value = amount.ToString("F2", CultureInfo.InvariantCulture)
                            }
                        }
                    }
                });

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                return doc.RootElement.GetProperty("id").GetString()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayPal order for {Amount} {Currency}", amount, currency);
                throw;
            }
        }

        // Capture a PayPal order and store payment in DB
        public async Task<string> CaptureOrderAsync(string orderId, int bookingId)
        {
            using var dbTransaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{orderId}/capture");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var capture = doc.RootElement
                    .GetProperty("payments")
                    .GetProperty("captures")[0];

                var captureId = capture.GetProperty("id").GetString()!;
                var amount = decimal.Parse(
                    capture.GetProperty("amount")
                           .GetProperty("value")
                           .GetString()!,
                    CultureInfo.InvariantCulture);

                // Save transaction
                var transaction = new Transaction
                {
                    AmountPaid = amount,
                    PaymentDate = DateTime.UtcNow,
                    FkBookingId = bookingId
                };
                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync();

                // Save provider payment
                var providerPayment = new ProviderPayment
                {
                    FkTransactionId = transaction.PkTransactionId,
                    PaymentProvider = "PayPal",
                    PaymentProviderOrderId = orderId,
                    PaymentProviderCaptureId = captureId,
                    ReceivedAt = DateTime.UtcNow,
                    RawWebHookJson = json
                };
                _db.ProviderPayments.Add(providerPayment);

                // Update booking status
                var booking = await _db.Bookings.FindAsync(bookingId);
                if (booking != null)
                {
                    booking.FkBookingStatusId = 2; // Mark as paid
                }

                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return captureId;
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Failed to capture PayPal order {OrderId} for booking {BookingId}", orderId, bookingId);
                throw;
            }
        }

        // Get PayPal access token
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

                var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                return doc.RootElement.GetProperty("access_token").GetString()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain PayPal access token");
                throw;
            }
        }
    }
}