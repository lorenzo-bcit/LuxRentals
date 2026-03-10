using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LuxRentals.Services.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(decimal amount, string currency);
        Task CaptureOrderAsync(string orderId, int bookingId);
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
            _logger = logger;
            _options = options.Value;
            _db = db;
        }

        public async Task<string> CreateOrderAsync(decimal amount, string currency)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/v2/checkout/orders");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

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

                return JsonDocument.Parse(json)
                    .RootElement.GetProperty("id")
                    .GetString()!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to create PayPal order for amount {Amount} {Currency}",
                     amount, currency);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse PayPal response");
                throw;
            }


        }

        public async Task CaptureOrderAsync(string orderId, int bookingId)
        {
            using var dbTransaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"/v2/checkout/orders/{orderId}/capture");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Get capture details from PayPal response
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

                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (HttpRequestException ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Failed to capture PayPal order with ID {OrderId}", orderId);
                throw;
            }
            catch (JsonException ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Failed to parse PayPal response for order capture");
                throw;
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error during PayPal order capture for order ID {OrderId}", orderId);
                throw;
            }
        }
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var authToken = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(
                        $"{_options.ClientId}:{_options.ClientSecret}"));

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/v1/oauth2/token");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", authToken);

                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonDocument.Parse(json)
                    .RootElement.GetProperty("access_token")
                    .GetString()!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to obtain PayPal access token");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while obtaining PayPal access token");
                throw;
            }
        }
        public async Task CaptureOrderAndStorePayment(int bookingId, string orderId)
        {
            using var dbTransaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"/v2/checkout/orders/{orderId}/capture");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var capture = doc.RootElement
                    .GetProperty("payments")
                    .GetProperty("captures")[0];

                var captureId = capture
                    .GetProperty("id")
                    .GetString()!;

                var amount = decimal.Parse(
                    capture.GetProperty("amount")
                           .GetProperty("value")
                           .GetString()!,
                    CultureInfo.InvariantCulture);

                var currency = capture
                    .GetProperty("amount")
                    .GetProperty("currency_code")
                    .GetString()!;

                var exists = await _db.ProviderPayments
                    .AnyAsync(p => p.PaymentProviderCaptureId == captureId);

                if (exists)
                {
                    _logger.LogInformation("Capture already processed: {CaptureId}", captureId);
                    return;
                }

                var transaction = new Transaction
                {
                    AmountPaid = amount,
                    PaymentDate = DateTime.UtcNow,
                    FkBookingId = bookingId
                };

                _db.Transactions.Add(transaction);
                await _db.SaveChangesAsync();

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
                var booking = await _db.Bookings.FindAsync(bookingId);
                if (booking != null)
                {
                    booking.FkBookingStatusId = 2;
                }

                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Error storing PayPal payment for order {OrderId}", orderId);
                throw;
            }
        }
    }
}
