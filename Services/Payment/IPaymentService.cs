using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LuxRentals.Services.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(decimal amount, string currency);

        Task<bool> CaptureOrderAsync(string orderId);
    }

    public class PayPalPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaypalOptions _options;
        private readonly ILogger<PayPalPaymentService> _logger;
        private readonly LuxRentalsDbContext _db;

        private string? _accessToken;
        private DateTime _tokenExpiry;

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

            var baseUrl = _options.Environment.ToLower() == "live"
                ? "https://api-m.paypal.com"
                : "https://api-m.sandbox.paypal.com";

            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                {
                    return _accessToken;
                }

                var authToken = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}")
                );

                var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Basic", authToken);

                request.Content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

                var response = await _httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);

                return doc.RootElement
                    .GetProperty("access_token")
                    .GetString()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain PayPal access token");
                throw;
            }
        }


        public async Task<string> CreateOrderAsync(decimal amount, string currency)
        {
            try
            {
                var token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
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

                var orderId = JsonDocument.Parse(json)
                    .RootElement
                    .GetProperty("id")
                    .GetString();

                return orderId!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayPal order");
                throw;
            }
        }

        // Create a PayPal order
        public async Task<bool> CaptureOrderAsync(string orderId)
        {
                    var token = await GetAccessTokenAsync();
                    Console.WriteLine("Access Token: {0}", token);
                    var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"/v2/checkout/orders/{orderId}/capture");


                    request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

                    request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("PayPal capture response: {json}", json);

            if (!response.IsSuccessStatusCode)
                return false;

            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            if (root.TryGetProperty("status", out var rootStatus))
            {
                if (rootStatus.GetString() == "COMPLETED")
                    return true;
            }

            var captureStatus = root
                .GetProperty("purchase_units")[0]
                .GetProperty("payments")
                .GetProperty("captures")[0]
                .GetProperty("status")
                .GetString();

            return captureStatus == "COMPLETED";
        }
        // Get PayPal access token
    }
}