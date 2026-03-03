namespace LuxRentals.Services;

public class ReCaptchaValidationResult
{
    public bool Success { get; set; }
    public string HostName { get; set; } = null!;
        public string TimeStamp { get; set; } = null!;
        public List<string> ErrorCodes { get; set; } = null!;
}