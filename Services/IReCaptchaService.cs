namespace LuxRentals.Services
{
    public interface IReCaptchaService
    {
        Task<ReCaptchaValidationResult> ValidateAsync(string token);
    }
}