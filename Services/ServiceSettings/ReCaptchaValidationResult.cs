namespace LuxRentals.Services;

public class ReCaptchaValidationResult
{
        public bool Success { get; set; }
        public string HostName { get; set; }
        public string TimeStamp { get; set; }
        public List<string> ErrorCodes { get; set; }
}