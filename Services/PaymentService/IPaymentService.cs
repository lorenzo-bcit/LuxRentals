using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LuxRentals.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(decimal amount, string currency);
        Task CaptureOrderAsync(string orderId);
    }

    public class PayPalPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly PaypalOptions _options;
        private readonly ILogger<PayPalPaymentService> _logger;

        public PayPalPaymentService(
            HttpClient httpClient,
            IOptions<PaypalOptions> options,
            ILogger<PayPalPaymentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
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
                                            value = amount.ToString("F2")
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

        public async Task CaptureOrderAsync(string orderId)
        {
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
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to capture PayPal order with ID {OrderId}", orderId);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse PayPal response for order capture");
                throw;
            }
            catch (Exception ex)
            {
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
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse PayPal access token response");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while obtaining PayPal access token");
                throw;
            }
        }
    }
}
