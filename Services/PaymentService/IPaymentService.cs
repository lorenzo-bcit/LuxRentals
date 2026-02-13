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

        public PayPalPaymentService(
            HttpClient httpClient,
            IOptions<PaypalOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> CreateOrderAsync(decimal amount, string currency)
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

        public async Task CaptureOrderAsync(string orderId)
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

        private async Task<string> GetAccessTokenAsync()
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
    }
}
