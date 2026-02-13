using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LuxRentals.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly ReCaptchaOptions _options;

        public ReCaptchaService(HttpClient httpClient, IOptions<ReCaptchaOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<ReCaptchaValidationResult> ValidateAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ReCaptchaValidationResult { Success = false };
            }

            try
            {
                var values = new[]
                {
                    new KeyValuePair<string, string>("secret", _options.SecretKey),
                    new KeyValuePair<string, string>("response", token)
                };

                using var content = new FormUrlEncodedContent(values);

                var response = await _httpClient.PostAsync("/recaptcha/api/siteverify", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new ReCaptchaValidationResult { Success = false };
                }

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ReCaptchaValidationResult>(json)
                       ?? new ReCaptchaValidationResult { Success = false };
            }
            catch
            {
                // Network failure, timeout, or deserialization error
                return new ReCaptchaValidationResult { Success = false };
            }
        }
    }
}